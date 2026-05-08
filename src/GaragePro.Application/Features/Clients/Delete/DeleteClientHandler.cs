using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Clients.Delete;

public class DeleteClientHandler(IClientRepository clientRepository) : IRequestHandler<DeleteClientCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.Id);
        if (client is null)
            return Result<bool>.NotFound("Client not found");

        if (!client.IsActive)
            return Result<bool>.Success(true);

        client.IsActive = false;
        client.UpdatedAt = DateTimeOffset.UtcNow;

        await clientRepository.UpdateAsync(client);
        return Result<bool>.Success(true);
    }
}
