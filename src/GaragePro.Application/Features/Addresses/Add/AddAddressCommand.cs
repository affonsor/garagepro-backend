using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using MediatR;

namespace GaragePro.Application.Features.Addresses.Add;

public record AddAddressCommand(
    Guid ClientId,
    AddressType Type,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string ZipCode) : IRequest<Result<Guid>>;
