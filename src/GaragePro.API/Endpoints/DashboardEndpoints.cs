using GaragePro.Core.Enums;
using GaragePro.Core.Entities;
using GaragePro.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.API.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder routes)
    {
        var dashboard = routes.MapGroup("/dashboard")
            .RequireAuthorization()
            .WithTags("Dashboard");

        dashboard.MapGet("/summary", async (DateOnly? date, AppDbContext db, CancellationToken ct) =>
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);
            var previousDate = targetDate.AddDays(-1);
            var todayQuery = OrdersForDate(db, targetDate);
            var previousQuery = OrdersForDate(db, previousDate);

            var revenue = await todayQuery
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalPrice, ct);

            var previousRevenue = await previousQuery
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalPrice, ct);

            var activeOrders = await db.ServiceOrders.CountAsync(o => o.Status != OrderStatus.Completed, ct);
            var delayedOrders = await db.ServiceOrders.CountAsync(
                o => o.Status != OrderStatus.Completed && o.ScheduledAt < DateTimeOffset.UtcNow,
                ct);

            var occupiedBoxes = await todayQuery
                .Where(o => o.Status != OrderStatus.Completed)
                .Select(o => o.BoxNumber)
                .Distinct()
                .CountAsync(ct);

            var completedCount = await todayQuery.CountAsync(o => o.Status == OrderStatus.Completed, ct);
            var averageTicket = completedCount == 0 ? 0 : revenue / completedCount;
            var revenueDeltaPercent = previousRevenue == 0
                ? (revenue > 0 ? 100 : 0)
                : Math.Round(((revenue - previousRevenue) / previousRevenue) * 100, 2);

            return Results.Ok(new DashboardSummaryResponse(
                targetDate,
                revenue,
                revenueDeltaPercent,
                activeOrders,
                delayedOrders,
                occupiedBoxes,
                6,
                Math.Round((occupiedBoxes / 6m) * 100, 2),
                averageTicket));
        })
        .Produces<DashboardSummaryResponse>(StatusCodes.Status200OK);

        dashboard.MapGet("/agenda", async (DateOnly? date, int take, AppDbContext db, CancellationToken ct) =>
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);
            take = Math.Clamp(take <= 0 ? 5 : take, 1, 50);

            var orders = await OrdersForDate(db, targetDate)
                .Include(o => o.Client)
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceLines)
                .OrderBy(o => o.ScheduledAt)
                .Take(take)
                .ToListAsync(ct);

            return Results.Ok(orders.Select(o => new DashboardAgendaItemResponse(
                o.Id,
                o.OrderNumber,
                o.ScheduledAt,
                o.Client.Name,
                o.Vehicle.Model,
                o.Vehicle.LicensePlate,
                o.ServiceLines.OrderBy(l => l.Id).FirstOrDefault()?.ServiceName ?? string.Empty,
                o.Status)));
        })
        .Produces<IEnumerable<DashboardAgendaItemResponse>>(StatusCodes.Status200OK);

        routes.MapGet("/boxes/occupancy", async (DateOnly? date, AppDbContext db, CancellationToken ct) =>
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime);
            var orders = await OrdersForDate(db, targetDate)
                .Include(o => o.Vehicle)
                .Include(o => o.ServiceLines)
                .OrderByDescending(o => o.ScheduledAt)
                .ToListAsync(ct);

            var response = Enumerable.Range(1, 6)
                .Select(box =>
                {
                    var order = orders.FirstOrDefault(o => o.BoxNumber == box);
                    return order is null
                        ? new BoxOccupancyResponse(box, null, null, null, 0, null)
                        : new BoxOccupancyResponse(
                            box,
                            order.Id,
                            order.ServiceLines.OrderBy(l => l.Id).FirstOrDefault()?.ServiceName,
                            order.Vehicle.LicensePlate,
                            ProgressFor(order.Status),
                            order.Status);
                });

            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithTags("Boxes")
        .Produces<IEnumerable<BoxOccupancyResponse>>(StatusCodes.Status200OK);

        return routes;
    }

    private static IQueryable<ServiceOrder> OrdersForDate(AppDbContext db, DateOnly date)
    {
        var from = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var to = new DateTimeOffset(date.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
        return db.ServiceOrders.Where(o => o.ScheduledAt >= from && o.ScheduledAt <= to);
    }

    private static int ProgressFor(OrderStatus status) => status switch
    {
        OrderStatus.Completed => 100,
        OrderStatus.Approval => 90,
        OrderStatus.InProgress => 70,
        _ => 30
    };
}

public record DashboardSummaryResponse(
    DateOnly Date,
    decimal Revenue,
    decimal RevenueDeltaPercent,
    int ActiveOrders,
    int DelayedOrders,
    int OccupiedBoxes,
    int TotalBoxes,
    decimal BoxCapacityPercent,
    decimal AverageTicket);

public record DashboardAgendaItemResponse(
    Guid Id,
    string OrderNumber,
    DateTimeOffset ScheduledAt,
    string CustomerName,
    string VehicleModel,
    string LicensePlate,
    string ServiceName,
    OrderStatus Status);

public record BoxOccupancyResponse(
    int BoxNumber,
    Guid? ServiceOrderId,
    string? ServiceName,
    string? LicensePlate,
    int ProgressPercent,
    OrderStatus? Status);
