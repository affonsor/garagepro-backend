using GaragePro.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GaragePro.API.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        logger.LogInformation("Applying database migrations");
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied");
    }
}
