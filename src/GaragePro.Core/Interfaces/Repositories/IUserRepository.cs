using GaragePro.Core.Entities;

namespace GaragePro.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
}
