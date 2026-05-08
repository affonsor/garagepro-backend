using GaragePro.Application.Common;
using GaragePro.Application.Features.Services;
using MediatR;

namespace GaragePro.Application.Features.Services.Update;

public record UpdateServiceCommand(
    Guid Id,
    string Name,
    string? Description,
    List<ServiceVehiclePriceDto>? VehiclePrices = null,
    decimal Price = 0,
    string? Code = null,
    string Tier = "standard",
    string? Category = null,
    string? Duration = null,
    decimal Cost = 0,
    bool Active = true,
    List<ServiceMaterialInput>? Materials = null,
    List<string>? Steps = null) : IRequest<Result<Guid>>;
