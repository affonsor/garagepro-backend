using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Services.GetById;

public class GetServiceByIdHandler(IServiceRepository serviceRepository) : IRequestHandler<GetServiceByIdQuery, Result<ServiceResponse>>
{
    public async Task<Result<ServiceResponse>> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(request.Id);
        if (service is null)
            return Result<ServiceResponse>.NotFound("Service not found");

        return Result<ServiceResponse>.Success(ServiceResponse.From(service));
    }
}
