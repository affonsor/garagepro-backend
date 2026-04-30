using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Create;

public record CreateVehicleCommand(
    Guid ClientId,
    string LicensePlate,
    string Make,
    string Model,
    int Year,
    string? Color,
    string? VIN) : IRequest<Result<Guid>>;
