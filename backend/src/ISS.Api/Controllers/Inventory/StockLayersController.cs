using ISS.Api.Security;
using ISS.Application.Persistence;
using ISS.Application.Services;
using ISS.Domain.Inventory;
using ISS.Domain.MasterData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISS.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/stock-layers")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Inventory},{Roles.Sales},{Roles.Procurement},{Roles.Reporting}")]
public sealed class StockLayersController(IIssDbContext dbContext, RetailInventoryPlanningService service) : ControllerBase
{
    public sealed record StockLayerDto(Guid Id, Guid WarehouseId, Guid? WarehouseBinId, Guid ItemId, decimal OriginalQuantity, decimal RemainingQuantity, decimal UnitCost, DateTimeOffset ReceivedAt, string ReferenceType, Guid ReferenceId, Guid? ReferenceLineId, string? BatchNumber, DateOnly? ExpiryDate, StockLayerStatus Status);
    public sealed record CreateStockLayerRequest(Guid WarehouseId, Guid? WarehouseBinId, Guid ItemId, decimal Quantity, decimal UnitCost, string ReferenceType, Guid ReferenceId, Guid? ReferenceLineId, string? BatchNumber, DateOnly? ExpiryDate);
    public sealed record AllocateRequest(Guid WarehouseId, Guid ItemId, decimal Quantity, ItemIssueMethod IssueMethod);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StockLayerDto>>> List([FromQuery] Guid? warehouseId, [FromQuery] Guid? itemId, CancellationToken cancellationToken)
    {
        var query = dbContext.StockLayers.AsNoTracking();
        if (warehouseId is not null && warehouseId != Guid.Empty) query = query.Where(x => x.WarehouseId == warehouseId);
        if (itemId is not null && itemId != Guid.Empty) query = query.Where(x => x.ItemId == itemId);
        var rows = await query.OrderBy(x => x.ExpiryDate).ThenBy(x => x.ReceivedAt).Select(x => ToDto(x)).ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory},{Roles.Procurement}")]
    public async Task<ActionResult<StockLayerDto>> Create(CreateStockLayerRequest request, CancellationToken cancellationToken)
    {
        var layer = await service.CreateStockLayerAsync(request.WarehouseId, request.WarehouseBinId, request.ItemId, request.Quantity, request.UnitCost, request.ReferenceType, request.ReferenceId, request.ReferenceLineId, request.BatchNumber, request.ExpiryDate, cancellationToken);
        return CreatedAtAction(nameof(List), new { itemId = layer.ItemId }, ToDto(layer));
    }

    [HttpPost("allocate")]
    public async Task<IActionResult> Allocate(AllocateRequest request, CancellationToken cancellationToken)
    {
        var allocations = await service.AllocateAsync(request.WarehouseId, request.ItemId, request.Quantity, request.IssueMethod, cancellationToken);
        return Ok(allocations);
    }

    private static StockLayerDto ToDto(StockLayer x)
        => new(x.Id, x.WarehouseId, x.WarehouseBinId, x.ItemId, x.OriginalQuantity, x.RemainingQuantity, x.UnitCost, x.ReceivedAt, x.ReferenceType, x.ReferenceId, x.ReferenceLineId, x.BatchNumber, x.ExpiryDate, x.Status);
}
