using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Users.Update;

public class UpdateUserHandler(IUserRepository userRepository) : IRequestHandler<UpdateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id);
        if (user is null)
            return Result<Guid>.NotFound("User not found");

        if (await userRepository.ExistsByEmailAsync(request.Email, excludeId: request.Id))
            return Result<Guid>.Failure("Email already in use by another user");

        user.Name = request.Name;
        user.Email = request.Email;
        user.Roles = request.Roles;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await userRepository.UpdateAsync(user);
        return Result<Guid>.Success(user.Id);
    }
}
