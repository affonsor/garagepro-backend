using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Products.Update;

public class UpdateProductHandler(IProductRepository productRepository) : IRequestHandler<UpdateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id);
        if (product is null)
            return Result<Guid>.NotFound("Product not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await productRepository.UpdateAsync(product);
        return Result<Guid>.Success(product.Id);
    }
}
