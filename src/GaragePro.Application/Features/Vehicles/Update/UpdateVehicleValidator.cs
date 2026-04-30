using FluentValidation;

namespace GaragePro.Application.Features.Vehicles.Update;

public class UpdateVehicleValidator : AbstractValidator<UpdateVehicleCommand>
{
    public UpdateVehicleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Make).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Year).InclusiveBetween(1900, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Color).MaximumLength(50).When(x => x.Color is not null);
        RuleFor(x => x.VIN).MaximumLength(17).When(x => x.VIN is not null);
    }
}
