using GaragePro.Application.Common;
using GaragePro.Application.Features.Services;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Services.Update;

public class UpdateServiceHandler(IServiceRepository serviceRepository) : IRequestHandler<UpdateServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(request.Id);
        if (service is null)
            return Result<Guid>.NotFound("Service not found");

        var vehiclePrices = ResolveVehiclePrices(request.VehiclePrices, request.Price);
        if (vehiclePrices.Count == 0)
            return Result<Guid>.ValidationFailure(["Either a service price or at least one vehicle price is required"]);

        service.Name = request.Name;
        service.Code = NormalizeOptional(request.Code);
        service.Tier = NormalizeTier(request.Tier);
        service.Category = request.Category;
        service.Duration = request.Duration;
        service.Description = request.Description;
        service.Cost = request.Cost;
        service.Price = ResolveLegacyPrice(vehiclePrices);
        service.IsActive = request.Active;
        SyncVehiclePrices(service, vehiclePrices);
        SyncMaterials(service, request.Materials ?? []);
        SyncSteps(service, request.Steps ?? []);
        service.UpdatedAt = DateTimeOffset.UtcNow;

        await serviceRepository.UpdateAsync(service);
        return Result<Guid>.Success(service.Id);
    }

    private static void SyncVehiclePrices(Service service, IReadOnlyCollection<ServiceVehiclePriceDto> vehiclePrices)
    {
        var requestedByType = vehiclePrices.ToDictionary(p => p.VehicleType);
        var removed = service.VehiclePrices
            .Where(existing => !requestedByType.ContainsKey(existing.VehicleType))
            .ToList();

        foreach (var price in removed)
            service.VehiclePrices.Remove(price);

        foreach (var requested in vehiclePrices)
        {
            var existing = service.VehiclePrices.FirstOrDefault(p => p.VehicleType == requested.VehicleType);
            if (existing is null)
            {
                service.VehiclePrices.Add(new ServiceVehiclePrice
                {
                    ServiceId = service.Id,
                    VehicleType = requested.VehicleType,
                    Price = requested.Price
                });
                continue;
            }

            existing.Price = requested.Price;
        }
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

    private static void SyncMaterials(Service service, IReadOnlyCollection<ServiceMaterialInput> materials)
    {
        service.Materials.Clear();
        foreach (var material in materials.GroupBy(m => m.ProductId))
        {
            service.Materials.Add(new ServiceMaterial
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                ProductId = material.Key,
                Quantity = material.Sum(m => m.Quantity)
            });
        }
    }

    private static void SyncSteps(Service service, IReadOnlyCollection<string> steps)
    {
        service.Steps.Clear();
        foreach (var (step, index) in steps
                     .Where(s => !string.IsNullOrWhiteSpace(s))
                     .Select((step, index) => (step.Trim(), index)))
        {
            service.Steps.Add(new ServiceStep
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                Position = index + 1,
                Description = step
            });
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeTier(string? tier) =>
        string.IsNullOrWhiteSpace(tier) ? "standard" : tier.Trim();
}
