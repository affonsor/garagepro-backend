using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Users.GetById;

public class GetUserByIdHandler(IUserRepository userRepository) : IRequestHandler<GetUserByIdQuery, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id);
        if (user is null)
            return Result<UserResponse>.NotFound("User not found");

        var response = new UserResponse(
            user.Id, user.Name, user.Email,
            user.Roles.Select(r => r.ToString()),
            user.CreatedAt);

        return Result<UserResponse>.Success(response);
    }
}
