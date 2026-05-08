using GaragePro.Core.Entities;
using GaragePro.Core.Enums;

namespace GaragePro.Application.Features.Services;

public record ServiceVehiclePriceDto(VehicleType VehicleType, decimal Price);

public record ServiceMaterialResponse(Guid ProductId, string ProductName, decimal Quantity);

public record ServiceMaterialInput(Guid ProductId, decimal Quantity);

public record ServiceResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    IEnumerable<ServiceVehiclePriceDto> VehiclePrices,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? Code,
    string Tier,
    string? Category,
    string? Duration,
    decimal Cost,
    bool Active,
    IEnumerable<ServiceMaterialResponse> Materials,
    IEnumerable<string> Steps,
    int ExecutionCountMonth)
{
    public static ServiceResponse From(Service service, int executionCountMonth = 0) => new(
        service.Id,
        service.Name,
        service.Description,
        service.Price,
        service.VehiclePrices
            .OrderBy(p => p.VehicleType)
            .Select(p => new ServiceVehiclePriceDto(p.VehicleType, p.Price)),
        service.CreatedAt,
        service.UpdatedAt,
        service.Code,
        service.Tier,
        service.Category,
        service.Duration,
        service.Cost,
        service.IsActive,
        service.Materials
            .OrderBy(m => m.Product.Name)
            .Select(m => new ServiceMaterialResponse(m.ProductId, m.Product?.Name ?? string.Empty, m.Quantity)),
        service.Steps
            .OrderBy(s => s.Position)
            .Select(s => s.Description),
        executionCountMonth);
}
