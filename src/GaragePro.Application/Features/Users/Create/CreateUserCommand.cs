using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using MediatR;

namespace GaragePro.Application.Features.Users.Create;

public record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    List<UserRole> Roles) : IRequest<Result<Guid>>;
