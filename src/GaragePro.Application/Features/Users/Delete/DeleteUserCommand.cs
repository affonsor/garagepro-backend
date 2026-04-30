using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Users.Delete;

public record DeleteUserCommand(Guid Id) : IRequest<Result<bool>>;
