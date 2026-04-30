using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Delete;

public record DeleteVehicleCommand(Guid Id) : IRequest<Result<bool>>;
