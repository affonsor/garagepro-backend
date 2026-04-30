using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using MediatR;

namespace GaragePro.Application.Features.Appointments.GetAll;

public record GetAppointmentsQuery(
    DateOnly? StartDate,
    DateOnly? EndDate,
    AppointmentStatus? Status = null,
    Guid? ClientId = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<AppointmentListResponse>>;
