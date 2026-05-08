using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Clients.Reactivate;

public class ReactivateClientHandler(IClientRepository clientRepository)
    : IRequestHandler<ReactivateClientCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ReactivateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.Id);
        if (client is null)
            return Result<bool>.NotFound("Client not found");

        if (client.IsActive)
            return Result<bool>.Success(true);

        if (client.Addresses.Count == 0)
            return Result<bool>.Failure("Client must have at least one address");

        if (await clientRepository.CountVehiclesByClientIdAsync(request.Id) == 0)
            return Result<bool>.Failure("Client must have at least one vehicle");

        client.IsActive = true;
        client.UpdatedAt = DateTimeOffset.UtcNow;

        await clientRepository.UpdateAsync(client);
        return Result<bool>.Success(true);
    }
}
