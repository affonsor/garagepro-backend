using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Repositories;

public class VehicleRepository(AppDbContext db) : IVehicleRepository
{
    public async Task<Vehicle?> GetByIdAsync(Guid id) =>
        await db.Vehicles
            .Include(v => v.Client)
            .Include(v => v.TransferHistory)
                .ThenInclude(t => t.FromClient)
            .Include(v => v.TransferHistory)
                .ThenInclude(t => t.ToClient)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<(IEnumerable<Vehicle> Vehicles, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, Guid? clientId)
    {
        var query = db.Vehicles.AsQueryable();
        if (clientId.HasValue)
            query = query.Where(v => v.ClientId == clientId.Value);

        var total = await query.CountAsync();
        var vehicles = await query
            .Include(v => v.Client)
            .OrderBy(v => v.LicensePlate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (vehicles, total);
    }

    public async Task<bool> ExistsByLicensePlateAsync(string licensePlate, Guid? excludeId = null)
    {
        var query = db.Vehicles.Where(v => v.LicensePlate == licensePlate);
        if (excludeId.HasValue)
            query = query.Where(v => v.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task CreateAsync(Vehicle vehicle)
    {
        db.Vehicles.Add(vehicle);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vehicle vehicle)
    {
        db.Vehicles.Update(vehicle);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var vehicle = await db.Vehicles.FindAsync(id);
        if (vehicle is not null)
        {
            db.Vehicles.Remove(vehicle);
            await db.SaveChangesAsync();
        }
    }

    public async Task<(Guid TransferRecordId, DateTimeOffset TransferredAt)> TransferAsync(Guid vehicleId, Guid toClientId, string? notes)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var vehicle = await db.Vehicles.FindAsync(vehicleId)
            ?? throw new InvalidOperationException($"Vehicle {vehicleId} not found");

        var record = new VehicleTransferRecord
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            FromClientId = vehicle.ClientId,
            ToClientId = toClientId,
            TransferredAt = DateTimeOffset.UtcNow,
            Notes = notes
        };

        vehicle.ClientId = toClientId;
        vehicle.UpdatedAt = DateTimeOffset.UtcNow;

        db.VehicleTransferRecords.Add(record);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return (record.Id, record.TransferredAt);
    }

    public async Task<bool> HasTransferHistoryAsync(Guid vehicleId) =>
        await db.VehicleTransferRecords.AnyAsync(t => t.VehicleId == vehicleId);
}
