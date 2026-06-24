using ISS.Domain.Common;

namespace ISS.Domain.Inventory;

public enum ReplenishmentRunStatus
{
    Draft = 1,
    Converted = 2,
    Cancelled = 3
}

public enum ReplenishmentLineStatus
{
    NotRequired = 1,
    ReorderRequired = 2,
    CriticalStock = 3,
    Overstock = 4,
    PendingPurchase = 5,
    Excluded = 6
}

public sealed class ReplenishmentRun : AuditableEntity
{
    private readonly List<ReplenishmentRunLine> _lines = [];

    private ReplenishmentRun() { }

    public ReplenishmentRun(string number, Guid warehouseId, DateTimeOffset calculatedAt, string? notes)
    {
        Number = Guard.NotNullOrWhiteSpace(number, nameof(number), maxLength: 32);
        WarehouseId = warehouseId == Guid.Empty ? throw new DomainValidationException("Warehouse is required.") : warehouseId;
        CalculatedAt = calculatedAt;
        Notes = notes?.Trim();
        Status = ReplenishmentRunStatus.Draft;
    }

    public string Number { get; private set; } = null!;
    public Guid WarehouseId { get; private set; }
    public DateTimeOffset CalculatedAt { get; private set; }
    public string? Notes { get; private set; }
    public ReplenishmentRunStatus Status { get; private set; }
    public Guid? PurchaseRequisitionId { get; private set; }
    public IReadOnlyCollection<ReplenishmentRunLine> Lines => _lines;

    public void AddLine(
        Guid itemId,
        decimal onHand,
        decimal openPurchaseQuantity,
        decimal reorderPoint,
        decimal reorderQuantity,
        decimal recommendedQuantity,
        ReplenishmentLineStatus status,
        Guid? preferredSupplierId,
        Guid? preferredPackId,
        decimal? recommendedPackQuantity)
        => _lines.Add(new ReplenishmentRunLine(
            itemId,
            onHand,
            openPurchaseQuantity,
            reorderPoint,
            reorderQuantity,
            recommendedQuantity,
            status,
            preferredSupplierId,
            preferredPackId,
            recommendedPackQuantity));

    public void MarkConverted(Guid purchaseRequisitionId)
    {
        if (Status != ReplenishmentRunStatus.Draft)
        {
            throw new DomainValidationException("Only draft replenishment runs can be converted.");
        }

        PurchaseRequisitionId = purchaseRequisitionId == Guid.Empty ? throw new DomainValidationException("Purchase requisition is required.") : purchaseRequisitionId;
        Status = ReplenishmentRunStatus.Converted;
    }
}

public sealed class ReplenishmentRunLine : AuditableEntity
{
    private ReplenishmentRunLine() { }

    internal ReplenishmentRunLine(
        Guid itemId,
        decimal onHand,
        decimal openPurchaseQuantity,
        decimal reorderPoint,
        decimal reorderQuantity,
        decimal recommendedQuantity,
        ReplenishmentLineStatus status,
        Guid? preferredSupplierId,
        Guid? preferredPackId,
        decimal? recommendedPackQuantity)
    {
        ItemId = itemId == Guid.Empty ? throw new DomainValidationException("Item is required.") : itemId;
        OnHand = onHand;
        OpenPurchaseQuantity = Guard.NotNegative(openPurchaseQuantity, nameof(openPurchaseQuantity));
        ReorderPoint = Guard.NotNegative(reorderPoint, nameof(reorderPoint));
        ReorderQuantity = Guard.NotNegative(reorderQuantity, nameof(reorderQuantity));
        RecommendedQuantity = Guard.NotNegative(recommendedQuantity, nameof(recommendedQuantity));
        Status = status;
        PreferredSupplierId = preferredSupplierId;
        PreferredPackId = preferredPackId;
        RecommendedPackQuantity = recommendedPackQuantity;
    }

    public Guid ReplenishmentRunId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal OnHand { get; private set; }
    public decimal OpenPurchaseQuantity { get; private set; }
    public decimal ReorderPoint { get; private set; }
    public decimal ReorderQuantity { get; private set; }
    public decimal RecommendedQuantity { get; private set; }
    public ReplenishmentLineStatus Status { get; private set; }
    public Guid? PreferredSupplierId { get; private set; }
    public Guid? PreferredPackId { get; private set; }
    public decimal? RecommendedPackQuantity { get; private set; }
}
