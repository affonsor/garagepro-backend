using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using GaragePro.Core.Exceptions;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Appointments.Cancel;

public class CancelAppointmentHandler(IAppointmentRepository appointmentRepository)
    : IRequestHandler<CancelAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (appointment is null)
            return Result<Guid>.NotFound("Agendamento não encontrado.");

        if (appointment.Status != AppointmentStatus.Scheduled)
            return Result<Guid>.Failure("Apenas agendamentos 'A realizar' podem ser cancelados.");

        appointment.Status = AppointmentStatus.Canceled;
        appointment.UpdatedAt = DateTimeOffset.UtcNow;

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
