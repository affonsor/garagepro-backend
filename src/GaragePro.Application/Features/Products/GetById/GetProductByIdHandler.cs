using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Products.GetById;

public class GetProductByIdHandler(IProductRepository productRepository) : IRequestHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id);
        if (product is null)
            return Result<ProductResponse>.NotFound("Product not found");

        return Result<ProductResponse>.Success(
            new ProductResponse(product.Id, product.Name, product.Description, product.Price, product.CreatedAt, product.UpdatedAt));
    }
}
