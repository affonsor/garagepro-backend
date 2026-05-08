using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Services.GetAll;

public record GetAllServicesQuery(
    int PageNumber,
    int PageSize,
    string? Search = null,
    string? Category = null,
    string? Tier = null,
    bool? Active = null) : IRequest<Result<PaginatedResult<ServiceResponse>>>;
