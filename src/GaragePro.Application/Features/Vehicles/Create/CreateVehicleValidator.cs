using FluentValidation;

namespace GaragePro.Application.Features.Vehicles.Create;

public class CreateVehicleValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Make).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Year).InclusiveBetween(1900, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Color).MaximumLength(50).When(x => x.Color is not null);
        RuleFor(x => x.VIN).MaximumLength(17).When(x => x.VIN is not null);
    }
}
