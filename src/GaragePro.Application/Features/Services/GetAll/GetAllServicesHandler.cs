using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Services.GetAll;

public class GetAllServicesHandler(IServiceRepository serviceRepository) : IRequestHandler<GetAllServicesQuery, Result<PaginatedResult<ServiceResponse>>>
{
    public async Task<Result<PaginatedResult<ServiceResponse>>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
    {
        var (services, total) = await serviceRepository.GetAllAsync(request.PageNumber, request.PageSize);

        var items = services.Select(s => new ServiceResponse(s.Id, s.Name, s.Description, s.Price, s.CreatedAt, s.UpdatedAt));

        return Result<PaginatedResult<ServiceResponse>>.Success(
            new PaginatedResult<ServiceResponse>(items, total, request.PageNumber, request.PageSize));
    }
}
