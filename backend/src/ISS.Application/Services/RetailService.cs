using ISS.Application.Abstractions;
using ISS.Application.Common;
using ISS.Application.Persistence;
using ISS.Domain.Retail;
using Microsoft.EntityFrameworkCore;

namespace ISS.Application.Services;

public sealed class RetailService(
    IIssDbContext dbContext,
    IDocumentNumberService documentNumberService,
    IClock clock)
{
    public async Task<Guid> CreateUtilityPaymentAsync(
        UtilityPaymentType type,
        string provider,
        string accountNumber,
        string? customerPhone,
        decimal amount,
        decimal serviceFee,
        Guid? paymentTypeId,
        string? externalReference,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var number = await documentNumberService.NextAsync("UTIL", "UTIL", cancellationToken);
        var payment = new UtilityPayment(
            number,
            type,
            provider,
            accountNumber,
            customerPhone,
            amount,
            serviceFee,
            paymentTypeId,
            externalReference,
            notes,
            clock.UtcNow);

        await dbContext.UtilityPayments.AddAsync(payment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return payment.Id;
    }

    public async Task PostUtilityPaymentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await dbContext.UtilityPayments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                      ?? throw new NotFoundException("Utility payment not found.");

        payment.Post(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task VoidUtilityPaymentAsync(Guid id, string reason, CancellationToken cancellationToken = default)
    {
        var payment = await dbContext.UtilityPayments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                      ?? throw new NotFoundException("Utility payment not found.");

        payment.Void(clock.UtcNow, reason);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
