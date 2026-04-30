using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Products.GetAll;

public class GetAllProductsHandler(IProductRepository productRepository) : IRequestHandler<GetAllProductsQuery, Result<PaginatedResult<ProductResponse>>>
{
    public async Task<Result<PaginatedResult<ProductResponse>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, total) = await productRepository.GetAllAsync(request.PageNumber, request.PageSize);

        var items = products.Select(p => new ProductResponse(p.Id, p.Name, p.Description, p.Price, p.CreatedAt, p.UpdatedAt));

        return Result<PaginatedResult<ProductResponse>>.Success(
            new PaginatedResult<ProductResponse>(items, total, request.PageNumber, request.PageSize));
    }
}
