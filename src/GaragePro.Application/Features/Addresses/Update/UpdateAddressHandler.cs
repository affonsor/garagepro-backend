using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Addresses.Update;

public class UpdateAddressHandler(
    IClientRepository clientRepository,
    IAddressRepository addressRepository) : IRequestHandler<UpdateAddressCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId);
        if (client is null)
            return Result<Guid>.NotFound("Client not found");

        var address = await addressRepository.GetByIdAsync(request.AddressId);
        if (address is null || address.ClientId != request.ClientId)
            return Result<Guid>.NotFound("Address not found");

        address.Type = request.Type;
        address.Street = request.Street;
        address.Number = request.Number;
        address.Complement = request.Complement;
        address.District = request.District;
        address.City = request.City;
        address.State = request.State;
        address.ZipCode = request.ZipCode;

        await addressRepository.UpdateAsync(address);
        return Result<Guid>.Success(address.Id);
    }
}
