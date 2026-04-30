using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Exceptions;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Appointments.Reschedule;

public class RescheduleAppointmentHandler(IAppointmentRepository appointmentRepository)
    : IRequestHandler<RescheduleAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
            return Result<Guid>.NotFound("Agendamento não encontrado.");

        if (appointment.Status != AppointmentStatus.Scheduled)
            return Result<Guid>.Failure("Apenas agendamentos 'A realizar' podem ser remarcados.");

        var history = new AppointmentRescheduleHistory
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointment.Id,
            PreviousStartAt = appointment.StartAt,
            PreviousExpectedEndAt = appointment.ExpectedEndAt,
            NewStartAt = request.NewStartAt,
            NewExpectedEndAt = request.NewExpectedEndAt,
            Reason = request.Reason,
            ChangedByUserId = Guid.Empty,
            ChangedAt = DateTimeOffset.UtcNow,
        };

        appointment.StartAt = request.NewStartAt;
        appointment.ExpectedEndAt = request.NewExpectedEndAt;
        appointment.IsRescheduled = true;
        appointment.RescheduleCount++;
        appointment.UpdatedAt = DateTimeOffset.UtcNow;
        appointment.RescheduleHistory.Add(history);

        try
        {
            await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        }
        catch (ConcurrencyException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }

        return Result<Guid>.Success(appointment.Id);
    }
}
