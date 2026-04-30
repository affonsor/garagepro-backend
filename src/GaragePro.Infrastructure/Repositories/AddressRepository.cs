using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Repositories;

public class AddressRepository(AppDbContext db) : IAddressRepository
{
    public async Task<Address?> GetByIdAsync(Guid id) =>
        await db.Addresses.FindAsync(id);

    public async Task AddAsync(Address address)
    {
        db.Addresses.Add(address);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Address address)
    {
        db.Addresses.Update(address);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var address = await db.Addresses.FindAsync(id);
        if (address is not null)
        {
            db.Addresses.Remove(address);
            await db.SaveChangesAsync();
        }
    }

    public async Task<int> CountByClientIdAsync(Guid clientId) =>
        await db.Addresses.CountAsync(a => a.ClientId == clientId);
}
