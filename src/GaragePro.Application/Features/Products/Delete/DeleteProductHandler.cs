using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Products.Delete;

public class DeleteProductHandler(IProductRepository productRepository) : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id);
        if (product is null)
            return Result<bool>.NotFound("Product not found");

        await productRepository.DeleteAsync(request.Id);
        return Result<bool>.Success(true);
    }
}
