using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using MediatR;

namespace GaragePro.Application.Features.Addresses.Update;

public record UpdateAddressCommand(
    Guid ClientId,
    Guid AddressId,
    AddressType Type,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string ZipCode) : IRequest<Result<Guid>>;
