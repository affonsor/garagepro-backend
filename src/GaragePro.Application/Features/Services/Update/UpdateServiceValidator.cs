using FluentValidation;
using GaragePro.Application.Features.Services;
using GaragePro.Application.Features.Services.Create;

namespace GaragePro.Application.Features.Services.Update;

public class UpdateServiceValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.Code));
        RuleFor(x => x.Tier).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Category).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Category));
        RuleFor(x => x.Duration).MaximumLength(40).When(x => !string.IsNullOrWhiteSpace(x.Duration));
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VehiclePrices)
            .Must((command, vehiclePrices) => (vehiclePrices is { Count: > 0 }) || command.Price > 0)
            .WithMessage("Either a service price or at least one vehicle price is required");
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VehiclePrices)
            .Must(HaveUniqueVehicleTypes)
            .WithMessage("Vehicle prices cannot contain duplicate vehicle types")
            .When(x => x.VehiclePrices is not null);
        RuleForEach(x => x.VehiclePrices).SetValidator(new ServiceVehiclePriceDtoValidator()).When(x => x.VehiclePrices is not null);
        RuleForEach(x => x.Materials).SetValidator(new ServiceMaterialInputValidator()).When(x => x.Materials is not null);
        RuleForEach(x => x.Steps).NotEmpty().MaximumLength(500).When(x => x.Steps is not null);
    }

    private static bool HaveUniqueVehicleTypes(IEnumerable<ServiceVehiclePriceDto>? vehiclePrices) =>
        vehiclePrices is null ||
        vehiclePrices.Select(p => p.VehicleType).Distinct().Count() == vehiclePrices.Count();
}
