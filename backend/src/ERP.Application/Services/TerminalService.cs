using ERP.Application.Abstractions;
using ERP.Application.Dto;
using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services;

public interface ITerminalService
{
    Task<IReadOnlyList<TerminalDto>> GetAsync(CancellationToken ct = default);
    Task<TerminalDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<TerminalDto> CreateAsync(CreateTerminalRequest request, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateTerminalRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class TerminalService(IErpDbContext db) : ITerminalService
{
    private static TerminalDto ToDto(Terminal t, string warehouseName) =>
        new(t.Id, t.Code, t.Name, t.WarehouseId, warehouseName, t.IsActive);

    public async Task<IReadOnlyList<TerminalDto>> GetAsync(CancellationToken ct = default)
    {
        return await db.Terminals
            .AsNoTracking()
            .Include(t => t.Warehouse)
            .OrderBy(t => t.Code)
            .Select(t => new TerminalDto(
                t.Id,
                t.Code,
                t.Name,
                t.WarehouseId,
                t.Warehouse != null ? t.Warehouse.Name : string.Empty,
                t.IsActive))
            .ToListAsync(ct);
    }

    public async Task<TerminalDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var t = await db.Terminals
            .AsNoTracking()
            .Include(t => t.Warehouse)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

        return t is null ? null : ToDto(t, t.Warehouse?.Name ?? string.Empty);
    }

    public async Task<TerminalDto> CreateAsync(CreateTerminalRequest request, CancellationToken ct = default)
    {
        var codeExists = await db.Terminals.AnyAsync(t => t.Code == request.Code.Trim(), ct);
        if (codeExists)
            throw new InvalidOperationException($"'{request.Code}' kodu zaten kullanılıyor.");

        var warehouseExists = await db.Warehouses.AnyAsync(w => w.Id == request.WarehouseId, ct);
        if (!warehouseExists)
            throw new InvalidOperationException("Belirtilen depo bulunamadı.");

        var terminal = new Terminal
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Name = request.Name.Trim(),
            WarehouseId = request.WarehouseId,
            IsActive = true
        };

        db.Terminals.Add(terminal);
        await db.SaveChangesAsync(ct);

        var warehouse = await db.Warehouses.FindAsync([request.WarehouseId], ct);
        return ToDto(terminal, warehouse?.Name ?? string.Empty);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTerminalRequest request, CancellationToken ct = default)
    {
        var terminal = await db.Terminals.FindAsync([id], ct);
        if (terminal is null) return false;

        var codeExists = await db.Terminals.AnyAsync(t => t.Code == request.Code.Trim() && t.Id != id, ct);
        if (codeExists)
            throw new InvalidOperationException($"'{request.Code}' kodu zaten kullanılıyor.");

        var warehouseExists = await db.Warehouses.AnyAsync(w => w.Id == request.WarehouseId, ct);
        if (!warehouseExists)
            throw new InvalidOperationException("Belirtilen depo bulunamadı.");

        terminal.Code = request.Code.Trim().ToUpperInvariant();
        terminal.Name = request.Name.Trim();
        terminal.WarehouseId = request.WarehouseId;
        terminal.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var terminal = await db.Terminals.FindAsync([id], ct);
        if (terminal is null) return false;

        var hasSales = await db.Sales.AnyAsync(s => s.TerminalId == id, ct);
        if (hasSales)
            throw new InvalidOperationException("Bu kasaya ait satış kayıtları bulunduğundan silinemez.");

        db.Terminals.Remove(terminal);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
