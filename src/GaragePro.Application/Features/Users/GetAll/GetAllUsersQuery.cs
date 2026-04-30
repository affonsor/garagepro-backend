using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Users.GetAll;

public record GetAllUsersQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedResult<UserResponse>>>;
