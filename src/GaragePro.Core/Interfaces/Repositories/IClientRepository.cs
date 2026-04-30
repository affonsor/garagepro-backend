using GaragePro.Core.Entities;

namespace GaragePro.Core.Interfaces.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id);
    Task<(IEnumerable<Client> Clients, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
    Task CreateAsync(Client client);
    Task UpdateAsync(Client client);
    Task DeleteAsync(Guid id);
    Task<bool> HasVehiclesByClientIdAsync(Guid clientId);
}
