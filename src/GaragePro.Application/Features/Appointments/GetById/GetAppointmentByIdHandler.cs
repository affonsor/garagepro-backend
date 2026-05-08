using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Appointments.GetById;

public class GetAppointmentByIdHandler(IAppointmentRepository appointmentRepository)
    : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDetailResponse>>
{
    public async Task<Result<AppointmentDetailResponse>> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.Id, cancellationToken);

        if (appointment is null)
            return Result<AppointmentDetailResponse>.NotFound("Agendamento não encontrado.");

        var history = appointment.RescheduleHistory
            .OrderBy(h => h.ChangedAt)
            .Select(h => new AppointmentRescheduleHistoryResponse(
                h.Id, h.PreviousStartAt, h.PreviousExpectedEndAt,
                h.NewStartAt, h.NewExpectedEndAt, h.Reason,
                h.ChangedBy.Name, h.ChangedAt));

        var response = new AppointmentDetailResponse(
            appointment.Id, appointment.ClientId, appointment.Client.Name,
            appointment.VehicleId, appointment.Vehicle.LicensePlate, appointment.Vehicle.Model, appointment.Vehicle.Type,
            appointment.ProductId, appointment.Product.Name,
            appointment.ServiceId, appointment.Service.Name,
            appointment.StartAt, appointment.ExpectedEndAt,
            appointment.Status, appointment.IsRescheduled, appointment.RescheduleCount,
            appointment.ProductValueSnapshot, appointment.ServiceValueSnapshot,
            appointment.TotalValue, appointment.Notes,
            appointment.CreatedAt, appointment.UpdatedAt, history);

        return Result<AppointmentDetailResponse>.Success(response);
    }
}
