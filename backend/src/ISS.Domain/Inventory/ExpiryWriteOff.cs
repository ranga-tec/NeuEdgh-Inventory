using ISS.Domain.Common;

namespace ISS.Domain.Inventory;

public enum ExpiryWriteOffStatus
{
    Draft = 1,
    Approved = 2,
    Posted = 3,
    Voided = 4
}

public sealed class ExpiryWriteOff : AuditableEntity
{
    private readonly List<ExpiryWriteOffLine> _lines = [];

    private ExpiryWriteOff() { }

    public ExpiryWriteOff(string number, Guid warehouseId, string? notes)
    {
        Number = Guard.NotNullOrWhiteSpace(number, nameof(number), maxLength: 32);
        WarehouseId = warehouseId == Guid.Empty ? throw new DomainValidationException("Warehouse is required.") : warehouseId;
        Notes = notes?.Trim();
        Status = ExpiryWriteOffStatus.Draft;
    }

    public string Number { get; private set; } = null!;
    public Guid WarehouseId { get; private set; }
    public string? Notes { get; private set; }
    public ExpiryWriteOffStatus Status { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public DateTimeOffset? VoidedAt { get; private set; }
    public string? VoidReason { get; private set; }
    public IReadOnlyCollection<ExpiryWriteOffLine> Lines => _lines;

    public void AddLine(Guid stockLayerId, Guid itemId, decimal quantity, decimal unitCost, string? reason)
    {
        if (Status != ExpiryWriteOffStatus.Draft)
        {
            throw new DomainValidationException("Only draft write-offs can be edited.");
        }

        _lines.Add(new ExpiryWriteOffLine(stockLayerId, itemId, quantity, unitCost, reason));
    }

    public void Approve(DateTimeOffset approvedAt)
    {
        if (Status != ExpiryWriteOffStatus.Draft)
        {
            throw new DomainValidationException("Only draft write-offs can be approved.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainValidationException("Write-off must have at least one line.");
        }

        Status = ExpiryWriteOffStatus.Approved;
        ApprovedAt = approvedAt;
    }

    public void Post(DateTimeOffset postedAt)
    {
        if (Status != ExpiryWriteOffStatus.Approved)
        {
            throw new DomainValidationException("Only approved write-offs can be posted.");
        }

        Status = ExpiryWriteOffStatus.Posted;
        PostedAt = postedAt;
    }

    public void Void(DateTimeOffset voidedAt, string reason)
    {
        if (Status == ExpiryWriteOffStatus.Posted)
        {
            throw new DomainValidationException("Posted write-offs cannot be voided.");
        }

        Status = ExpiryWriteOffStatus.Voided;
        VoidedAt = voidedAt;
        VoidReason = Guard.NotNullOrWhiteSpace(reason, nameof(reason), maxLength: 512);
    }
}

public sealed class ExpiryWriteOffLine : AuditableEntity
{
    private ExpiryWriteOffLine() { }

    internal ExpiryWriteOffLine(Guid stockLayerId, Guid itemId, decimal quantity, decimal unitCost, string? reason)
    {
        StockLayerId = stockLayerId == Guid.Empty ? throw new DomainValidationException("Stock layer is required.") : stockLayerId;
        ItemId = itemId == Guid.Empty ? throw new DomainValidationException("Item is required.") : itemId;
        Quantity = Guard.Positive(quantity, nameof(quantity));
        UnitCost = Guard.NotNegative(unitCost, nameof(unitCost));
        Reason = reason?.Trim();
    }

    public Guid ExpiryWriteOffId { get; private set; }
    public Guid StockLayerId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public string? Reason { get; private set; }
}
