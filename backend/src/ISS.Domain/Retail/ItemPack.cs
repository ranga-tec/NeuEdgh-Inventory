using ISS.Domain.Common;

namespace ISS.Domain.Retail;

public sealed class ItemPack : AuditableEntity
{
    private ItemPack() { }

    public ItemPack(
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
        Guid? taxCodeId)
    {
        ItemId = itemId == Guid.Empty ? throw new DomainValidationException("Item is required.") : itemId;
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code), maxLength: 32);
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
        Barcode = Guard.NotNullOrWhiteSpace(barcode, nameof(barcode), maxLength: 128);
        BaseQuantity = Guard.Positive(baseQuantity, nameof(baseQuantity));
        UnitOfMeasure = Guard.NotNullOrWhiteSpace(unitOfMeasure, nameof(unitOfMeasure), maxLength: 32);
        Price = Guard.NotNegative(price, nameof(price));
        PurchaseAllowed = purchaseAllowed;
        SaleAllowed = saleAllowed;
        TransferAllowed = transferAllowed;
        IsDefaultPurchasePack = isDefaultPurchasePack;
        IsDefaultSalesPack = isDefaultSalesPack;
        TaxCodeId = taxCodeId;
        IsActive = true;
    }

    public Guid ItemId { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Barcode { get; private set; } = null!;
    public decimal BaseQuantity { get; private set; }
    public string UnitOfMeasure { get; private set; } = null!;
    public decimal Price { get; private set; }
    public bool PurchaseAllowed { get; private set; }
    public bool SaleAllowed { get; private set; }
    public bool TransferAllowed { get; private set; }
    public bool IsDefaultPurchasePack { get; private set; }
    public bool IsDefaultSalesPack { get; private set; }
    public Guid? TaxCodeId { get; private set; }
    public bool IsActive { get; private set; }

    public void Update(
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
        bool isActive)
    {
        Code = Guard.NotNullOrWhiteSpace(code, nameof(code), maxLength: 32);
        Name = Guard.NotNullOrWhiteSpace(name, nameof(name), maxLength: 128);
        Barcode = Guard.NotNullOrWhiteSpace(barcode, nameof(barcode), maxLength: 128);
        BaseQuantity = Guard.Positive(baseQuantity, nameof(baseQuantity));
        UnitOfMeasure = Guard.NotNullOrWhiteSpace(unitOfMeasure, nameof(unitOfMeasure), maxLength: 32);
        Price = Guard.NotNegative(price, nameof(price));
        PurchaseAllowed = purchaseAllowed;
        SaleAllowed = saleAllowed;
        TransferAllowed = transferAllowed;
        IsDefaultPurchasePack = isDefaultPurchasePack;
        IsDefaultSalesPack = isDefaultSalesPack;
        TaxCodeId = taxCodeId;
        IsActive = isActive;
    }
}
