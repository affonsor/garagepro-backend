using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Services.Update;

public record UpdateServiceCommand(Guid Id, string Name, string? Description, decimal Price) : IRequest<Result<Guid>>;
