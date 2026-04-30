using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.GetById;

public class GetVehicleByIdHandler(IVehicleRepository vehicleRepository) : IRequestHandler<GetVehicleByIdQuery, Result<VehicleDetailResponse>>
{
    public async Task<Result<VehicleDetailResponse>> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(request.Id);
        if (vehicle is null)
            return Result<VehicleDetailResponse>.NotFound("Vehicle not found");

        var response = new VehicleDetailResponse(
            vehicle.Id,
            vehicle.LicensePlate,
            vehicle.Make,
            vehicle.Model,
            vehicle.Year,
            vehicle.Color,
            vehicle.VIN,
            new ClientRefResponse(vehicle.Client.Id, vehicle.Client.Name),
            vehicle.TransferHistory.Select(t => new TransferHistoryResponse(
                t.Id,
                new ClientRefResponse(t.FromClientId, t.FromClient.Name),
                new ClientRefResponse(t.ToClientId, t.ToClient.Name),
                t.TransferredAt,
                t.Notes)),
            vehicle.CreatedAt,
            vehicle.UpdatedAt);

        return Result<VehicleDetailResponse>.Success(response);
    }
}
