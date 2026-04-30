using FluentValidation;
using GaragePro.Core.Enums;

namespace GaragePro.Application.Features.Users.Update;

public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Roles).NotEmpty()
            .Must(r => r.All(role => Enum.IsDefined(typeof(UserRole), role)))
            .WithMessage("Invalid role value");
    }
}
