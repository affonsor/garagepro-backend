using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Users.GetAll;

public class GetAllUsersHandler(IUserRepository userRepository) : IRequestHandler<GetAllUsersQuery, Result<PaginatedResult<UserResponse>>>
{
    public async Task<Result<PaginatedResult<UserResponse>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var (users, total) = await userRepository.GetAllAsync(request.PageNumber, request.PageSize);

        var responses = users.Select(u => new UserResponse(
            u.Id, u.Name, u.Email,
            u.Roles.Select(r => r.ToString()),
            u.CreatedAt));

        var paginated = new PaginatedResult<UserResponse>(responses, request.PageNumber, request.PageSize, total);
        return Result<PaginatedResult<UserResponse>>.Success(paginated);
    }
}
