using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Create;

public class CreateVehicleHandler(
    IClientRepository clientRepository,
    IVehicleRepository vehicleRepository) : IRequestHandler<CreateVehicleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId);
        if (client is null)
            return Result<Guid>.NotFound("Client not found");

        if (await vehicleRepository.ExistsByLicensePlateAsync(request.LicensePlate))
            return Result<Guid>.Failure("License plate already registered");

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            LicensePlate = request.LicensePlate,
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Color = request.Color,
            VIN = request.VIN,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await vehicleRepository.CreateAsync(vehicle);
        return Result<Guid>.Success(vehicle.Id);
    }
}
