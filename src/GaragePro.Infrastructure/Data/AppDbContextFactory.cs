using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GaragePro.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder
            .UseNpgsql("Host=localhost;Port=5432;Database=garagepro;Username=garagepro;Password=garagepro123")
            .UseSnakeCaseNamingConvention();
        return new AppDbContext(optionsBuilder.Options);
    }
}
