using ISS.Api.Security;
using ISS.Application.Persistence;
using ISS.Application.Services;
using ISS.Domain.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISS.Api.Controllers.Inventory;

[ApiController]
[Route("api/inventory/expiry")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Inventory},{Roles.Reporting}")]
public sealed class ExpiryController(IIssDbContext dbContext, RetailInventoryPlanningService service) : ControllerBase
{
    public sealed record StockLayerExpiryDto(Guid Id, Guid WarehouseId, Guid? WarehouseBinId, Guid ItemId, decimal RemainingQuantity, decimal UnitCost, string? BatchNumber, DateOnly? ExpiryDate, StockLayerStatus Status);
    public sealed record CreateWriteOffLineRequest(Guid StockLayerId, decimal Quantity, string? Reason);
    public sealed record CreateWriteOffRequest(Guid WarehouseId, string? Notes, IReadOnlyList<CreateWriteOffLineRequest> Lines);
    public sealed record WriteOffDto(Guid Id, string Number, Guid WarehouseId, string? Notes, ExpiryWriteOffStatus Status, DateTimeOffset? ApprovedAt, DateTimeOffset? PostedAt, int LineCount);

    [HttpGet("near-expiry")]
    public async Task<ActionResult<IReadOnlyList<StockLayerExpiryDto>>> NearExpiry([FromQuery] Guid? warehouseId, [FromQuery] int days = 30, CancellationToken cancellationToken = default)
        => Ok((await service.GetNearExpiryAsync(warehouseId, days, cancellationToken)).Select(ToExpiryDto).ToList());

    [HttpGet("expired")]
    public async Task<ActionResult<IReadOnlyList<StockLayerExpiryDto>>> Expired([FromQuery] Guid? warehouseId, CancellationToken cancellationToken)
        => Ok((await service.GetExpiredAsync(warehouseId, cancellationToken)).Select(ToExpiryDto).ToList());

    [HttpGet("write-offs")]
    public async Task<ActionResult<IReadOnlyList<WriteOffDto>>> ListWriteOffs(CancellationToken cancellationToken)
    {
        var rows = await dbContext.ExpiryWriteOffs.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WriteOffDto(x.Id, x.Number, x.WarehouseId, x.Notes, x.Status, x.ApprovedAt, x.PostedAt, x.Lines.Count))
            .ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpPost("write-offs")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<ActionResult> CreateWriteOff(CreateWriteOffRequest request, CancellationToken cancellationToken)
    {
        var id = await service.CreateExpiryWriteOffAsync(request.WarehouseId, request.Lines.Select(x => (x.StockLayerId, x.Quantity, x.Reason)).ToList(), request.Notes, cancellationToken);
        return CreatedAtAction(nameof(ListWriteOffs), new { id }, new { id });
    }

    [HttpPost("write-offs/{id:guid}/approve")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await service.ApproveExpiryWriteOffAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("write-offs/{id:guid}/post")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory}")]
    public async Task<IActionResult> Post(Guid id, CancellationToken cancellationToken)
    {
        await service.PostExpiryWriteOffAsync(id, cancellationToken);
        return NoContent();
    }

    private static StockLayerExpiryDto ToExpiryDto(StockLayer x)
        => new(x.Id, x.WarehouseId, x.WarehouseBinId, x.ItemId, x.RemainingQuantity, x.UnitCost, x.BatchNumber, x.ExpiryDate, x.Status);
}
