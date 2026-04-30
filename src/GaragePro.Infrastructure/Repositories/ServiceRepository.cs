using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Repositories;

public class ServiceRepository(AppDbContext db) : IServiceRepository
{
    public async Task<Service?> GetByIdAsync(Guid id) =>
        await db.Services.FindAsync(id);

    public async Task<(IEnumerable<Service> Services, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var total = await db.Services.CountAsync();
        var services = await db.Services
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
        await db.Services.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync(ct);

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
