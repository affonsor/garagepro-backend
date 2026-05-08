using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using MediatR;

namespace GaragePro.Application.Features.Clients.Create;

public record CreateClientCommand(
    string Name,
    string? Email,
    string? Phone,
    string Document,
    List<CreateAddressDto> Addresses,
    List<CreateClientVehicleDto> Vehicles,
    string Tier = "standard",
    DateOnly? Birthday = null,
    string? AddressText = null,
    string? Notes = null) : IRequest<Result<Guid>>;

public record CreateAddressDto(
    AddressType Type,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string ZipCode);

public record CreateClientVehicleDto(
    string LicensePlate,
    string Make,
    string Model,
    VehicleType Type,
    int Year,
    string? Color,
    string? VIN);
