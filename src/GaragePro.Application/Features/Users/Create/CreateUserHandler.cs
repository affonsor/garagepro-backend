using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Core.Interfaces.Services;
using MediatR;

namespace GaragePro.Application.Features.Users.Create;

public class CreateUserHandler(
    IUserRepository userRepository,
    IAuthService authService) : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByEmailAsync(request.Email))
            return Result<Guid>.Failure("Email already in use");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = authService.HashPassword(request.Password),
            Roles = request.Roles,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.CreateAsync(user);
        return Result<Guid>.Success(user.Id);
    }
}
