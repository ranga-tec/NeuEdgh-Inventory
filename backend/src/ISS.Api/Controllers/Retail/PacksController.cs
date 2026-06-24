using ISS.Api.Security;
using ISS.Application.Persistence;
using ISS.Application.Services;
using ISS.Domain.Retail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISS.Api.Controllers.Retail;

[ApiController]
[Route("api/retail/packs")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Inventory},{Roles.Sales},{Roles.Procurement},{Roles.Reporting}")]
public sealed class PacksController(IIssDbContext dbContext, RetailInventoryPlanningService service) : ControllerBase
{
    public sealed record PackDto(Guid Id, Guid ItemId, string Code, string Name, string Barcode, decimal BaseQuantity, string UnitOfMeasure, decimal Price, bool PurchaseAllowed, bool SaleAllowed, bool TransferAllowed, bool IsDefaultPurchasePack, bool IsDefaultSalesPack, Guid? TaxCodeId, bool IsActive);
    public sealed record SavePackRequest(Guid ItemId, string Code, string Name, string Barcode, decimal BaseQuantity, string UnitOfMeasure, decimal Price, bool PurchaseAllowed, bool SaleAllowed, bool TransferAllowed, bool IsDefaultPurchasePack, bool IsDefaultSalesPack, Guid? TaxCodeId, bool IsActive = true);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PackDto>>> List([FromQuery] Guid? itemId, CancellationToken cancellationToken)
    {
        var query = dbContext.ItemPacks.AsNoTracking();
        if (itemId is not null && itemId != Guid.Empty)
        {
            query = query.Where(x => x.ItemId == itemId);
        }

        var rows = await query.OrderBy(x => x.ItemId).ThenBy(x => x.Code).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PackDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var row = await dbContext.ItemPacks.AsNoTracking().Where(x => x.Id == id).Select(x => ToDto(x)).FirstOrDefaultAsync(cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<PackDto>> GetByBarcode(string barcode, CancellationToken cancellationToken)
    {
        var trimmed = barcode.Trim();
        var row = await dbContext.ItemPacks.AsNoTracking().Where(x => x.Barcode == trimmed && x.IsActive).Select(x => ToDto(x)).FirstOrDefaultAsync(cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<ActionResult<PackDto>> Create(SavePackRequest request, CancellationToken cancellationToken)
    {
        var pack = await service.CreatePackAsync(request.ItemId, request.Code, request.Name, request.Barcode, request.BaseQuantity, request.UnitOfMeasure, request.Price, request.PurchaseAllowed, request.SaleAllowed, request.TransferAllowed, request.IsDefaultPurchasePack, request.IsDefaultSalesPack, request.TaxCodeId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = pack.Id }, ToDto(pack));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<ActionResult<PackDto>> Update(Guid id, SavePackRequest request, CancellationToken cancellationToken)
    {
        await service.UpdatePackAsync(id, request.Code, request.Name, request.Barcode, request.BaseQuantity, request.UnitOfMeasure, request.Price, request.PurchaseAllowed, request.SaleAllowed, request.TransferAllowed, request.IsDefaultPurchasePack, request.IsDefaultSalesPack, request.TaxCodeId, request.IsActive, cancellationToken);
        var row = await dbContext.ItemPacks.AsNoTracking().Where(x => x.Id == id).Select(x => ToDto(x)).FirstAsync(cancellationToken);
        return Ok(row);
    }

    private static PackDto ToDto(ItemPack x)
        => new(x.Id, x.ItemId, x.Code, x.Name, x.Barcode, x.BaseQuantity, x.UnitOfMeasure, x.Price, x.PurchaseAllowed, x.SaleAllowed, x.TransferAllowed, x.IsDefaultPurchasePack, x.IsDefaultSalesPack, x.TaxCodeId, x.IsActive);
}
