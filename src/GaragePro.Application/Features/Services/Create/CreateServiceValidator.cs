using FluentValidation;

namespace GaragePro.Application.Features.Services.Create;

public class CreateServiceValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
