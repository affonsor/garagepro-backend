using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id) =>
        await db.Products.FindAsync(id);

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var total = await db.Products.CountAsync();
        var products = await db.Products
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (products, total);
    }

    public async Task CreateAsync(Product product)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        db.Products.Update(product);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveAsync(CancellationToken ct) =>
        await db.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync(ct);

    public async Task DeleteAsync(Guid id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is not null)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync();
        }
    }
}
