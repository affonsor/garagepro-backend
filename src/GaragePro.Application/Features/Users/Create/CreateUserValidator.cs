using FluentValidation;
using GaragePro.Core.Enums;

namespace GaragePro.Application.Features.Users.Create;

public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Roles).NotEmpty()
            .Must(r => r.All(role => Enum.IsDefined(typeof(UserRole), role)))
            .WithMessage("Invalid role value");
    }
}
