using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Repositories;

public class ClientRepository(AppDbContext db) : IClientRepository
{
    public async Task<Client?> GetByIdAsync(Guid id) =>
        await db.Clients
            .Include(c => c.Addresses)
            .Include(c => c.Vehicles).ThenInclude(v => v.ServiceOrders)
            .Include(c => c.ServiceOrders)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<(IEnumerable<Client> Clients, int TotalCount)> GetAllAsync(
        int pageNumber,
        int pageSize,
        bool includeInactive,
        string? search = null,
        string? tier = null,
        int? birthdayMonth = null)
    {
        var query = db.Clients.AsQueryable();
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                (c.Phone != null && c.Phone.Contains(search.Trim())) ||
                (c.Email != null && c.Email.ToLower().Contains(term)) ||
                c.Document.Contains(search.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(tier))
        {
            var normalizedTier = tier.Trim().ToLower();
            query = query.Where(c => c.Tier.ToLower() == normalizedTier);
        }

        if (birthdayMonth.HasValue)
            query = query.Where(c => c.Birthday.HasValue && c.Birthday.Value.Month == birthdayMonth.Value);

        var total = await query.CountAsync();
        var clients = await query
            .Include(c => c.Vehicles)
            .Include(c => c.ServiceOrders)
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (clients, total);
    }

    public async Task<bool> ExistsByDocumentAsync(string document, Guid? excludeId = null)
    {
        var query = db.Clients.Where(c => c.Document == document);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<int> CountVehiclesByClientIdAsync(Guid clientId) =>
        await db.Vehicles.CountAsync(v => v.ClientId == clientId);

    public async Task CreateAsync(Client client)
    {
        db.Clients.Add(client);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Client client)
    {
        db.Clients.Update(client);
        await db.SaveChangesAsync();
    }

}
