using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Users.GetById;

public record GetUserByIdQuery(Guid Id) : IRequest<Result<UserResponse>>;
