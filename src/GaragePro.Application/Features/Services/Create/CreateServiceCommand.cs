using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Services.Create;

public record CreateServiceCommand(string Name, string? Description, decimal Price) : IRequest<Result<Guid>>;
