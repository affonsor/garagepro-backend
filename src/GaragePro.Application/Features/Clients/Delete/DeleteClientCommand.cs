using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Clients.Delete;

public record DeleteClientCommand(Guid Id) : IRequest<Result<bool>>;
