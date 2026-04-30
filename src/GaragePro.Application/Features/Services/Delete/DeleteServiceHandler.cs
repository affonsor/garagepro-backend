using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Services.Delete;

public class DeleteServiceHandler(IServiceRepository serviceRepository) : IRequestHandler<DeleteServiceCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(request.Id);
        if (service is null)
            return Result<bool>.NotFound("Service not found");

        await serviceRepository.DeleteAsync(request.Id);
        return Result<bool>.Success(true);
    }
}
