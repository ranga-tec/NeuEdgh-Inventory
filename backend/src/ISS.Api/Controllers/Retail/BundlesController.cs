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
[Route("api/retail/bundles")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Inventory},{Roles.Sales},{Roles.Reporting}")]
public sealed class BundlesController(IIssDbContext dbContext, ICurrentUser currentUser, RetailInventoryPlanningService service) : ControllerBase
{
    public sealed record BundleComponentDto(Guid Id, Guid ItemId, decimal Quantity, string UnitOfMeasure, bool IsOptional);
    public sealed record BundleDto(Guid Id, Guid CompanyId, string Sku, string Name, string Barcode, BundleType Type, decimal Price, DateOnly? StartsOn, DateOnly? EndsOn, Guid? CategoryId, Guid? TaxCodeId, bool IsActive, IReadOnlyList<BundleComponentDto> Components);
    public sealed record SaveBundleComponentRequest(Guid ItemId, decimal Quantity, string UnitOfMeasure, bool IsOptional);
    public sealed record SaveBundleRequest(string Sku, string Name, string Barcode, BundleType Type, decimal Price, DateOnly? StartsOn, DateOnly? EndsOn, Guid? CategoryId, Guid? TaxCodeId, bool IsActive, IReadOnlyList<SaveBundleComponentRequest> Components);
    public sealed record BundleAvailabilityDto(Guid BundleId, decimal AvailableQuantity);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BundleDto>>> List(CancellationToken cancellationToken)
    {
        var companyId = currentUser.CompanyId ?? CompanyDefaults.DefaultCompanyId;
        var rows = await dbContext.Bundles.AsNoTracking().Include(x => x.Components)
            .Where(x => x.CompanyId == companyId)
            .OrderBy(x => x.Sku)
            .ToListAsync(cancellationToken);
        return Ok(rows.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BundleDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var row = await dbContext.Bundles.AsNoTracking().Include(x => x.Components).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return row is null ? NotFound() : Ok(ToDto(row));
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<ActionResult<BundleDto>> Create(SaveBundleRequest request, CancellationToken cancellationToken)
    {
        var companyId = currentUser.CompanyId ?? CompanyDefaults.DefaultCompanyId;
        var bundle = new Bundle(companyId, request.Sku, request.Name, request.Barcode, request.Type, request.Price, request.StartsOn, request.EndsOn, request.CategoryId, request.TaxCodeId);
        bundle.Update(request.Sku, request.Name, request.Barcode, request.Type, request.Price, request.StartsOn, request.EndsOn, request.CategoryId, request.TaxCodeId, request.IsActive);
        bundle.ReplaceComponents(request.Components.Select(x => (x.ItemId, x.Quantity, x.UnitOfMeasure, x.IsOptional)));
        await dbContext.Bundles.AddAsync(bundle, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = bundle.Id }, ToDto(bundle));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<ActionResult<BundleDto>> Update(Guid id, SaveBundleRequest request, CancellationToken cancellationToken)
    {
        var bundle = await dbContext.Bundles.Include(x => x.Components).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (bundle is null)
        {
            return NotFound();
        }

        bundle.Update(request.Sku, request.Name, request.Barcode, request.Type, request.Price, request.StartsOn, request.EndsOn, request.CategoryId, request.TaxCodeId, request.IsActive);
        bundle.ReplaceComponents(request.Components.Select(x => (x.ItemId, x.Quantity, x.UnitOfMeasure, x.IsOptional)));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(bundle));
    }

    [HttpGet("{id:guid}/availability")]
    public async Task<ActionResult<BundleAvailabilityDto>> Availability(Guid id, [FromQuery] Guid warehouseId, CancellationToken cancellationToken)
    {
        var result = await service.GetBundleAvailabilityAsync(id, warehouseId, cancellationToken);
        return Ok(new BundleAvailabilityDto(result.BundleId, result.AvailableQuantity));
    }

    private static BundleDto ToDto(Bundle x)
        => new(
            x.Id,
            x.CompanyId,
            x.Sku,
            x.Name,
            x.Barcode,
            x.Type,
            x.Price,
            x.StartsOn,
            x.EndsOn,
            x.CategoryId,
            x.TaxCodeId,
            x.IsActive,
            x.Components.Select(c => new BundleComponentDto(c.Id, c.ItemId, c.Quantity, c.UnitOfMeasure, c.IsOptional)).ToList());
}
