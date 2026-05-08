using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Products.GetAll;

public class GetAllProductsHandler(IProductRepository productRepository) : IRequestHandler<GetAllProductsQuery, Result<PaginatedResult<ProductResponse>>>
{
    public async Task<Result<PaginatedResult<ProductResponse>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, total) = await productRepository.GetAllAsync(
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.Status,
            request.Category);

        var items = products.Select(ProductResponse.From);

        return Result<PaginatedResult<ProductResponse>>.Success(
            new PaginatedResult<ProductResponse>(items, request.PageNumber, request.PageSize, total));
    }
}
