using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Services.GetAll;

public class GetAllServicesHandler(IServiceRepository serviceRepository) : IRequestHandler<GetAllServicesQuery, Result<PaginatedResult<ServiceResponse>>>
{
    public async Task<Result<PaginatedResult<ServiceResponse>>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
    {
        var (services, total) = await serviceRepository.GetAllAsync(
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.Category,
            request.Tier,
            request.Active);

        var items = services.Select(ServiceResponse.From);

        return Result<PaginatedResult<ServiceResponse>>.Success(
            new PaginatedResult<ServiceResponse>(items, request.PageNumber, request.PageSize, total));
    }
}
