using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.GetAll;

public class GetAllVehiclesHandler(IVehicleRepository vehicleRepository) : IRequestHandler<GetAllVehiclesQuery, Result<PaginatedResult<VehicleSummaryResponse>>>
{
    public async Task<Result<PaginatedResult<VehicleSummaryResponse>>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
    {
        var (vehicles, total) = await vehicleRepository.GetAllAsync(request.PageNumber, request.PageSize, request.ClientId);

        var items = vehicles.Select(v => new VehicleSummaryResponse(
            v.Id,
            v.LicensePlate,
            v.Make,
            v.Model,
            v.Year,
            v.Color,
            new ClientRefResponse(v.Client.Id, v.Client.Name),
            v.CreatedAt));

        return Result<PaginatedResult<VehicleSummaryResponse>>.Success(
            new PaginatedResult<VehicleSummaryResponse>(items, total, request.PageNumber, request.PageSize));
    }
}
