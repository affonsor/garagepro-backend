using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Core.Interfaces.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GaragePro.API.Extensions;

public static class SeedExtensions
{
    public static async Task SeedAdminUserAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

        if (await userRepository.ExistsByEmailAsync("admin@garagepro.com"))
            return;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@garagepro.com",
            PasswordHash = authService.HashPassword("Admin@1234"),
            Roles = [UserRole.Admin],
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.CreateAsync(admin);
    }
}
