using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Clients.Update;

public class UpdateClientHandler(IClientRepository clientRepository) : IRequestHandler<UpdateClientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.Id);
        if (client is null)
            return Result<Guid>.NotFound("Client not found");

        client.Name = request.Name;
        client.Email = request.Email;
        client.Phone = request.Phone;
        client.Document = request.Document;
        client.UpdatedAt = DateTimeOffset.UtcNow;

        await clientRepository.UpdateAsync(client);
        return Result<Guid>.Success(client.Id);
    }
}
