using ERP.Application.Dto;
using ERP.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public class WarehousesController(IWarehouseService warehouses) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WarehouseDto>>> Get(CancellationToken ct)
        => Ok(await warehouses.GetAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseDto>> GetById(Guid id, CancellationToken ct)
    {
        var warehouse = await warehouses.GetByIdAsync(id, ct);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<WarehouseDto>> Create([FromBody] CreateWarehouseRequest request, CancellationToken ct)
    {
        try
        {
            var warehouse = await warehouses.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWarehouseRequest request, CancellationToken ct)
    {
        try
        {
            return await warehouses.UpdateAsync(id, request, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            return await warehouses.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
