using GaragePro.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.API.Endpoints;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/search", async (string? query, AppDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(query))
                return Results.Ok(new GlobalSearchResponse([], [], []));

            var term = query.Trim().ToLower();

            var clients = await db.Clients
                .Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    (c.Phone != null && c.Phone.Contains(query.Trim())) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)))
                .OrderBy(c => c.Name)
                .Take(5)
                .Select(c => new SearchResultResponse(c.Id, c.Name, c.Phone))
                .ToListAsync(ct);

            var vehicles = await db.Vehicles
                .Where(v =>
                    v.LicensePlate.ToLower().Contains(term) ||
                    v.Model.ToLower().Contains(term) ||
                    v.Make.ToLower().Contains(term))
                .OrderBy(v => v.LicensePlate)
                .Take(5)
                .Select(v => new SearchResultResponse(v.Id, v.LicensePlate, $"{v.Make} {v.Model}"))
                .ToListAsync(ct);

            var serviceOrders = await db.ServiceOrders
                .Include(o => o.Client)
                .Include(o => o.Vehicle)
                .Where(o =>
                    o.OrderNumber.ToLower().Contains(term) ||
                    o.Client.Name.ToLower().Contains(term) ||
                    o.Vehicle.LicensePlate.ToLower().Contains(term))
                .OrderByDescending(o => o.ScheduledAt)
                .Take(5)
                .Select(o => new SearchResultResponse(o.Id, o.OrderNumber, $"{o.Client.Name} - {o.Vehicle.LicensePlate}"))
                .ToListAsync(ct);

            return Results.Ok(new GlobalSearchResponse(clients, vehicles, serviceOrders));
        })
        .RequireAuthorization()
        .WithTags("Search")
        .Produces<GlobalSearchResponse>(StatusCodes.Status200OK);

        return routes;
    }
}

public record GlobalSearchResponse(
    IEnumerable<SearchResultResponse> Clients,
    IEnumerable<SearchResultResponse> Vehicles,
    IEnumerable<SearchResultResponse> ServiceOrders);

public record SearchResultResponse(Guid Id, string Label, string? Description);
