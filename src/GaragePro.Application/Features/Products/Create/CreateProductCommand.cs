using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Products.Create;

public record CreateProductCommand(string Name, string? Description, decimal Price) : IRequest<Result<Guid>>;
