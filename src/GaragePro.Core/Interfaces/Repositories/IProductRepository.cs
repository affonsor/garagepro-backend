using GaragePro.Core.Entities;

namespace GaragePro.Core.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
    Task<IEnumerable<Product>> GetActiveAsync(CancellationToken ct);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
}
