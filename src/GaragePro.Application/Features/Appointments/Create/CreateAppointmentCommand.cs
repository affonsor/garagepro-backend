using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Appointments.Create;

public record CreateAppointmentCommand(
    Guid ClientId,
    Guid VehicleId,
    Guid ProductId,
    Guid ServiceId,
    DateTimeOffset StartAt,
    DateTimeOffset ExpectedEndAt,
    string? Notes) : IRequest<Result<Guid>>;
