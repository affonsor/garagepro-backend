using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Core.ValueObjects;
using MediatR;

namespace GaragePro.Application.Features.Clients.Update;

public class UpdateClientHandler(IClientRepository clientRepository) : IRequestHandler<UpdateClientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.Id);
        if (client is null)
            return Result<Guid>.NotFound("Client not found");

        if (!Document.TryCreate(request.Document, out var document))
            return Result<Guid>.ValidationFailure(["Document must be a valid CPF or CNPJ"]);

        if (await clientRepository.ExistsByDocumentAsync(document!.Value, request.Id))
            return Result<Guid>.Conflict("Document already registered");

        client.Name = request.Name;
        client.Email = request.Email;
        client.Phone = request.Phone;
        client.Document = document.Value;
        client.Tier = NormalizeTier(request.Tier);
        client.Birthday = request.Birthday;
        client.AddressText = request.AddressText;
        client.Notes = request.Notes;
        client.UpdatedAt = DateTimeOffset.UtcNow;

        await clientRepository.UpdateAsync(client);
        return Result<Guid>.Success(client.Id);
    }

    private static string NormalizeTier(string? tier) =>
        string.IsNullOrWhiteSpace(tier) ? "standard" : tier.Trim();
}
