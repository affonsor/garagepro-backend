using GaragePro.Application.Common;
using GaragePro.Application.Features.Services;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Services.Create;

public class CreateServiceHandler(IServiceRepository serviceRepository) : IRequestHandler<CreateServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var vehiclePrices = ResolveVehiclePrices(request.VehiclePrices, request.Price);
        if (vehiclePrices.Count == 0)
            return Result<Guid>.ValidationFailure(["Either a service price or at least one vehicle price is required"]);

        var serviceId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var service = new Service
        {
            Id = serviceId,
            Name = request.Name,
            Code = NormalizeOptional(request.Code),
            Tier = NormalizeTier(request.Tier),
            Category = request.Category,
            Duration = request.Duration,
            Description = request.Description,
            Cost = request.Cost,
            Price = ResolveLegacyPrice(vehiclePrices),
            IsActive = request.Active,
            VehiclePrices = vehiclePrices.Select(p => new ServiceVehiclePrice
            {
                ServiceId = serviceId,
                VehicleType = p.VehicleType,
                Price = p.Price
            }).ToList(),
            Materials = (request.Materials ?? [])
                .GroupBy(m => m.ProductId)
                .Select(g => new ServiceMaterial
                {
                    Id = Guid.NewGuid(),
                    ServiceId = serviceId,
                    ProductId = g.Key,
                    Quantity = g.Sum(m => m.Quantity)
                }).ToList(),
            Steps = (request.Steps ?? [])
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select((step, index) => new ServiceStep
                {
                    Id = Guid.NewGuid(),
                    ServiceId = serviceId,
                    Position = index + 1,
                    Description = step.Trim()
                }).ToList(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await serviceRepository.CreateAsync(service);
        return Result<Guid>.Success(service.Id);
    }

    private static decimal ResolveLegacyPrice(IReadOnlyCollection<ServiceVehiclePriceDto> prices) =>
        prices.FirstOrDefault(p => p.VehicleType == VehicleType.Car)?.Price ?? prices.First().Price;

    private static List<ServiceVehiclePriceDto> ResolveVehiclePrices(
        IReadOnlyCollection<ServiceVehiclePriceDto>? vehiclePrices,
        decimal price)
    {
        if (vehiclePrices is { Count: > 0 })
            return vehiclePrices.ToList();

        return price > 0 ? [new ServiceVehiclePriceDto(VehicleType.Car, price)] : [];
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeTier(string? tier) =>
        string.IsNullOrWhiteSpace(tier) ? "standard" : tier.Trim();
}
