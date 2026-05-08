using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(Guid id) =>
        await db.Products.FindAsync(id);

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string? status = null,
        string? category = null)
    {
        var query = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Sku != null && p.Sku.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim().ToLower();
            query = query.Where(p => p.Category != null && p.Category.ToLower() == normalizedCategory);
        }

        query = NormalizeStatus(status) switch
        {
            "active" => query.Where(p => p.IsActive),
            "inactive" => query.Where(p => !p.IsActive),
            "lowstock" => query.Where(p => p.Stock <= p.MinStock),
            _ => query
        };

        var total = await query.CountAsync();
        var products = await query
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

    private static string NormalizeStatus(string? status) =>
        status?.Replace("-", string.Empty).Replace("_", string.Empty).Trim().ToLowerInvariant() ?? "all";
}
