using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Clients.Reactivate;

public record ReactivateClientCommand(Guid Id) : IRequest<Result<bool>>;
