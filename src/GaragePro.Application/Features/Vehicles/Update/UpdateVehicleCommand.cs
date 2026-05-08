using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Update;

public record UpdateVehicleCommand(
    Guid Id,
    string Make,
    string Model,
    VehicleType Type,
    int Year,
    string? Color,
    string? VIN) : IRequest<Result<Guid>>;
