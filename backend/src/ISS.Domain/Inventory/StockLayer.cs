using ISS.Domain.Common;

namespace ISS.Domain.Inventory;

public enum StockLayerStatus
{
    Available = 1,
    Blocked = 2,
    WrittenOff = 3
}

public sealed class StockLayer : AuditableEntity
{
    private StockLayer() { }

    public StockLayer(
        Guid warehouseId,
        Guid? warehouseBinId,
        Guid itemId,
        decimal quantity,
        decimal unitCost,
        DateTimeOffset receivedAt,
        string referenceType,
        Guid referenceId,
        Guid? referenceLineId,
        string? batchNumber,
        DateOnly? expiryDate)
    {
        WarehouseId = warehouseId == Guid.Empty ? throw new DomainValidationException("Warehouse is required.") : warehouseId;
        WarehouseBinId = warehouseBinId;
        ItemId = itemId == Guid.Empty ? throw new DomainValidationException("Item is required.") : itemId;
        OriginalQuantity = Guard.Positive(quantity, nameof(quantity));
        RemainingQuantity = quantity;
        UnitCost = Guard.NotNegative(unitCost, nameof(unitCost));
        ReceivedAt = receivedAt;
        ReferenceType = Guard.NotNullOrWhiteSpace(referenceType, nameof(referenceType), maxLength: 64);
        ReferenceId = referenceId == Guid.Empty ? throw new DomainValidationException("Reference is required.") : referenceId;
        ReferenceLineId = referenceLineId;
        BatchNumber = batchNumber?.Trim();
        ExpiryDate = expiryDate;
        Status = StockLayerStatus.Available;
    }

    public Guid WarehouseId { get; private set; }
    public Guid? WarehouseBinId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal OriginalQuantity { get; private set; }
    public decimal RemainingQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
    public string ReferenceType { get; private set; } = null!;
    public Guid ReferenceId { get; private set; }
    public Guid? ReferenceLineId { get; private set; }
    public string? BatchNumber { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public StockLayerStatus Status { get; private set; }

    public void Consume(decimal quantity)
    {
        quantity = Guard.Positive(quantity, nameof(quantity));
        if (Status != StockLayerStatus.Available)
        {
            throw new DomainValidationException("Stock layer is not available.");
        }

        if (RemainingQuantity < quantity)
        {
            throw new DomainValidationException("Insufficient quantity in stock layer.");
        }

        RemainingQuantity -= quantity;
        if (RemainingQuantity == 0m)
        {
            Status = StockLayerStatus.Blocked;
        }
    }

    public void WriteOff(decimal quantity)
    {
        Consume(quantity);
        if (RemainingQuantity == 0m)
        {
            Status = StockLayerStatus.WrittenOff;
        }
    }

    public void Block() => Status = StockLayerStatus.Blocked;
    public void Unblock()
    {
        if (RemainingQuantity <= 0m)
        {
            throw new DomainValidationException("Empty stock layer cannot be unblocked.");
        }

        Status = StockLayerStatus.Available;
    }
}
