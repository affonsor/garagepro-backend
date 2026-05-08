using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Repositories;

public class ServiceRepository(AppDbContext db) : IServiceRepository
{
    public async Task<Service?> GetByIdAsync(Guid id) =>
        await db.Services
            .Include(s => s.VehiclePrices)
            .Include(s => s.Materials).ThenInclude(m => m.Product)
            .Include(s => s.Steps)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<(IEnumerable<Service> Services, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string? category = null,
        string? tier = null,
        bool? active = null)
    {
        var query = db.Services
            .Include(s => s.VehiclePrices)
            .Include(s => s.Materials).ThenInclude(m => m.Product)
            .Include(s => s.Steps)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                (s.Code != null && s.Code.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim().ToLower();
            query = query.Where(s => s.Category != null && s.Category.ToLower() == normalizedCategory);
        }

        if (!string.IsNullOrWhiteSpace(tier))
        {
            var normalizedTier = tier.Trim().ToLower();
            query = query.Where(s => s.Tier.ToLower() == normalizedTier);
        }

        if (active.HasValue)
            query = query.Where(s => s.IsActive == active.Value);

        var total = await query.CountAsync();
        var services = await query
            .OrderBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (services, total);
    }

    public async Task CreateAsync(Service service)
    {
        db.Services.Add(service);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Service service)
    {
        db.Services.Update(service);
        await db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Service>> GetActiveAsync(CancellationToken ct) =>
        await db.Services
            .Include(s => s.VehiclePrices)
            .Include(s => s.Materials).ThenInclude(m => m.Product)
            .Include(s => s.Steps)
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);

    public async Task DeleteAsync(Guid id)
    {
        var service = await db.Services.FindAsync(id);
        if (service is not null)
        {
            db.Services.Remove(service);
            await db.SaveChangesAsync();
        }
    }
}
