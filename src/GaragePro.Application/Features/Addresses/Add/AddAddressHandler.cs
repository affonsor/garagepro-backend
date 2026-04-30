using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Addresses.Add;

public class AddAddressHandler(
    IClientRepository clientRepository,
    IAddressRepository addressRepository) : IRequestHandler<AddAddressCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId);
        if (client is null)
            return Result<Guid>.NotFound("Client not found");

        var address = new Address
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            Type = request.Type,
            Street = request.Street,
            Number = request.Number,
            Complement = request.Complement,
            District = request.District,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await addressRepository.AddAsync(address);
        return Result<Guid>.Success(address.Id);
    }
}
