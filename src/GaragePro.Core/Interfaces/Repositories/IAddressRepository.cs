using GaragePro.Core.Entities;

namespace GaragePro.Core.Interfaces.Repositories;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id);
    Task AddAsync(Address address);
    Task UpdateAsync(Address address);
    Task DeleteAsync(Guid id);
    Task<int> CountByClientIdAsync(Guid clientId);
}
