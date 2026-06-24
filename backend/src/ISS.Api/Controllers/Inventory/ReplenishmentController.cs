using ISS.Api.Security;
using ISS.Application.Persistence;
using ISS.Application.Services;
using ISS.Domain.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ISS.Api.Controllers.Inventory;

[ApiController]
[Route("api/replenishment")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Inventory},{Roles.Procurement},{Roles.Reporting}")]
public sealed class ReplenishmentController(IIssDbContext dbContext, RetailInventoryPlanningService service) : ControllerBase
{
    public sealed record CreateRunRequest(Guid WarehouseId, string? Notes);
    public sealed record ConvertRunRequest(bool Submit = false);
    public sealed record ReplenishmentRunDto(Guid Id, string Number, Guid WarehouseId, DateTimeOffset CalculatedAt, string? Notes, ReplenishmentRunStatus Status, Guid? PurchaseRequisitionId, int LineCount);

    [HttpGet("recommendations")]
    public async Task<IActionResult> Recommendations([FromQuery] Guid warehouseId, CancellationToken cancellationToken)
    {
        if (warehouseId == Guid.Empty) return BadRequest("warehouseId is required.");
        return Ok(await service.CalculateReplenishmentAsync(warehouseId, cancellationToken));
    }

    [HttpGet("runs")]
    public async Task<ActionResult<IReadOnlyList<ReplenishmentRunDto>>> Runs(CancellationToken cancellationToken)
    {
        var rows = await dbContext.ReplenishmentRuns.AsNoTracking()
            .OrderByDescending(x => x.CalculatedAt)
            .Select(x => new ReplenishmentRunDto(x.Id, x.Number, x.WarehouseId, x.CalculatedAt, x.Notes, x.Status, x.PurchaseRequisitionId, x.Lines.Count))
            .ToListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpGet("runs/{id:guid}")]
    public async Task<IActionResult> GetRun(Guid id, CancellationToken cancellationToken)
    {
        var run = await dbContext.ReplenishmentRuns.AsNoTracking().Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return run is null ? NotFound() : Ok(run);
    }

    [HttpPost("runs")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory},{Roles.Procurement}")]
    public async Task<ActionResult> CreateRun(CreateRunRequest request, CancellationToken cancellationToken)
    {
        if (request.WarehouseId == Guid.Empty) return BadRequest("warehouseId is required.");
        var id = await service.CreateReplenishmentRunAsync(request.WarehouseId, request.Notes, cancellationToken);
        return CreatedAtAction(nameof(GetRun), new { id }, new { id });
    }

    [HttpPost("runs/{id:guid}/create-purchase-requisition")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Inventory},{Roles.Procurement}")]
    public async Task<ActionResult> Convert(Guid id, ConvertRunRequest request, CancellationToken cancellationToken)
    {
        var prId = await service.ConvertReplenishmentRunToPurchaseRequisitionAsync(id, request.Submit, cancellationToken);
        return Ok(new { purchaseRequisitionId = prId });
    }
}
