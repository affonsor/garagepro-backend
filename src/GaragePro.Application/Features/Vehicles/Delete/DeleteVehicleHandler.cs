using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Delete;

public class DeleteVehicleHandler(
    IVehicleRepository vehicleRepository,
    IClientRepository clientRepository) : IRequestHandler<DeleteVehicleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.Id);
        if (vehicle is null)
            return Result<bool>.NotFound("Vehicle not found");

        if (await vehicleRepository.HasTransferHistoryAsync(request.Id))
            return Result<bool>.Failure("Vehicle has transfer history and cannot be deleted");

        if (await clientRepository.CountVehiclesByClientIdAsync(vehicle.ClientId) <= 1)
            return Result<bool>.Failure("Client must have at least one vehicle");

        await vehicleRepository.DeleteAsync(request.Id);
        return Result<bool>.Success(true);
    }
}
