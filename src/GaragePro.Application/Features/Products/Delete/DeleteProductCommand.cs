using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Products.Delete;

public record DeleteProductCommand(Guid Id) : IRequest<Result<bool>>;
