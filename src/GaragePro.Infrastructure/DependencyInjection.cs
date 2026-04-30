using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Core.Interfaces.Services;
using GaragePro.Infrastructure.Data;
using GaragePro.Infrastructure.Repositories;
using GaragePro.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GaragePro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Default"))
                   .UseSnakeCaseNamingConvention());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        return services;
    }
}
