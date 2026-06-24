using ISS.Domain.Common;

namespace ISS.Domain.Retail;

public enum PromotionType
{
    PercentageDiscount = 1,
    FixedAmountDiscount = 2,
    SpecialPrice = 3,
    BuyXGetY = 4,
    BundlePrice = 5,
    CategoryDiscount = 6,
    BrandDiscount = 7,
    QuantityBreak = 8
}

public enum PromotionStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Active = 4,
    Paused = 5,
    Expired = 6,
    Cancelled = 7
}

public sealed class Promotion : AuditableEntity
{
    private readonly List<PromotionLine> _lines = [];

    private Promotion() { }

    public Promotion(
        Guid companyId,
        string code,
        string name,
        PromotionType type,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        int priority,
        bool isStackable,
        decimal? maxDiscountAmount,
        string? description)
    {
        CompanyId = companyId == Guid.Empty ? throw new DomainValidationException("Company is required.") : companyId;
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code), maxLength: 32);
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
        Type = type;
        if (endsAt <= startsAt)
        {
            throw new DomainValidationException("Promotion end date must be after start date.");
        }

        StartsAt = startsAt;
        EndsAt = endsAt;
        Priority = priority;
        IsStackable = isStackable;
        MaxDiscountAmount = maxDiscountAmount;
        Description = description?.Trim();
        Status = PromotionStatus.Draft;
    }

    public Guid CompanyId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PromotionType Type { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public int Priority { get; private set; }
    public bool IsStackable { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public PromotionStatus Status { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancelReason { get; private set; }
    public IReadOnlyCollection<PromotionLine> Lines => _lines;

    public void Update(string code, string name, PromotionType type, DateTimeOffset startsAt, DateTimeOffset endsAt, int priority, bool isStackable, decimal? maxDiscountAmount, string? description)
    {
        if (Status is PromotionStatus.Active or PromotionStatus.Cancelled or PromotionStatus.Expired)
        {
            throw new DomainValidationException("Active, expired, or cancelled promotions cannot be edited.");
        }

        if (endsAt <= startsAt)
        {
            throw new DomainValidationException("Promotion end date must be after start date.");
        }

        Code = Guard.NotNullOrWhiteSpace(code, nameof(code), maxLength: 32);
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
        Type = type;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Priority = priority;
        IsStackable = isStackable;
        MaxDiscountAmount = maxDiscountAmount;
        Description = description?.Trim();
    }

    public void ReplaceLines(IEnumerable<PromotionLineInput> lines)
    {
        if (Status is PromotionStatus.Active or PromotionStatus.Cancelled or PromotionStatus.Expired)
        {
            throw new DomainValidationException("Active, expired, or cancelled promotions cannot be edited.");
        }

        _lines.Clear();
        foreach (var line in lines)
        {
            _lines.Add(new PromotionLine(line.ItemId, line.ItemPackId, line.BundleId, line.CategoryId, line.BrandId, line.BuyQuantity, line.GetItemId, line.GetQuantity, line.DiscountPercent, line.DiscountAmount, line.SpecialPrice, line.MinimumBasketAmount));
        }
    }

    public void Submit()
    {
        if (Status != PromotionStatus.Draft)
        {
            throw new DomainValidationException("Only draft promotions can be submitted.");
        }

        if (_lines.Count == 0)
        {
            throw new DomainValidationException("Promotion must have at least one line.");
        }

        Status = PromotionStatus.PendingApproval;
    }

    public void Approve(DateTimeOffset approvedAt)
    {
        if (Status != PromotionStatus.PendingApproval)
        {
            throw new DomainValidationException("Only pending promotions can be approved.");
        }

        Status = approvedAt >= StartsAt && approvedAt <= EndsAt ? PromotionStatus.Active : PromotionStatus.Approved;
        ApprovedAt = approvedAt;
    }

    public void Pause()
    {
        if (Status != PromotionStatus.Active)
        {
            throw new DomainValidationException("Only active promotions can be paused.");
        }

        Status = PromotionStatus.Paused;
    }

    public void Activate(DateTimeOffset activatedAt)
    {
        if (Status is not PromotionStatus.Approved and not PromotionStatus.Paused)
        {
            throw new DomainValidationException("Only approved or paused promotions can be activated.");
        }

        if (activatedAt > EndsAt)
        {
            throw new DomainValidationException("Expired promotions cannot be activated.");
        }

        Status = PromotionStatus.Active;
    }

    public void Cancel(DateTimeOffset cancelledAt, string reason)
    {
        if (Status == PromotionStatus.Cancelled)
        {
            return;
        }

        Status = PromotionStatus.Cancelled;
        CancelledAt = cancelledAt;
        CancelReason = Guard.NotNullOrWhiteSpace(reason, nameof(reason), maxLength: 512);
    }
}

public sealed record PromotionLineInput(
    Guid? ItemId,
    Guid? ItemPackId,
    Guid? BundleId,
    Guid? CategoryId,
    Guid? BrandId,
    decimal? BuyQuantity,
    Guid? GetItemId,
    decimal? GetQuantity,
    decimal? DiscountPercent,
    decimal? DiscountAmount,
    decimal? SpecialPrice,
    decimal? MinimumBasketAmount);

public sealed class PromotionLine : AuditableEntity
{
    private PromotionLine() { }

    internal PromotionLine(
        Guid? itemId,
        Guid? itemPackId,
        Guid? bundleId,
        Guid? categoryId,
        Guid? brandId,
        decimal? buyQuantity,
        Guid? getItemId,
        decimal? getQuantity,
        decimal? discountPercent,
        decimal? discountAmount,
        decimal? specialPrice,
        decimal? minimumBasketAmount)
    {
        ItemId = itemId;
        ItemPackId = itemPackId;
        BundleId = bundleId;
        CategoryId = categoryId;
        BrandId = brandId;
        BuyQuantity = buyQuantity;
        GetItemId = getItemId;
        GetQuantity = getQuantity;
        DiscountPercent = discountPercent;
        DiscountAmount = discountAmount;
        SpecialPrice = specialPrice;
        MinimumBasketAmount = minimumBasketAmount;
    }

    public Guid PromotionId { get; private set; }
    public Guid? ItemId { get; private set; }
    public Guid? ItemPackId { get; private set; }
    public Guid? BundleId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? BrandId { get; private set; }
    public decimal? BuyQuantity { get; private set; }
    public Guid? GetItemId { get; private set; }
    public decimal? GetQuantity { get; private set; }
    public decimal? DiscountPercent { get; private set; }
    public decimal? DiscountAmount { get; private set; }
    public decimal? SpecialPrice { get; private set; }
    public decimal? MinimumBasketAmount { get; private set; }
}
