using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Core.Interfaces.Services;
using MediatR;

namespace GaragePro.Application.Features.Auth.Login;

public class LoginHandler(
    IUserRepository userRepository,
    IAuthService authService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null || !authService.VerifyPassword(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid credentials");

        var (token, expiresAt) = authService.GenerateToken(user);

        var response = new LoginResponse(
            token,
            expiresAt,
            new LoginUserInfo(user.Id, user.Name, user.Email, user.Roles.Select(r => r.ToString())));

        return Result<LoginResponse>.Success(response);
    }
}
