using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Update;

public class UpdateVehicleHandler(IVehicleRepository vehicleRepository) : IRequestHandler<UpdateVehicleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.Id);
        if (vehicle is null)
            return Result<Guid>.NotFound("Vehicle not found");

        vehicle.Make = request.Make;
        vehicle.Model = request.Model;
        vehicle.Type = request.Type;
        vehicle.Year = request.Year;
        vehicle.Color = request.Color;
        vehicle.VIN = request.VIN;
        vehicle.UpdatedAt = DateTimeOffset.UtcNow;

        await vehicleRepository.UpdateAsync(vehicle);
        return Result<Guid>.Success(vehicle.Id);
    }
}
