using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Appointments.Reschedule;

public record RescheduleAppointmentCommand(
    Guid Id,
    DateTimeOffset NewStartAt,
    DateTimeOffset NewExpectedEndAt,
    string? Reason) : IRequest<Result<Guid>>;
