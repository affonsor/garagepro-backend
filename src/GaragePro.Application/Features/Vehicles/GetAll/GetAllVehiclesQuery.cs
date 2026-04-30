using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.GetAll;

public record GetAllVehiclesQuery(int PageNumber, int PageSize, Guid? ClientId) : IRequest<Result<PaginatedResult<VehicleSummaryResponse>>>;
