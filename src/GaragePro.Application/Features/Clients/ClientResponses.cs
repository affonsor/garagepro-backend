namespace GaragePro.Application.Features.Clients;

public record ClientSummaryResponse(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    int VehicleCount,
    DateTimeOffset CreatedAt);

public record ClientDetailResponse(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string? Document,
    IEnumerable<AddressResponse> Addresses,
    IEnumerable<VehicleInClientResponse> Vehicles,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record AddressResponse(
    Guid Id,
    string Type,
    string Street,
    string Number,
    string? Complement,
    string District,
    string City,
    string State,
    string ZipCode);

public record VehicleInClientResponse(
    Guid Id,
    string LicensePlate,
    string Make,
    string Model,
    int Year,
    string? Color);
