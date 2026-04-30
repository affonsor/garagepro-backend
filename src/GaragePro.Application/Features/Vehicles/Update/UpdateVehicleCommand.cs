using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Vehicles.Update;

public record UpdateVehicleCommand(
    Guid Id,
    string Make,
    string Model,
    int Year,
    string? Color,
    string? VIN) : IRequest<Result<Guid>>;
