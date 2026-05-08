using GaragePro.Core.Entities;

namespace GaragePro.Core.Interfaces.Repositories;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id);
    Task<(IEnumerable<Service> Services, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string? category = null,
        string? tier = null,
        bool? active = null);
    Task<IEnumerable<Service>> GetActiveAsync(CancellationToken ct);
    Task CreateAsync(Service service);
    Task UpdateAsync(Service service);
    Task DeleteAsync(Guid id);
}
