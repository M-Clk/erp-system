using ERP.Application.Dto;
using ERP.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/terminals")]
[Authorize]
public class TerminalsController(ITerminalService terminals) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TerminalDto>>> Get(CancellationToken ct)
        => Ok(await terminals.GetAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TerminalDto>> GetById(Guid id, CancellationToken ct)
    {
        var terminal = await terminals.GetByIdAsync(id, ct);
        return terminal is null ? NotFound() : Ok(terminal);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TerminalDto>> Create([FromBody] CreateTerminalRequest request, CancellationToken ct)
    {
        try
        {
            var terminal = await terminals.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = terminal.Id }, terminal);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTerminalRequest request, CancellationToken ct)
    {
        try
        {
            return await terminals.UpdateAsync(id, request, ct) ? NoContent() : NotFound();
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
            return await terminals.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
