using FluentValidation;

namespace GaragePro.Application.Features.Services.Update;

public class UpdateServiceValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
