using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Clients.GetById;

public class GetClientByIdHandler(IClientRepository clientRepository) : IRequestHandler<GetClientByIdQuery, Result<ClientDetailResponse>>
{
    public async Task<Result<ClientDetailResponse>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.Id);
        if (client is null)
            return Result<ClientDetailResponse>.NotFound("Client not found");

        var response = new ClientDetailResponse(
            client.Id, client.Name, client.Email, client.Phone, client.Document,
            client.Addresses.Select(a => new AddressResponse(
                a.Id, a.Type.ToString(), a.Street, a.Number, a.Complement,
                a.District, a.City, a.State, a.ZipCode)),
            client.Vehicles.Select(v => new VehicleInClientResponse(
                v.Id, v.LicensePlate, v.Make, v.Model, v.Year, v.Color)),
            client.CreatedAt, client.UpdatedAt);

        return Result<ClientDetailResponse>.Success(response);
    }
}
