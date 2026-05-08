using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.API.Endpoints;

public static class ServiceOrdersEndpoints
{
    public static IEndpointRouteBuilder MapServiceOrdersEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/service-orders")
            .RequireAuthorization()
            .WithTags("ServiceOrders");

        group.MapGet("/", async (
            AppDbContext db,
            OrderStatus? status = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            string? search = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = BaseQuery(db);
            query = ApplyFilters(query, status, dateFrom, dateTo, search);

            var total = await query.CountAsync(ct);
            var orders = await query
                .OrderBy(o => o.ScheduledAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Results.Ok(new PaginatedResult<ServiceOrderResponse>(
                orders.Select(ToResponse),
                pageNumber,
                pageSize,
                total));
        })
        .Produces<PaginatedResult<ServiceOrderResponse>>(StatusCodes.Status200OK);

        group.MapGet("/summary", async (AppDbContext db, CancellationToken ct) =>
        {
            var counts = await db.ServiceOrders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            int Count(OrderStatus s) => counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0;

            var response = new ServiceOrderSummaryResponse(
                Count(OrderStatus.Scheduled),
                Count(OrderStatus.InProgress),
                Count(OrderStatus.Approval),
                Count(OrderStatus.Completed),
                await db.ServiceOrders.CountAsync(o => o.Status != OrderStatus.Completed, ct));

            return Results.Ok(response);
        })
        .Produces<ServiceOrderSummaryResponse>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db, CancellationToken ct) =>
        {
            var order = await BaseQuery(db)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            return order is null
                ? Results.NotFound(new { error = "Service order not found" })
                : Results.Ok(ToDetailResponse(order));
        })
        .Produces<ServiceOrderDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateServiceOrderRequest request, AppDbContext db, CancellationToken ct) =>
        {
            if (request.BoxNumber is < 1 or > 6)
                return Results.BadRequest(new { error = "BoxNumber must be between 1 and 6" });

            var requestedServices = request.Services ?? [];
            var requestedProducts = request.Products ?? [];

            if (requestedServices.Count == 0)
                return Results.BadRequest(new { error = "At least one service is required" });

            var client = await db.Clients.FindAsync([request.ClientId], ct);
            if (client is null)
                return Results.NotFound(new { error = "Client not found" });

            var vehicle = await db.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId, ct);
            if (vehicle is null)
                return Results.NotFound(new { error = "Vehicle not found" });

            if (vehicle.ClientId != client.Id)
                return Results.BadRequest(new { error = "Vehicle does not belong to the selected client" });

            var serviceIds = requestedServices.Select(s => s.ServiceId).Distinct().ToArray();
            var services = await db.Services
                .Where(s => serviceIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, ct);
            if (services.Count != serviceIds.Length)
                return Results.NotFound(new { error = "One or more services were not found" });

            var productIds = requestedProducts.Select(p => p.ProductId).Distinct().ToArray();
            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, ct);
            if (products.Count != productIds.Length)
                return Results.NotFound(new { error = "One or more products were not found" });

            var now = DateTimeOffset.UtcNow;
            var order = new ServiceOrder
            {
                Id = Guid.NewGuid(),
                OrderNumber = await NextOrderNumberAsync(db, ct),
                ClientId = request.ClientId,
                VehicleId = request.VehicleId,
                BoxNumber = request.BoxNumber,
                ScheduledAt = request.ScheduledAt,
                Status = OrderStatus.Scheduled,
                Notes = request.Notes,
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var line in requestedServices)
            {
                var service = services[line.ServiceId];
                var quantity = line.Quantity <= 0 ? 1 : line.Quantity;
                var unitPrice = line.UnitPrice ?? service.Price;
                order.ServiceLines.Add(new ServiceOrderServiceLine
                {
                    Id = Guid.NewGuid(),
                    ServiceOrderId = order.Id,
                    ServiceId = service.Id,
                    ServiceName = service.Name,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    LineTotal = quantity * unitPrice
                });
            }

            foreach (var line in requestedProducts)
            {
                var product = products[line.ProductId];
                var quantity = line.Quantity <= 0 ? 1 : line.Quantity;
                var unitPrice = line.UnitPrice ?? product.Price;
                order.ProductLines.Add(new ServiceOrderProductLine
                {
                    Id = Guid.NewGuid(),
                    ServiceOrderId = order.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    LineTotal = quantity * unitPrice
                });
            }

            order.TotalPrice = order.ServiceLines.Sum(l => l.LineTotal) + order.ProductLines.Sum(l => l.LineTotal);

            db.ServiceOrders.Add(order);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/service-orders/{order.Id}", new { order.Id, order.OrderNumber });
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/status", async (
            Guid id,
            UpdateServiceOrderStatusRequest request,
            AppDbContext db,
            CancellationToken ct) =>
        {
            var order = await db.ServiceOrders
                .Include(o => o.ProductLines).ThenInclude(l => l.Product)
                .Include(o => o.ServiceLines).ThenInclude(l => l.Service).ThenInclude(s => s.Materials).ThenInclude(m => m.Product)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            if (order is null)
                return Results.NotFound(new { error = "Service order not found" });

            if (order.Status == OrderStatus.Completed && request.Status != OrderStatus.Completed)
                return Results.BadRequest(new { error = "Completed service orders cannot leave Completed status" });

            var shouldConsumeStock = order.Status != OrderStatus.Completed && request.Status == OrderStatus.Completed;

            order.Status = request.Status;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            if (shouldConsumeStock)
                ConsumeStock(order);

            await db.SaveChangesAsync(ct);
            return Results.Ok(new { order.Id });
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return routes;
    }

    private static IQueryable<ServiceOrder> BaseQuery(AppDbContext db) =>
        db.ServiceOrders
            .Include(o => o.Client)
            .Include(o => o.Vehicle)
            .Include(o => o.ServiceLines)
            .Include(o => o.ProductLines);

    private static IQueryable<ServiceOrder> ApplyFilters(
        IQueryable<ServiceOrder> query,
        OrderStatus? status,
        DateOnly? dateFrom,
        DateOnly? dateTo,
        string? search)
    {
        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (dateFrom.HasValue)
        {
            var from = new DateTimeOffset(dateFrom.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            query = query.Where(o => o.ScheduledAt >= from);
        }

        if (dateTo.HasValue)
        {
            var to = new DateTimeOffset(dateTo.Value.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            query = query.Where(o => o.ScheduledAt <= to);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(term) ||
                o.Client.Name.ToLower().Contains(term) ||
                o.Vehicle.LicensePlate.ToLower().Contains(term) ||
                o.Vehicle.Model.ToLower().Contains(term) ||
                o.ServiceLines.Any(l => l.ServiceName.ToLower().Contains(term)));
        }

        return query;
    }

    private static ServiceOrderResponse ToResponse(ServiceOrder order) => new(
        order.Id,
        order.OrderNumber,
        order.Client.Name,
        order.Vehicle.Model,
        order.Vehicle.LicensePlate,
        order.BoxNumber,
        order.ServiceLines.OrderBy(l => l.Id).FirstOrDefault()?.ServiceName ?? string.Empty,
        order.TotalPrice,
        order.Status,
        order.ScheduledAt);

    private static ServiceOrderDetailResponse ToDetailResponse(ServiceOrder order) => new(
        ToResponse(order),
        order.ClientId,
        order.VehicleId,
        order.Notes,
        order.ServiceLines
            .OrderBy(l => l.ServiceName)
            .Select(l => new ServiceOrderServiceLineResponse(l.ServiceId, l.ServiceName, l.Quantity, l.UnitPrice, l.LineTotal)),
        order.ProductLines
            .OrderBy(l => l.ProductName)
            .Select(l => new ServiceOrderProductLineResponse(l.ProductId, l.ProductName, l.Quantity, l.UnitPrice, l.LineTotal)),
        order.CreatedAt,
        order.UpdatedAt);

    private static async Task<string> NextOrderNumberAsync(AppDbContext db, CancellationToken ct)
    {
        var count = await db.ServiceOrders.CountAsync(ct);
        return $"OS-{count + 1:00000}";
    }

    private static void ConsumeStock(ServiceOrder order)
    {
        foreach (var line in order.ProductLines)
            line.Product.Stock = Math.Max(0, line.Product.Stock - line.Quantity);

        foreach (var serviceLine in order.ServiceLines)
        {
            foreach (var material in serviceLine.Service.Materials)
            {
                var quantity = material.Quantity * serviceLine.Quantity;
                material.Product.Stock = Math.Max(0, material.Product.Stock - quantity);
            }
        }
    }
}

public record ServiceOrderResponse(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string VehicleModel,
    string LicensePlate,
    int BoxNumber,
    string PrimaryServiceName,
    decimal TotalPrice,
    OrderStatus Status,
    DateTimeOffset ScheduledAt);

public record ServiceOrderDetailResponse(
    ServiceOrderResponse Order,
    Guid ClientId,
    Guid VehicleId,
    string? Notes,
    IEnumerable<ServiceOrderServiceLineResponse> Services,
    IEnumerable<ServiceOrderProductLineResponse> Products,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record ServiceOrderServiceLineResponse(
    Guid ServiceId,
    string ServiceName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record ServiceOrderProductLineResponse(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record ServiceOrderSummaryResponse(
    int Scheduled,
    int InProgress,
    int Approval,
    int Completed,
    int ActiveTotal);

public record CreateServiceOrderRequest(
    Guid ClientId,
    Guid VehicleId,
    int BoxNumber,
    DateTimeOffset ScheduledAt,
    List<CreateServiceOrderServiceLineRequest>? Services,
    List<CreateServiceOrderProductLineRequest>? Products,
    string? Notes);

public record CreateServiceOrderServiceLineRequest(Guid ServiceId, decimal Quantity = 1, decimal? UnitPrice = null);

public record CreateServiceOrderProductLineRequest(Guid ProductId, decimal Quantity = 1, decimal? UnitPrice = null);

public record UpdateServiceOrderStatusRequest(OrderStatus Status);
