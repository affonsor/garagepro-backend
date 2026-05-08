using FluentValidation;
using GaragePro.Core.Enums;
using GaragePro.Core.ValueObjects;

namespace GaragePro.Application.Features.Clients.Create;

public class CreateClientValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(256).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
        RuleFor(x => x.Tier).NotEmpty().MaximumLength(40);
        RuleFor(x => x.AddressText).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.AddressText));
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Notes));
        RuleFor(x => x.Document)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(18)
            .Must(value => Document.TryCreate(value, out _))
            .WithMessage("Document must be a valid CPF or CNPJ");
        RuleFor(x => x.Addresses).NotEmpty().WithMessage("At least one address is required");
        RuleForEach(x => x.Addresses).SetValidator(new CreateAddressDtoValidator());
        RuleFor(x => x.Vehicles).NotEmpty().WithMessage("At least one vehicle is required");
        RuleForEach(x => x.Vehicles).SetValidator(new CreateClientVehicleDtoValidator());
    }
}

public class CreateAddressDtoValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressDtoValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Street).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Number).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Complement).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Complement));
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().Length(2).WithMessage("State must be exactly 2 characters (UF code)");
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(10);
    }
}

public class CreateClientVehicleDtoValidator : AbstractValidator<CreateClientVehicleDto>
{
    public CreateClientVehicleDtoValidator()
    {
        RuleFor(x => x.LicensePlate).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Make).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Year).InclusiveBetween(1900, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.Color).MaximumLength(50).When(x => x.Color is not null);
        RuleFor(x => x.VIN).MaximumLength(17).When(x => x.VIN is not null);
    }
}
