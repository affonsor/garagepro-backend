using FluentValidation;
using GaragePro.Core.ValueObjects;

namespace GaragePro.Application.Features.Clients.Update;

public class UpdateClientValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientValidator()
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
    }
}
