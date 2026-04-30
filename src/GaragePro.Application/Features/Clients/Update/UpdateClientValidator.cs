using FluentValidation;

namespace GaragePro.Application.Features.Clients.Update;

public class UpdateClientValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(256).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrEmpty(x.Phone));
        RuleFor(x => x.Document).MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Document));
    }
}
