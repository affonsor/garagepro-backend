using System.Text.Json.Serialization;
using GaragePro.API;
using GaragePro.API.Endpoints;
using GaragePro.API.Extensions;
using GaragePro.Application;
using GaragePro.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
const string angularDevClientCorsPolicy = "AngularDevClient";

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddCors(options =>
{
    options.AddPolicy(angularDevClientCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(config);
builder.Services.AddJwtBearerAuthentication(config);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();

app.UseExceptionHandler();
app.UseCors(angularDevClientCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

var api = app.MapGroup("/api");
api.MapAuthEndpoints();
api.MapUsersEndpoints();
api.MapClientsEndpoints();
api.MapVehiclesEndpoints();
api.MapProductsEndpoints();
api.MapServicesEndpoints();
api.MapAppointmentsEndpoints();

app.UseSwaggerInDevelopment(app.Environment);

if (app.Environment.IsDevelopment())
{
    await app.SeedAdminUserAsync();
}

app.Run();
