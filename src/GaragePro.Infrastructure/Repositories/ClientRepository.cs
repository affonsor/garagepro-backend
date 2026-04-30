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
            .Include(c => c.Vehicles)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<(IEnumerable<Client> Clients, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var total = await db.Clients.CountAsync();
        var clients = await db.Clients
            .Include(c => c.Vehicles)
            .OrderBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (clients, total);
    }

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

    public async Task DeleteAsync(Guid id)
    {
        var client = await db.Clients.FindAsync(id);
        if (client is not null)
        {
            db.Clients.Remove(client);
            await db.SaveChangesAsync();
        }
    }

    public async Task<bool> HasVehiclesByClientIdAsync(Guid clientId) =>
        await db.Vehicles.AnyAsync(v => v.ClientId == clientId);
}
