using GaragePro.Core.Entities;
using GaragePro.Core.Enums;

namespace GaragePro.Core.Interfaces.Repositories;

public record AppointmentSummaryData(
    int ScheduledCount, decimal ScheduledTotal,
    int CompletedCount, decimal CompletedTotal,
    int CanceledCount, decimal CanceledTotal);

public interface IAppointmentRepository
{
    Task<(IEnumerable<Appointment> Items, int TotalCount)> GetAllAsync(
        DateOnly? startDate, DateOnly? endDate, AppointmentStatus? status,
        Guid? clientId, string? search, int pageNumber, int pageSize, CancellationToken ct);

    Task<AppointmentSummaryData> GetSummaryAsync(
        DateOnly? startDate, DateOnly? endDate, AppointmentStatus? status,
        Guid? clientId, string? search, CancellationToken ct);

    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Guid> AddAsync(Appointment appointment, CancellationToken ct);
    Task UpdateAsync(Appointment appointment, CancellationToken ct);
}
