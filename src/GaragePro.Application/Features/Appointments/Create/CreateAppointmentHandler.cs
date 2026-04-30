using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Appointments.Create;

public class CreateAppointmentHandler(
    IAppointmentRepository appointmentRepository,
    IClientRepository clientRepository,
    IProductRepository productRepository,
    IServiceRepository serviceRepository)
    : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId);
        if (client is null)
            return Result<Guid>.Failure("Cliente não encontrado.");

        var product = await productRepository.GetByIdAsync(request.ProductId);
        if (product is null || !product.IsActive)
            return Result<Guid>.Failure("Produto não encontrado ou inativo.");

        var service = await serviceRepository.GetByIdAsync(request.ServiceId);
        if (service is null || !service.IsActive)
            return Result<Guid>.Failure("Serviço não encontrado ou inativo.");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            ProductId = request.ProductId,
            ServiceId = request.ServiceId,
            StartAt = request.StartAt,
            ExpectedEndAt = request.ExpectedEndAt,
            Status = AppointmentStatus.Scheduled,
            IsRescheduled = false,
            RescheduleCount = 0,
            ProductValueSnapshot = product.Price,
            ServiceValueSnapshot = service.Price,
            TotalValue = product.Price + service.Price,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        await appointmentRepository.AddAsync(appointment, cancellationToken);
        return Result<Guid>.Success(appointment.Id);
    }
}
