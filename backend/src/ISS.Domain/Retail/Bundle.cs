using ISS.Domain.Common;

namespace ISS.Domain.Retail;

public enum BundleType
{
    Dynamic = 1,
    PreAssembled = 2,
    PriceOnly = 3
}

public sealed class Bundle : AuditableEntity
{
    private readonly List<BundleComponent> _components = [];

    private Bundle() { }

    public Bundle(
        Guid companyId,
        string sku,
        string name,
        string barcode,
        BundleType type,
        decimal price,
        DateOnly? startsOn,
        DateOnly? endsOn,
        Guid? categoryId,
        Guid? taxCodeId)
    {
        CompanyId = companyId == Guid.Empty ? throw new DomainValidationException("Company is required.") : companyId;
        Sku = Guard.NotNullOrWhiteSpace(sku, nameof(sku), maxLength: 64);
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
        Barcode = Guard.NotNullOrWhiteSpace(barcode, nameof(barcode), maxLength: 128);
        Type = type;
        Price = Guard.NotNegative(price, nameof(price));
        StartsOn = startsOn;
        EndsOn = endsOn;
        CategoryId = categoryId;
        TaxCodeId = taxCodeId;
        IsActive = true;
    }

    public Guid CompanyId { get; private set; }
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Barcode { get; private set; } = null!;
    public BundleType Type { get; private set; }
    public decimal Price { get; private set; }
    public DateOnly? StartsOn { get; private set; }
    public DateOnly? EndsOn { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? TaxCodeId { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<BundleComponent> Components => _components;

    public void Update(string sku, string name, string barcode, BundleType type, decimal price, DateOnly? startsOn, DateOnly? endsOn, Guid? categoryId, Guid? taxCodeId, bool isActive)
    {
        Sku = Guard.NotNullOrWhiteSpace(sku, nameof(sku), maxLength: 64);
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
        Barcode = Guard.NotNullOrWhiteSpace(barcode, nameof(barcode), maxLength: 128);
        Type = type;
        Price = Guard.NotNegative(price, nameof(price));
        StartsOn = startsOn;
        EndsOn = endsOn;
        CategoryId = categoryId;
        TaxCodeId = taxCodeId;
        IsActive = isActive;
    }

    public void ReplaceComponents(IEnumerable<(Guid ItemId, decimal Quantity, string UnitOfMeasure, bool IsOptional)> components)
    {
        _components.Clear();
        foreach (var component in components)
        {
            _components.Add(new BundleComponent(component.ItemId, component.Quantity, component.UnitOfMeasure, component.IsOptional));
        }
    }
}

public sealed class BundleComponent : AuditableEntity
{
    private BundleComponent() { }

    internal BundleComponent(Guid itemId, decimal quantity, string unitOfMeasure, bool isOptional)
    {
        ItemId = itemId == Guid.Empty ? throw new DomainValidationException("Item is required.") : itemId;
        Quantity = Guard.Positive(quantity, nameof(quantity));
        UnitOfMeasure = Guard.NotNullOrWhiteSpace(unitOfMeasure, nameof(unitOfMeasure), maxLength: 32);
        IsOptional = isOptional;
    }

    public Guid BundleId { get; private set; }
    public Guid ItemId { get; private set; }
    public decimal Quantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = null!;
    public bool IsOptional { get; private set; }
}
