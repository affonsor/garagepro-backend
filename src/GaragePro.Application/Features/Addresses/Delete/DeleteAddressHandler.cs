using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Addresses.Delete;

public class DeleteAddressHandler(
    IClientRepository clientRepository,
    IAddressRepository addressRepository) : IRequestHandler<DeleteAddressCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId);
        if (client is null)
            return Result<bool>.NotFound("Client not found");

        var address = await addressRepository.GetByIdAsync(request.AddressId);
        if (address is null || address.ClientId != request.ClientId)
            return Result<bool>.NotFound("Address not found");

        var count = await addressRepository.CountByClientIdAsync(request.ClientId);
        if (count <= 1)
            return Result<bool>.Failure("Client must have at least one address");

        await addressRepository.DeleteAsync(request.AddressId);
        return Result<bool>.Success(true);
    }
}
