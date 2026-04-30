using GaragePro.Core.Entities;

namespace GaragePro.Core.Interfaces.Repositories;

public interface IVehicleRepository
{
    Task<Vehicle?> GetByIdAsync(Guid id);
    Task<(IEnumerable<Vehicle> Vehicles, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, Guid? clientId);
    Task<bool> ExistsByLicensePlateAsync(string licensePlate, Guid? excludeId = null);
    Task CreateAsync(Vehicle vehicle);
    Task UpdateAsync(Vehicle vehicle);
    Task DeleteAsync(Guid id);
    Task<(Guid TransferRecordId, DateTimeOffset TransferredAt)> TransferAsync(Guid vehicleId, Guid toClientId, string? notes);
    Task<bool> HasTransferHistoryAsync(Guid vehicleId);
}
