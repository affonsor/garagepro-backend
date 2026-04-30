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

        if (await clientRepository.HasVehiclesByClientIdAsync(request.Id))
            return Result<bool>.Failure("Client has linked vehicles and cannot be deleted");

        await clientRepository.DeleteAsync(request.Id);
        return Result<bool>.Success(true);
    }
}
