using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Clients.Update;

public record UpdateClientCommand(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string Document,
    string Tier = "standard",
    DateOnly? Birthday = null,
    string? AddressText = null,
    string? Notes = null) : IRequest<Result<Guid>>;
