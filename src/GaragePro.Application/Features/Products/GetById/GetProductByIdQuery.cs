using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Products.GetById;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductResponse>>;
