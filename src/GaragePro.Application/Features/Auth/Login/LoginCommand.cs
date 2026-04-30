using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Auth.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, LoginUserInfo User);

public record LoginUserInfo(Guid Id, string Name, string Email, IEnumerable<string> Roles);
