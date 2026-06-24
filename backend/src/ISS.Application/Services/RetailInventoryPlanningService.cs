using ISS.Application.Abstractions;
using ISS.Application.Common;
using ISS.Application.Persistence;
using ISS.Domain.Common;
using ISS.Domain.Inventory;
using ISS.Domain.MasterData;
using ISS.Domain.Procurement;
using ISS.Domain.Retail;
using Microsoft.EntityFrameworkCore;

namespace ISS.Application.Services;

public sealed class RetailInventoryPlanningService(
    IIssDbContext dbContext,
    IDocumentNumberService documentNumberService,
    InventoryService inventoryService,
    ProcurementService procurementService,
    IClock clock)
{
    public sealed record FifoAllocation(Guid StockLayerId, Guid ItemId, decimal Quantity, decimal UnitCost, string? BatchNumber, DateOnly? ExpiryDate);
    public sealed record BundleAvailability(Guid BundleId, decimal AvailableQuantity);
    public sealed record PromotionEvaluationResult(Guid PromotionId, string Code, string Name, PromotionType Type, decimal DiscountAmount, decimal? SpecialPrice);
    public sealed record ReplenishmentRecommendation(
        Guid WarehouseId,
        Guid ItemId,
        decimal OnHand,
        decimal OpenPurchaseQuantity,
        decimal ReorderPoint,
        decimal ReorderQuantity,
        decimal RecommendedQuantity,
        ReplenishmentLineStatus Status,
        Guid? PreferredPackId,
        decimal? RecommendedPackQuantity);

    public async Task<ItemPack> CreatePackAsync(
        Guid itemId,
        string code,
        string name,
        string barcode,
        decimal baseQuantity,
        string unitOfMeasure,
        decimal price,
        bool purchaseAllowed,
        bool saleAllowed,
        bool transferAllowed,
        bool isDefaultPurchasePack,
        bool isDefaultSalesPack,
        Guid? taxCodeId,
        CancellationToken cancellationToken = default)
    {
        await EnsureItemExistsAsync(itemId, cancellationToken);
        await EnsurePackBarcodeAvailableAsync(barcode, null, cancellationToken);

        var pack = new ItemPack(itemId, code, name, barcode, baseQuantity, unitOfMeasure, price, purchaseAllowed, saleAllowed, transferAllowed, isDefaultPurchasePack, isDefaultSalesPack, taxCodeId);
        await dbContext.ItemPacks.AddAsync(pack, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return pack;
    }

    public async Task UpdatePackAsync(
        Guid id,
        string code,
        string name,
        string barcode,
        decimal baseQuantity,
        string unitOfMeasure,
        decimal price,
        bool purchaseAllowed,
        bool saleAllowed,
        bool transferAllowed,
        bool isDefaultPurchasePack,
        bool isDefaultSalesPack,
        Guid? taxCodeId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var pack = await dbContext.ItemPacks.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                   ?? throw new NotFoundException("Pack not found.");

        await EnsurePackBarcodeAvailableAsync(barcode, id, cancellationToken);
        pack.Update(code, name, barcode, baseQuantity, unitOfMeasure, price, purchaseAllowed, saleAllowed, transferAllowed, isDefaultPurchasePack, isDefaultSalesPack, taxCodeId, isActive);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<StockLayer> CreateStockLayerAsync(
        Guid warehouseId,
        Guid? warehouseBinId,
        Guid itemId,
        decimal quantity,
        decimal unitCost,
        string referenceType,
        Guid referenceId,
        Guid? referenceLineId,
        string? batchNumber,
        DateOnly? expiryDate,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.Items.AsNoTracking().FirstOrDefaultAsync(x => x.Id == itemId, cancellationToken)
                   ?? throw new NotFoundException("Item not found.");

        ValidateExpiryTracking(item, batchNumber, expiryDate);

        var layer = new StockLayer(warehouseId, warehouseBinId, itemId, quantity, unitCost, clock.UtcNow, referenceType, referenceId, referenceLineId, batchNumber, expiryDate);
        await dbContext.StockLayers.AddAsync(layer, cancellationToken);
        await inventoryService.RecordReceiptAsync(clock.UtcNow, warehouseId, item, quantity, unitCost, referenceType, referenceId, referenceLineId, batchNumber, serialNumbers: null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return layer;
    }

    public async Task<IReadOnlyList<FifoAllocation>> AllocateAsync(
        Guid warehouseId,
        Guid itemId,
        decimal quantity,
        ItemIssueMethod issueMethod,
        CancellationToken cancellationToken = default)
    {
        quantity = Guard.Positive(quantity, nameof(quantity));

        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var query = dbContext.StockLayers
            .Where(x => x.WarehouseId == warehouseId
                        && x.ItemId == itemId
                        && x.Status == StockLayerStatus.Available
                        && x.RemainingQuantity > 0m
                        && (x.ExpiryDate == null || x.ExpiryDate >= today));

        query = issueMethod == ItemIssueMethod.Fefo
            ? query.OrderBy(x => x.ExpiryDate ?? DateOnly.MaxValue).ThenBy(x => x.ReceivedAt)
            : query.OrderBy(x => x.ReceivedAt).ThenBy(x => x.ExpiryDate ?? DateOnly.MaxValue);

        var layers = await query.ToListAsync(cancellationToken);
        var remaining = quantity;
        var allocations = new List<FifoAllocation>();

        foreach (var layer in layers)
        {
            if (remaining <= 0m)
            {
                break;
            }

            var allocated = Math.Min(layer.RemainingQuantity, remaining);
            allocations.Add(new FifoAllocation(layer.Id, layer.ItemId, allocated, layer.UnitCost, layer.BatchNumber, layer.ExpiryDate));
            remaining -= allocated;
        }

        if (remaining > 0m)
        {
            throw new DomainValidationException("Insufficient valid stock layers for FIFO/FEFO allocation.");
        }

        return allocations;
    }

    public async Task<IReadOnlyList<StockLayer>> GetNearExpiryAsync(Guid? warehouseId, int days, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var until = today.AddDays(Math.Max(days, 0));
        var query = dbContext.StockLayers.AsNoTracking()
            .Where(x => x.ExpiryDate != null
                        && x.ExpiryDate >= today
                        && x.ExpiryDate <= until
                        && x.RemainingQuantity > 0m
                        && x.Status == StockLayerStatus.Available);

        if (warehouseId is not null && warehouseId != Guid.Empty)
        {
            query = query.Where(x => x.WarehouseId == warehouseId);
        }

        return await query.OrderBy(x => x.ExpiryDate).ThenBy(x => x.ItemId).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockLayer>> GetExpiredAsync(Guid? warehouseId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var query = dbContext.StockLayers.AsNoTracking()
            .Where(x => x.ExpiryDate != null
                        && x.ExpiryDate < today
                        && x.RemainingQuantity > 0m
                        && x.Status == StockLayerStatus.Available);

        if (warehouseId is not null && warehouseId != Guid.Empty)
        {
            query = query.Where(x => x.WarehouseId == warehouseId);
        }

        return await query.OrderBy(x => x.ExpiryDate).ThenBy(x => x.ItemId).ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateExpiryWriteOffAsync(Guid warehouseId, IReadOnlyCollection<(Guid StockLayerId, decimal Quantity, string? Reason)> lines, string? notes, CancellationToken cancellationToken = default)
    {
        if (lines.Count == 0)
        {
            throw new DomainValidationException("At least one write-off line is required.");
        }

        var number = await documentNumberService.NextAsync("EXPWO", "EXPWO", cancellationToken);
        var writeOff = new ExpiryWriteOff(number, warehouseId, notes);

        foreach (var input in lines)
        {
            var layer = await dbContext.StockLayers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == input.StockLayerId && x.WarehouseId == warehouseId, cancellationToken)
                        ?? throw new NotFoundException("Stock layer not found.");

            if (layer.RemainingQuantity < input.Quantity)
            {
                throw new DomainValidationException("Write-off quantity exceeds stock layer remaining quantity.");
            }

            writeOff.AddLine(layer.Id, layer.ItemId, input.Quantity, layer.UnitCost, input.Reason);
        }

        await dbContext.ExpiryWriteOffs.AddAsync(writeOff, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return writeOff.Id;
    }

    public async Task ApproveExpiryWriteOffAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var writeOff = await dbContext.ExpiryWriteOffs.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                       ?? throw new NotFoundException("Expiry write-off not found.");

        writeOff.Approve(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PostExpiryWriteOffAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var writeOff = await dbContext.ExpiryWriteOffs.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                       ?? throw new NotFoundException("Expiry write-off not found.");

        foreach (var line in writeOff.Lines)
        {
            var layer = await dbContext.StockLayers.FirstOrDefaultAsync(x => x.Id == line.StockLayerId, cancellationToken)
                        ?? throw new NotFoundException("Stock layer not found.");
            var item = await dbContext.Items.FirstAsync(x => x.Id == line.ItemId, cancellationToken);

            layer.WriteOff(line.Quantity);
            await inventoryService.RecordAdjustmentAsync(clock.UtcNow, writeOff.WarehouseId, item, -line.Quantity, line.UnitCost, "ExpiryWriteOff", writeOff.Id, line.Id, layer.BatchNumber, serialNumbers: null, cancellationToken);
        }

        writeOff.Post(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<BundleAvailability> GetBundleAvailabilityAsync(Guid bundleId, Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var bundle = await dbContext.Bundles.AsNoTracking().Include(x => x.Components).FirstOrDefaultAsync(x => x.Id == bundleId, cancellationToken)
                     ?? throw new NotFoundException("Bundle not found.");

        var requiredComponents = bundle.Components.Where(x => !x.IsOptional).ToList();
        if (requiredComponents.Count == 0)
        {
            return new BundleAvailability(bundleId, 0m);
        }

        decimal? available = null;
        foreach (var component in requiredComponents)
        {
            var onHand = await inventoryService.GetOnHandAsync(warehouseId, component.ItemId, cancellationToken: cancellationToken);
            var componentAvailable = Math.Floor(onHand / component.Quantity);
            available = available is null ? componentAvailable : Math.Min(available.Value, componentAvailable);
        }

        return new BundleAvailability(bundleId, available ?? 0m);
    }

    public async Task<IReadOnlyList<ReplenishmentRecommendation>> CalculateReplenishmentAsync(Guid warehouseId, CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.ReorderSettings.AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId)
            .OrderBy(x => x.ItemId)
            .ToListAsync(cancellationToken);

        var recommendations = new List<ReplenishmentRecommendation>();
        foreach (var setting in settings)
        {
            var onHand = await inventoryService.GetOnHandAsync(warehouseId, setting.ItemId, cancellationToken: cancellationToken);
            var openPurchaseQuantity = await dbContext.PurchaseOrders.AsNoTracking()
                .Where(po => po.Status == PurchaseOrderStatus.Approved || po.Status == PurchaseOrderStatus.PartiallyReceived)
                .SelectMany(po => po.Lines)
                .Where(line => line.ItemId == setting.ItemId)
                .SumAsync(line => line.OrderedQuantity - line.ReceivedQuantity, cancellationToken);

            var projected = onHand + openPurchaseQuantity;
            var shortage = setting.ReorderPoint - projected;
            var recommended = shortage <= 0m ? 0m : Math.Max(shortage, setting.ReorderQuantity);
            var status = recommended > 0m
                ? (onHand <= 0m ? ReplenishmentLineStatus.CriticalStock : ReplenishmentLineStatus.ReorderRequired)
                : ReplenishmentLineStatus.NotRequired;

            var defaultPack = await dbContext.ItemPacks.AsNoTracking()
                .Where(x => x.ItemId == setting.ItemId && x.IsActive && x.PurchaseAllowed && x.IsDefaultPurchasePack)
                .OrderByDescending(x => x.BaseQuantity)
                .FirstOrDefaultAsync(cancellationToken);

            decimal? packQty = defaultPack is null || recommended <= 0m
                ? null
                : Math.Ceiling(recommended / defaultPack.BaseQuantity);

            recommendations.Add(new ReplenishmentRecommendation(warehouseId, setting.ItemId, onHand, openPurchaseQuantity, setting.ReorderPoint, setting.ReorderQuantity, recommended, status, defaultPack?.Id, packQty));
        }

        return recommendations;
    }

    public async Task<Guid> CreateReplenishmentRunAsync(Guid warehouseId, string? notes, CancellationToken cancellationToken = default)
    {
        var recommendations = await CalculateReplenishmentAsync(warehouseId, cancellationToken);
        var number = await documentNumberService.NextAsync("REPL", "REPL", cancellationToken);
        var run = new ReplenishmentRun(number, warehouseId, clock.UtcNow, notes);

        foreach (var row in recommendations)
        {
            run.AddLine(row.ItemId, row.OnHand, row.OpenPurchaseQuantity, row.ReorderPoint, row.ReorderQuantity, row.RecommendedQuantity, row.Status, preferredSupplierId: null, row.PreferredPackId, row.RecommendedPackQuantity);
        }

        await dbContext.ReplenishmentRuns.AddAsync(run, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return run.Id;
    }

    public async Task<Guid> ConvertReplenishmentRunToPurchaseRequisitionAsync(Guid runId, bool submit, CancellationToken cancellationToken = default)
    {
        var run = await dbContext.ReplenishmentRuns.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == runId, cancellationToken)
                  ?? throw new NotFoundException("Replenishment run not found.");

        var actionable = run.Lines.Where(x => x.RecommendedQuantity > 0m && x.Status is ReplenishmentLineStatus.ReorderRequired or ReplenishmentLineStatus.CriticalStock).ToList();
        if (actionable.Count == 0)
        {
            throw new DomainValidationException("No replenishment lines require purchasing.");
        }

        var prId = await procurementService.CreatePurchaseRequisitionAsync($"Generated from replenishment run {run.Number}.", cancellationToken);
        foreach (var line in actionable)
        {
            await procurementService.AddPurchaseRequisitionLineAsync(prId, line.ItemId, line.RecommendedQuantity, "Replenishment recommendation.", cancellationToken);
        }

        if (submit)
        {
            await procurementService.SubmitPurchaseRequisitionAsync(prId, cancellationToken);
        }

        run.MarkConverted(prId);
        await dbContext.SaveChangesAsync(cancellationToken);
        return prId;
    }

    public async Task<IReadOnlyList<PromotionEvaluationResult>> EvaluatePromotionsAsync(Guid companyId, IReadOnlyCollection<(Guid? ItemId, Guid? ItemPackId, Guid? BundleId, Guid? CategoryId, Guid? BrandId, decimal Quantity, decimal UnitPrice)> lines, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var promotions = await dbContext.Promotions.AsNoTracking()
            .Include(x => x.Lines)
            .Where(x => x.CompanyId == companyId && x.Status == PromotionStatus.Active && x.StartsAt <= now && x.EndsAt >= now)
            .OrderByDescending(x => x.Priority)
            .ToListAsync(cancellationToken);

        var results = new List<PromotionEvaluationResult>();
        foreach (var promo in promotions)
        {
            decimal discount = 0m;
            decimal? specialPrice = null;

            foreach (var promoLine in promo.Lines)
            {
                foreach (var saleLine in lines)
                {
                    if (!PromotionLineMatches(promoLine, saleLine))
                    {
                        continue;
                    }

                    var lineTotal = saleLine.Quantity * saleLine.UnitPrice;
                    if (promoLine.SpecialPrice is decimal sp)
                    {
                        specialPrice = sp;
                        discount += Math.Max(0m, lineTotal - (sp * saleLine.Quantity));
                    }
                    else if (promoLine.DiscountPercent is decimal pct)
                    {
                        discount += lineTotal * (pct / 100m);
                    }
                    else if (promoLine.DiscountAmount is decimal amount)
                    {
                        discount += Math.Min(lineTotal, amount);
                    }
                }
            }

            if (promo.MaxDiscountAmount is decimal max)
            {
                discount = Math.Min(discount, max);
            }

            if (discount > 0m || specialPrice is not null)
            {
                results.Add(new PromotionEvaluationResult(promo.Id, promo.Code, promo.Name, promo.Type, decimal.Round(discount, 4), specialPrice));
                if (!promo.IsStackable)
                {
                    break;
                }
            }
        }

        return results;
    }

    private async Task EnsureItemExistsAsync(Guid itemId, CancellationToken cancellationToken)
    {
        if (!await dbContext.Items.AsNoTracking().AnyAsync(x => x.Id == itemId, cancellationToken))
        {
            throw new NotFoundException("Item not found.");
        }
    }

    private async Task EnsurePackBarcodeAvailableAsync(string barcode, Guid? excludingPackId, CancellationToken cancellationToken)
    {
        var trimmed = Guard.NotNullOrWhiteSpace(barcode, nameof(barcode), maxLength: 128);
        var exists = await dbContext.ItemPacks.AsNoTracking()
            .AnyAsync(x => x.Barcode == trimmed && (excludingPackId == null || x.Id != excludingPackId.Value), cancellationToken);
        if (exists)
        {
            throw new DomainValidationException("Pack barcode is already in use.");
        }
    }

    private static void ValidateExpiryTracking(Item item, string? batchNumber, DateOnly? expiryDate)
    {
        if (item.TrackingType is (TrackingType.Batch or TrackingType.BatchAndExpiry) && string.IsNullOrWhiteSpace(batchNumber))
        {
            throw new DomainValidationException("Batch number is required for this item.");
        }

        if (item.TrackingType is (TrackingType.Expiry or TrackingType.BatchAndExpiry) && expiryDate is null)
        {
            throw new DomainValidationException("Expiry date is required for this item.");
        }
    }

    private static bool PromotionLineMatches(PromotionLine promoLine, (Guid? ItemId, Guid? ItemPackId, Guid? BundleId, Guid? CategoryId, Guid? BrandId, decimal Quantity, decimal UnitPrice) saleLine)
        => (promoLine.ItemId is not null && promoLine.ItemId == saleLine.ItemId)
           || (promoLine.ItemPackId is not null && promoLine.ItemPackId == saleLine.ItemPackId)
           || (promoLine.BundleId is not null && promoLine.BundleId == saleLine.BundleId)
           || (promoLine.CategoryId is not null && promoLine.CategoryId == saleLine.CategoryId)
           || (promoLine.BrandId is not null && promoLine.BrandId == saleLine.BrandId);
}
