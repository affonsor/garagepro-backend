using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace GaragePro.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "GaragePro API", Version = "v1" });

            options.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT Bearer token"
            });

            options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("BearerAuth", doc), [] }
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    public static WebApplication UseSwaggerInDevelopment(this WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GaragePro API v1"));
        }

        return app;
    }
}
