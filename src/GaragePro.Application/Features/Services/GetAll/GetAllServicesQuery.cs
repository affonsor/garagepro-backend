using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Services.GetAll;

public record GetAllServicesQuery(int PageNumber, int PageSize) : IRequest<Result<PaginatedResult<ServiceResponse>>>;
