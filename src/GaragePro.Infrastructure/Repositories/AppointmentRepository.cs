using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Exceptions;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Repositories;

public class AppointmentRepository(AppDbContext context) : IAppointmentRepository
{
    public async Task<(IEnumerable<Appointment> Items, int TotalCount)> GetAllAsync(
        DateOnly? startDate, DateOnly? endDate, AppointmentStatus? status,
        Guid? clientId, string? search, int pageNumber, int pageSize, CancellationToken ct)
    {
        var query = context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Vehicle)
            .Include(a => a.Product)
            .Include(a => a.Service)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => DateOnly.FromDateTime(a.StartAt.Date) >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(a => DateOnly.FromDateTime(a.StartAt.Date) <= endDate.Value);
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        if (clientId.HasValue)
            query = query.Where(a => a.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a =>
                a.Client.Name.Contains(search) ||
                a.Product.Name.Contains(search) ||
                a.Service.Name.Contains(search));

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(a => a.StartAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<AppointmentSummaryData> GetSummaryAsync(
        DateOnly? startDate, DateOnly? endDate, AppointmentStatus? status,
        Guid? clientId, string? search, CancellationToken ct)
    {
        var query = context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Vehicle)
            .Include(a => a.Product)
            .Include(a => a.Service)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => DateOnly.FromDateTime(a.StartAt.Date) >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(a => DateOnly.FromDateTime(a.StartAt.Date) <= endDate.Value);
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        if (clientId.HasValue)
            query = query.Where(a => a.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a =>
                a.Client.Name.Contains(search) ||
                a.Product.Name.Contains(search) ||
                a.Service.Name.Contains(search));

        var scheduledCount = await query.CountAsync(a => a.Status == AppointmentStatus.Scheduled, ct);
        var scheduledTotal = await query.Where(a => a.Status == AppointmentStatus.Scheduled).SumAsync(a => a.TotalValue, ct);
        var completedCount = await query.CountAsync(a => a.Status == AppointmentStatus.Completed, ct);
        var completedTotal = await query.Where(a => a.Status == AppointmentStatus.Completed).SumAsync(a => a.TotalValue, ct);
        var canceledCount = await query.CountAsync(a => a.Status == AppointmentStatus.Canceled, ct);
        var canceledTotal = await query.Where(a => a.Status == AppointmentStatus.Canceled).SumAsync(a => a.TotalValue, ct);

        return new AppointmentSummaryData(
            scheduledCount, scheduledTotal,
            completedCount, completedTotal,
            canceledCount, canceledTotal);
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await context.Appointments
            .Include(a => a.Client)
            .Include(a => a.Vehicle)
            .Include(a => a.Product)
            .Include(a => a.Service)
            .Include(a => a.RescheduleHistory).ThenInclude(h => h.ChangedBy)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Guid> AddAsync(Appointment appointment, CancellationToken ct)
    {
        context.Add(appointment);
        await context.SaveChangesAsync(ct);
        return appointment.Id;
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken ct)
    {
        context.Update(appointment);
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException("O agendamento foi modificado por outro usuário. Recarregue e tente novamente.");
        }
    }
}
