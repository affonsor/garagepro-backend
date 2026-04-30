using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Appointments.GetById;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<Result<AppointmentDetailResponse>>;
