using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using MediatR;

namespace GaragePro.Application.Features.Clients.Create;

public record CreateClientCommand(
    string Name,
    string? Email,
    string? Phone,
    string? Document,
    List<CreateAddressDto> Addresses) : IRequest<Result<Guid>>;

public record CreateAddressDto(
    AddressType Type,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string ZipCode);
