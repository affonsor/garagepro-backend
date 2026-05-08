using GaragePro.Application.Common;
using GaragePro.Core.Enums;

namespace GaragePro.Application.Features.Appointments;

public record AppointmentSummaryResponse(
    Guid Id,
    string ClientName,
    Guid VehicleId,
    string VehicleLicensePlate,
    string VehicleModel,
    VehicleType VehicleType,
    string ProductName,
    string ServiceName,
    DateTimeOffset StartAt,
    DateTimeOffset ExpectedEndAt,
    AppointmentStatus Status,
    bool IsRescheduled,
    decimal TotalValue,
    string? Notes);

public record AppointmentSummaryDto(
    int ScheduledCount, decimal ScheduledTotal,
    int CompletedCount, decimal CompletedTotal,
    int CanceledCount, decimal CanceledTotal);

public record AppointmentListResponse(
    PaginatedResult<AppointmentSummaryResponse> Data,
    AppointmentSummaryDto Summary);

public record AppointmentRescheduleHistoryResponse(
    Guid Id,
    DateTimeOffset PreviousStartAt,
    DateTimeOffset PreviousExpectedEndAt,
    DateTimeOffset NewStartAt,
    DateTimeOffset NewExpectedEndAt,
    string? Reason,
    string ChangedByUserName,
    DateTimeOffset ChangedAt);

public record AppointmentDetailResponse(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid VehicleId,
    string VehicleLicensePlate,
    string VehicleModel,
    VehicleType VehicleType,
    Guid ProductId,
    string ProductName,
    Guid ServiceId,
    string ServiceName,
    DateTimeOffset StartAt,
    DateTimeOffset ExpectedEndAt,
    AppointmentStatus Status,
    bool IsRescheduled,
    int RescheduleCount,
    decimal ProductValueSnapshot,
    decimal ServiceValueSnapshot,
    decimal TotalValue,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IEnumerable<AppointmentRescheduleHistoryResponse> RescheduleHistory);
