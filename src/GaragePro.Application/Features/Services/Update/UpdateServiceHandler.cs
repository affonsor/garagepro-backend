using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Services.Update;

public class UpdateServiceHandler(IServiceRepository serviceRepository) : IRequestHandler<UpdateServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(request.Id);
        if (service is null)
            return Result<Guid>.NotFound("Service not found");

        service.Name = request.Name;
        service.Description = request.Description;
        service.Price = request.Price;
        service.UpdatedAt = DateTimeOffset.UtcNow;

        await serviceRepository.UpdateAsync(service);
        return Result<Guid>.Success(service.Id);
    }
}
