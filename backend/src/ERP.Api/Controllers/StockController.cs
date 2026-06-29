using ERP.Application.Dto;
using ERP.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController(IStockService stock) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<IReadOnlyList<CurrentStockDto>>> GetCurrent(CancellationToken cancellationToken)
        => Ok(await stock.GetCurrentAsync(cancellationToken));

    [HttpGet("movements")]
    public async Task<ActionResult<IReadOnlyList<StockMovementDto>>> GetMovements(CancellationToken cancellationToken)
        => Ok(await stock.GetMovementsAsync(cancellationToken));

    [HttpPost("movements")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<StockMovementDto>> AddMovement(
        [FromBody] AddStockMovementRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await stock.AddMovementAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetMovements), result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("movements/{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CancelMovement(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await stock.CancelMovementAsync(id, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

