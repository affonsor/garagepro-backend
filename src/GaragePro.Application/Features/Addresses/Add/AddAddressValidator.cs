using FluentValidation;

namespace GaragePro.Application.Features.Addresses.Add;

public class AddAddressValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
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
