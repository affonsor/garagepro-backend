using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Products.GetAll;

public record GetAllProductsQuery(int PageNumber, int PageSize) : IRequest<Result<PaginatedResult<ProductResponse>>>;
