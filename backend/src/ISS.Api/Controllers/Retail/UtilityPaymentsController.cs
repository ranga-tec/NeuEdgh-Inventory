using ISS.Api.Security;
using ISS.Application.Persistence;
using ISS.Application.Services;
using ISS.Domain.Retail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ISS.Api.Controllers.Retail;

[ApiController]
[Route("api/retail/utility-payments")]
[Authorize]
public sealed class UtilityPaymentsController(
    IIssDbContext dbContext,
    RetailService retailService,
    AccessControlService accessControl) : ControllerBase
{
    public sealed record UtilityPaymentDto(
        Guid Id,
        string Number,
        UtilityPaymentType Type,
        UtilityPaymentStatus Status,
        string Provider,
        string AccountNumber,
        string? CustomerPhone,
        decimal Amount,
        decimal ServiceFee,
        decimal Total,
        Guid? PaymentTypeId,
        string? PaymentTypeName,
        string? ExternalReference,
        string? Notes,
        DateTimeOffset CreatedAt,
        DateTimeOffset? PostedAt,
        DateTimeOffset? VoidedAt,
        string? VoidReason);

    public sealed record CreateUtilityPaymentRequest(
        UtilityPaymentType Type,
        string Provider,
        string AccountNumber,
        string? CustomerPhone,
        decimal Amount,
        decimal? ServiceFee,
        Guid? PaymentTypeId,
        string? ExternalReference,
        string? Notes);

    public sealed record VoidUtilityPaymentRequest(string Reason);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UtilityPaymentDto>>> List(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        if (!await HasPermissionAsync(AppPermissions.RetailUtilityPaymentView, cancellationToken))
        {
            return Forbid();
        }

        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 500);

        var payments = await dbContext.UtilityPayments.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .GroupJoin(
                dbContext.PaymentTypes.AsNoTracking(),
                utility => utility.PaymentTypeId,
                paymentType => paymentType.Id,
                (utility, paymentTypes) => new { utility, paymentType = paymentTypes.FirstOrDefault() })
            .Select(x => new UtilityPaymentDto(
                x.utility.Id,
                x.utility.Number,
                x.utility.Type,
                x.utility.Status,
                x.utility.Provider,
                x.utility.AccountNumber,
                x.utility.CustomerPhone,
                x.utility.Amount,
                x.utility.ServiceFee,
                x.utility.Total,
                x.utility.PaymentTypeId,
                x.paymentType != null ? x.paymentType.Name : null,
                x.utility.ExternalReference,
                x.utility.Notes,
                x.utility.CreatedAt,
                x.utility.PostedAt,
                x.utility.VoidedAt,
                x.utility.VoidReason))
            .ToListAsync(cancellationToken);

        return Ok(payments);
    }

    [HttpPost]
    public async Task<ActionResult<UtilityPaymentDto>> Create(
        CreateUtilityPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!await HasPermissionAsync(AppPermissions.RetailUtilityPaymentCreate, cancellationToken))
        {
            return Forbid();
        }

        var id = await retailService.CreateUtilityPaymentAsync(
            request.Type,
            request.Provider,
            request.AccountNumber,
            request.CustomerPhone,
            request.Amount,
            request.ServiceFee ?? 0m,
            request.PaymentTypeId,
            request.ExternalReference,
            request.Notes,
            cancellationToken);

        var created = await dbContext.UtilityPayments.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UtilityPaymentDto(
                x.Id,
                x.Number,
                x.Type,
                x.Status,
                x.Provider,
                x.AccountNumber,
                x.CustomerPhone,
                x.Amount,
                x.ServiceFee,
                x.Total,
                x.PaymentTypeId,
                null,
                x.ExternalReference,
                x.Notes,
                x.CreatedAt,
                x.PostedAt,
                x.VoidedAt,
                x.VoidReason))
            .FirstAsync(cancellationToken);

        return Ok(created);
    }

    [HttpPost("{id:guid}/post")]
    public async Task<ActionResult> Post(Guid id, CancellationToken cancellationToken)
    {
        if (!await HasPermissionAsync(AppPermissions.RetailUtilityPaymentPost, cancellationToken))
        {
            return Forbid();
        }

        await retailService.PostUtilityPaymentAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/void")]
    public async Task<ActionResult> Void(Guid id, VoidUtilityPaymentRequest request, CancellationToken cancellationToken)
    {
        if (!await HasPermissionAsync(AppPermissions.RetailUtilityPaymentVoid, cancellationToken))
        {
            return Forbid();
        }

        await retailService.VoidUtilityPaymentAsync(id, request.Reason, cancellationToken);
        return NoContent();
    }

    private async Task<bool> HasPermissionAsync(string permissionKey, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdValue, out var userId)
               && await accessControl.HasPermissionAsync(userId, permissionKey, cancellationToken);
    }
}
