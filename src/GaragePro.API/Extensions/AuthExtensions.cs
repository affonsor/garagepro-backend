using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GaragePro.API.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:Secret"]!))
                };
            });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
            .AddPolicy("TechnicianOrAdmin", policy => policy.RequireRole("Admin", "Technician"))
            .AddPolicy("FinancialOrAdmin", policy => policy.RequireRole("Admin", "Financial"));

        return services;
    }
}
