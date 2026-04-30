using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Users.Delete;

public class DeleteUserHandler(IUserRepository userRepository) : IRequestHandler<DeleteUserCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id);
        if (user is null)
            return Result<bool>.NotFound("User not found");

        await userRepository.DeleteAsync(request.Id);
        return Result<bool>.Success(true);
    }
}
