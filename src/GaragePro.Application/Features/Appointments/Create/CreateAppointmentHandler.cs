using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Appointments.Create;

public class CreateAppointmentHandler(
    IAppointmentRepository appointmentRepository,
    IClientRepository clientRepository,
    IVehicleRepository vehicleRepository,
    IProductRepository productRepository,
    IServiceRepository serviceRepository)
    : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId);
        if (client is null || !client.IsActive)
            return Result<Guid>.Failure("Client not found or inactive");

        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle is null)
            return Result<Guid>.Failure("Vehicle not found");

        if (vehicle.ClientId != request.ClientId)
            return Result<Guid>.Failure("Vehicle does not belong to the informed client");

        var product = await productRepository.GetByIdAsync(request.ProductId);
        if (product is null || !product.IsActive)
            return Result<Guid>.Failure("Produto nao encontrado ou inativo.");

        var service = await serviceRepository.GetByIdAsync(request.ServiceId);
        if (service is null || !service.IsActive)
            return Result<Guid>.Failure("Servico nao encontrado ou inativo.");

        var servicePrice = service.VehiclePrices.FirstOrDefault(p => p.VehicleType == vehicle.Type);
        if (servicePrice is null)
            return Result<Guid>.Failure("Service does not have a price for the vehicle type");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            VehicleId = request.VehicleId,
            ProductId = request.ProductId,
            ServiceId = request.ServiceId,
            StartAt = request.StartAt,
            ExpectedEndAt = request.ExpectedEndAt,
            Status = AppointmentStatus.Scheduled,
            IsRescheduled = false,
            RescheduleCount = 0,
            ProductValueSnapshot = product.Price,
            ServiceValueSnapshot = servicePrice.Price,
            TotalValue = product.Price + servicePrice.Price,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        await appointmentRepository.AddAsync(appointment, cancellationToken);
        return Result<Guid>.Success(appointment.Id);
    }
}
