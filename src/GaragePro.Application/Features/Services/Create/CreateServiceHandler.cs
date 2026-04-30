using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Services.Create;

public class CreateServiceHandler(IServiceRepository serviceRepository) : IRequestHandler<CreateServiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await serviceRepository.CreateAsync(service);
        return Result<Guid>.Success(service.Id);
    }
}
