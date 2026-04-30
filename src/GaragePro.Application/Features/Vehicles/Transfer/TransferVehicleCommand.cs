using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Transfer;

public record TransferVehicleCommand(Guid VehicleId, Guid ToClientId, string? Notes) : IRequest<Result<TransferResponse>>;
