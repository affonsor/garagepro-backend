using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Clients.GetAll;

public record GetAllClientsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedResult<ClientSummaryResponse>>>;
