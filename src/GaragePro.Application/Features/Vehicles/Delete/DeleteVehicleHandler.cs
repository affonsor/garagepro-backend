using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Delete;

public class DeleteVehicleHandler(IVehicleRepository vehicleRepository) : IRequestHandler<DeleteVehicleCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.Id);
        if (vehicle is null)
            return Result<bool>.NotFound("Vehicle not found");

        if (await vehicleRepository.HasTransferHistoryAsync(request.Id))
            return Result<bool>.Failure("Vehicle has transfer history and cannot be deleted");

        await vehicleRepository.DeleteAsync(request.Id);
        return Result<bool>.Success(true);
    }
}
