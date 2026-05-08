using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Transfer;

public class TransferVehicleHandler(
    IVehicleRepository vehicleRepository,
    IClientRepository clientRepository) : IRequestHandler<TransferVehicleCommand, Result<TransferResponse>>
{
    public async Task<Result<TransferResponse>> Handle(TransferVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle is null)
            return Result<TransferResponse>.NotFound("Vehicle not found");

        if (vehicle.ClientId == request.ToClientId)
            return Result<TransferResponse>.Failure("Target client is the current owner");

        if (!vehicle.Client.IsActive)
            return Result<TransferResponse>.Failure("Inactive clients cannot transfer vehicles");

        if (await clientRepository.CountVehiclesByClientIdAsync(vehicle.ClientId) <= 1)
            return Result<TransferResponse>.Failure("Client must have at least one vehicle");

        var targetClient = await clientRepository.GetByIdAsync(request.ToClientId);
        if (targetClient is null)
            return Result<TransferResponse>.NotFound("Target client not found");

        if (!targetClient.IsActive)
            return Result<TransferResponse>.Failure("Inactive clients cannot receive vehicles");

        var (recordId, transferredAt) = await vehicleRepository.TransferAsync(
            request.VehicleId, request.ToClientId, request.Notes);

        return Result<TransferResponse>.Success(new TransferResponse(recordId, transferredAt));
    }
}
