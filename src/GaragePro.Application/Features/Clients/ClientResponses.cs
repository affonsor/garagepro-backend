using GaragePro.Core.Enums;

namespace GaragePro.Application.Features.Clients;

public record ClientSummaryResponse(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string Document,
    bool IsActive,
    int VehicleCount,
    DateTimeOffset CreatedAt,
    string Tier,
    DateOnly? Birthday,
    string? AddressText,
    string? Notes,
    int OrderCount,
    decimal Ltv);

public record ClientDetailResponse(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string Document,
    bool IsActive,
    string Tier,
    DateOnly? Birthday,
    string? AddressText,
    string? Notes,
    int OrderCount,
    decimal Ltv,
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
    VehicleType Type,
    int Year,
    string? Color,
    int OrderCount,
    decimal Ltv);
