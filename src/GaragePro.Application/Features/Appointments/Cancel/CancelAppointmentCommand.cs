using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Appointments.Cancel;

public record CancelAppointmentCommand(Guid Id) : IRequest<Result<Guid>>;
