using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.GetById;

public record GetVehicleByIdQuery(Guid Id) : IRequest<Result<VehicleDetailResponse>>;
