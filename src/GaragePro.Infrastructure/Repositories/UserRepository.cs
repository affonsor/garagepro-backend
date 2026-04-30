using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id) =>
        await db.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var total = await db.Users.CountAsync();
        var users = await db.Users
            .OrderBy(u => u.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (users, total);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null)
    {
        var query = db.Users.Where(u => u.Email == email);
        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task CreateAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        db.Users.Update(user);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is not null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
    }
}
