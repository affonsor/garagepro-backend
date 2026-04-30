using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Appointments.GetAll;

public class GetAppointmentsHandler(IAppointmentRepository appointmentRepository)
    : IRequestHandler<GetAppointmentsQuery, Result<AppointmentListResponse>>
{
    public async Task<Result<AppointmentListResponse>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await appointmentRepository.GetAllAsync(
            request.StartDate, request.EndDate, request.Status,
            request.ClientId, request.Search,
            request.PageNumber, request.PageSize, cancellationToken);

        var summary = await appointmentRepository.GetSummaryAsync(
            request.StartDate, request.EndDate, request.Status,
            request.ClientId, request.Search, cancellationToken);

        var responses = items.Select(a => new AppointmentSummaryResponse(
            a.Id, a.Client.Name, a.Product.Name, a.Service.Name,
            a.StartAt, a.ExpectedEndAt, a.Status, a.IsRescheduled,
            a.TotalValue, a.Notes));

        var paginated = new PaginatedResult<AppointmentSummaryResponse>(
            responses, request.PageNumber, request.PageSize, totalCount);

        var summaryDto = new AppointmentSummaryDto(
            summary.ScheduledCount, summary.ScheduledTotal,
            summary.CompletedCount, summary.CompletedTotal,
            summary.CanceledCount, summary.CanceledTotal);

        return Result<AppointmentListResponse>.Success(new AppointmentListResponse(paginated, summaryDto));
    }
}
