using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Clients.Update;

public record UpdateClientCommand(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string? Document) : IRequest<Result<Guid>>;
