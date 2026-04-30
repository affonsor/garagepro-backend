using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Products.Update;

public record UpdateProductCommand(Guid Id, string Name, string? Description, decimal Price) : IRequest<Result<Guid>>;
