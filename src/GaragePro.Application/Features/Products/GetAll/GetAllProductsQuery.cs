using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Products.GetAll;

public record GetAllProductsQuery(
    int PageNumber,
    int PageSize,
    string? Search = null,
    string? Status = null,
    string? Category = null) : IRequest<Result<PaginatedResult<ProductResponse>>>;
