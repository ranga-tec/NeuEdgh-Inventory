using ISS.Api.Security;
using ISS.Application.Abstractions;
using ISS.Application.Persistence;
using ISS.Application.Services;
using ISS.Domain.MasterData;
using ISS.Domain.Retail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISS.Api.Controllers.Retail;

[ApiController]
[Route("api/retail/promotions")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Inventory},{Roles.Sales},{Roles.Reporting}")]
public sealed class PromotionsController(IIssDbContext dbContext, ICurrentUser currentUser, RetailInventoryPlanningService service) : ControllerBase
{
    public sealed record PromotionLineDto(Guid Id, Guid? ItemId, Guid? ItemPackId, Guid? BundleId, Guid? CategoryId, Guid? BrandId, decimal? BuyQuantity, Guid? GetItemId, decimal? GetQuantity, decimal? DiscountPercent, decimal? DiscountAmount, decimal? SpecialPrice, decimal? MinimumBasketAmount);
    public sealed record PromotionDto(Guid Id, Guid CompanyId, string Code, string Name, string? Description, PromotionType Type, DateTimeOffset StartsAt, DateTimeOffset EndsAt, int Priority, bool IsStackable, decimal? MaxDiscountAmount, PromotionStatus Status, IReadOnlyList<PromotionLineDto> Lines);
    public sealed record SavePromotionRequest(string Code, string Name, string? Description, PromotionType Type, DateTimeOffset StartsAt, DateTimeOffset EndsAt, int Priority, bool IsStackable, decimal? MaxDiscountAmount, IReadOnlyList<PromotionLineInput> Lines);
    public sealed record CancelPromotionRequest(string Reason);
    public sealed record EvaluatePromotionLineRequest(Guid? ItemId, Guid? ItemPackId, Guid? BundleId, Guid? CategoryId, Guid? BrandId, decimal Quantity, decimal UnitPrice);
    public sealed record EvaluatePromotionRequest(IReadOnlyList<EvaluatePromotionLineRequest> Lines);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromotionDto>>> List(CancellationToken cancellationToken)
    {
        var companyId = currentUser.CompanyId ?? CompanyDefaults.DefaultCompanyId;
        var rows = await dbContext.Promotions.AsNoTracking().Include(x => x.Lines)
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.StartsAt)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PromotionDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var row = await dbContext.Promotions.AsNoTracking().Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return row is null ? NotFound() : Ok(ToDto(row));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<ActionResult<PromotionDto>> Create(SavePromotionRequest request, CancellationToken cancellationToken)
    {
        var companyId = currentUser.CompanyId ?? CompanyDefaults.DefaultCompanyId;
        var promotion = new Promotion(companyId, request.Code, request.Name, request.Type, request.StartsAt, request.EndsAt, request.Priority, request.IsStackable, request.MaxDiscountAmount, request.Description);
        promotion.ReplaceLines(request.Lines);
        await dbContext.Promotions.AddAsync(promotion, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = promotion.Id }, ToDto(promotion));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<ActionResult<PromotionDto>> Update(Guid id, SavePromotionRequest request, CancellationToken cancellationToken)
    {
        var promotion = await dbContext.Promotions.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promotion is null)
        {
            return NotFound();
        }

        promotion.Update(request.Code, request.Name, request.Type, request.StartsAt, request.EndsAt, request.Priority, request.IsStackable, request.MaxDiscountAmount, request.Description);
        promotion.ReplaceLines(request.Lines);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(promotion));
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await dbContext.Promotions.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promotion is null) return NotFound();
        promotion.Submit();
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await dbContext.Promotions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promotion is null) return NotFound();
        promotion.Approve(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/pause")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<IActionResult> Pause(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await dbContext.Promotions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promotion is null) return NotFound();
        promotion.Pause();
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var promotion = await dbContext.Promotions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promotion is null) return NotFound();
        promotion.Activate(DateTimeOffset.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<IActionResult> Cancel(Guid id, CancelPromotionRequest request, CancellationToken cancellationToken)
    {
        var promotion = await dbContext.Promotions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (promotion is null) return NotFound();
        promotion.Cancel(DateTimeOffset.UtcNow, request.Reason);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate(EvaluatePromotionRequest request, CancellationToken cancellationToken)
    {
        var companyId = currentUser.CompanyId ?? CompanyDefaults.DefaultCompanyId;
        var result = await service.EvaluatePromotionsAsync(companyId, request.Lines.Select(x => (x.ItemId, x.ItemPackId, x.BundleId, x.CategoryId, x.BrandId, x.Quantity, x.UnitPrice)).ToList(), cancellationToken);
        return Ok(result);
    }

    private static PromotionDto ToDto(Promotion x)
        => new(
            x.Id,
            x.CompanyId,
            x.Code,
            x.Name,
            x.Description,
            x.Type,
            x.StartsAt,
            x.EndsAt,
            x.Priority,
            x.IsStackable,
            x.MaxDiscountAmount,
            x.Status,
            x.Lines.Select(l => new PromotionLineDto(l.Id, l.ItemId, l.ItemPackId, l.BundleId, l.CategoryId, l.BrandId, l.BuyQuantity, l.GetItemId, l.GetQuantity, l.DiscountPercent, l.DiscountAmount, l.SpecialPrice, l.MinimumBasketAmount)).ToList());
}
