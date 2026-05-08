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

        if (!client.IsActive)
            return Result<Guid>.Failure("Inactive clients cannot receive vehicles");

        var licensePlate = NormalizeLicensePlate(request.LicensePlate);

        if (await vehicleRepository.ExistsByLicensePlateAsync(licensePlate))
            return Result<Guid>.Conflict("License plate already registered");

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            LicensePlate = licensePlate,
            Make = request.Make,
            Model = request.Model,
            Type = request.Type,
            Year = request.Year,
            Color = request.Color,
            VIN = request.VIN,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await vehicleRepository.CreateAsync(vehicle);
        return Result<Guid>.Success(vehicle.Id);
    }

    private static string NormalizeLicensePlate(string licensePlate) =>
        licensePlate.Trim().ToUpperInvariant();
}
