using FluentValidation;

namespace GaragePro.Application.Features.Products.Update;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Sku).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.Sku));
        RuleFor(x => x.Brand).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Brand));
        RuleFor(x => x.Category).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Category));
        RuleFor(x => x.Size).MaximumLength(40).When(x => !string.IsNullOrWhiteSpace(x.Size));
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Supplier).MaximumLength(160).When(x => !string.IsNullOrWhiteSpace(x.Supplier));
        RuleFor(x => x.Barcode).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.Barcode));
    }
}
