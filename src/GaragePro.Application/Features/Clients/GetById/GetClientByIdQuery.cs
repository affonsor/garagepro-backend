using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Clients.GetById;

public record GetClientByIdQuery(Guid Id) : IRequest<Result<ClientDetailResponse>>;
