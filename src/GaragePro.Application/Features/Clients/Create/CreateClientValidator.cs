using FluentValidation;
using GaragePro.Core.Enums;

namespace GaragePro.Application.Features.Clients.Create;

public class CreateClientValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(256).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
        RuleFor(x => x.Document).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Document));
        RuleFor(x => x.Addresses).NotEmpty().WithMessage("At least one address is required");
        RuleForEach(x => x.Addresses).SetValidator(new CreateAddressDtoValidator());
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
