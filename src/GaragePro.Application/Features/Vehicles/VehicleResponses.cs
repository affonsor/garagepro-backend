namespace GaragePro.Application.Features.Vehicles;

public record VehicleSummaryResponse(
    Guid Id,
    string LicensePlate,
    string Make,
    string Model,
    int Year,
    string? Color,
    ClientRefResponse CurrentOwner,
    DateTimeOffset CreatedAt);

public record VehicleDetailResponse(
    Guid Id,
    string LicensePlate,
    string Make,
    string Model,
    int Year,
    string? Color,
    string? VIN,
    ClientRefResponse CurrentOwner,
    IEnumerable<TransferHistoryResponse> TransferHistory,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record ClientRefResponse(Guid Id, string Name);

public record TransferHistoryResponse(
    Guid Id,
    ClientRefResponse FromClient,
    ClientRefResponse ToClient,
    DateTimeOffset TransferredAt,
    string? Notes);

public record TransferResponse(Guid TransferRecordId, DateTimeOffset TransferredAt);
