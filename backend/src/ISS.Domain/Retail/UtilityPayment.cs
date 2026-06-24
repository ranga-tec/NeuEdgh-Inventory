using ISS.Domain.Common;

namespace ISS.Domain.Retail;

public enum UtilityPaymentType
{
    Airtime = 1,
    BillPayment = 2
}

public enum UtilityPaymentStatus
{
    Draft = 1,
    Posted = 2,
    Voided = 3
}

public sealed class UtilityPayment : AuditableEntity
{
    private UtilityPayment() { }

    public UtilityPayment(
        string number,
        UtilityPaymentType type,
        string provider,
        string accountNumber,
        string? customerPhone,
        decimal amount,
        decimal serviceFee,
        Guid? paymentTypeId,
        string? externalReference,
        string? notes,
        DateTimeOffset createdAt)
    {
        Number = Guard.NotNullOrWhiteSpace(number, nameof(Number), maxLength: 32);
        Type = type;
        Provider = Guard.NotNullOrWhiteSpace(provider, nameof(Provider), maxLength: 128);
        AccountNumber = Guard.NotNullOrWhiteSpace(accountNumber, nameof(AccountNumber), maxLength: 128);
        CustomerPhone = customerPhone?.Trim();
        Amount = Guard.Positive(amount, nameof(Amount));
        ServiceFee = Guard.NotNegative(serviceFee, nameof(ServiceFee));
        PaymentTypeId = paymentTypeId;
        ExternalReference = externalReference?.Trim();
        Notes = notes?.Trim();
        CreatedAt = createdAt;
        Status = UtilityPaymentStatus.Draft;
    }

    public string Number { get; private set; } = null!;
    public UtilityPaymentType Type { get; private set; }
    public UtilityPaymentStatus Status { get; private set; }
    public string Provider { get; private set; } = null!;
    public string AccountNumber { get; private set; } = null!;
    public string? CustomerPhone { get; private set; }
    public decimal Amount { get; private set; }
    public decimal ServiceFee { get; private set; }
    public decimal Total => Amount + ServiceFee;
    public Guid? PaymentTypeId { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public DateTimeOffset? VoidedAt { get; private set; }
    public string? VoidReason { get; private set; }

    public void Post(DateTimeOffset postedAt)
    {
        if (Status != UtilityPaymentStatus.Draft)
        {
            throw new DomainValidationException("Only draft utility payments can be posted.");
        }

        Status = UtilityPaymentStatus.Posted;
        PostedAt = postedAt;
    }

    public void Void(DateTimeOffset voidedAt, string reason)
    {
        if (Status == UtilityPaymentStatus.Voided)
        {
            throw new DomainValidationException("Utility payment is already voided.");
        }

        Status = UtilityPaymentStatus.Voided;
        VoidedAt = voidedAt;
        VoidReason = Guard.NotNullOrWhiteSpace(reason, nameof(reason), maxLength: 512);
    }
}
