using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Appointments.Complete;

public record CompleteAppointmentCommand(Guid Id) : IRequest<Result<Guid>>;
