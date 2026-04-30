using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using MediatR;

namespace GaragePro.Application.Features.Users.Update;

public record UpdateUserCommand(
    Guid Id,
    string Name,
    string Email,
    List<UserRole> Roles) : IRequest<Result<Guid>>;
