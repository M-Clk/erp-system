using ERP.Application.Abstractions;
using ERP.Application.Dto;
using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.Services;

public interface IWarehouseService
{
    Task<IReadOnlyList<WarehouseDto>> GetAsync(CancellationToken ct = default);
    Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, UpdateWarehouseRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class WarehouseService(IErpDbContext db) : IWarehouseService
{
    private static WarehouseDto ToDto(Warehouse w) => new(w.Id, w.Name);

    public async Task<IReadOnlyList<WarehouseDto>> GetAsync(CancellationToken ct = default)
    {
        return await db.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseDto(w.Id, w.Name))
            .ToListAsync(ct);
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var w = await db.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, ct);
        return w is null ? null : ToDto(w);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request, CancellationToken ct = default)
    {
        var nameExists = await db.Warehouses.AnyAsync(w => w.Name == request.Name.Trim(), ct);
        if (nameExists)
            throw new InvalidOperationException($"'{request.Name}' adlı depo zaten mevcut.");

        var warehouse = new Warehouse
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim()
        };

        db.Warehouses.Add(warehouse);
        await db.SaveChangesAsync(ct);
        return ToDto(warehouse);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateWarehouseRequest request, CancellationToken ct = default)
    {
        var warehouse = await db.Warehouses.FindAsync([id], ct);
        if (warehouse is null) return false;

        var nameTaken = await db.Warehouses.AnyAsync(w => w.Name == request.Name.Trim() && w.Id != id, ct);
        if (nameTaken)
            throw new InvalidOperationException($"'{request.Name}' adlı depo zaten mevcut.");

        warehouse.Name = request.Name.Trim();
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var warehouse = await db.Warehouses.FindAsync([id], ct);
        if (warehouse is null) return false;

        var hasMovements = await db.StockMovements.AnyAsync(m => m.WarehouseId == id, ct);
        if (hasMovements)
            throw new InvalidOperationException("Bu depoya ait stok hareketleri bulunduğundan silinemez.");

        var hasTerminals = await db.Terminals.AnyAsync(t => t.WarehouseId == id, ct);
        if (hasTerminals)
            throw new InvalidOperationException("Bu depoya bağlı kasalar bulunduğundan silinemez.");

        db.Warehouses.Remove(warehouse);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
