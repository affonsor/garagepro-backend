using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Clients.GetAll;

public record GetAllClientsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    bool IncludeInactive = false,
    string? Search = null,
    string? Tier = null,
    int? BirthdayMonth = null)
    : IRequest<Result<PaginatedResult<ClientSummaryResponse>>>;
