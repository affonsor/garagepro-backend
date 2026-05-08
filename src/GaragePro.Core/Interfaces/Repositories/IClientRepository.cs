using GaragePro.Core.Entities;

namespace GaragePro.Core.Interfaces.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id);
    Task<(IEnumerable<Client> Clients, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        bool includeInactive,
        string? search = null,
        string? tier = null,
        int? birthdayMonth = null);
    Task<bool> ExistsByDocumentAsync(string document, Guid? excludeId = null);
    Task<int> CountVehiclesByClientIdAsync(Guid clientId);
    Task CreateAsync(Client client);
    Task UpdateAsync(Client client);
}
