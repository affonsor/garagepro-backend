using FluentAssertions;
using GaragePro.Application.Features.Appointments;
using GaragePro.Application.Features.Appointments.GetAll;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Appointments;

public class GetAppointmentsHandlerTests
{
    private readonly Mock<IAppointmentRepository> _repoMock = new();
    private readonly GetAppointmentsHandler _handler;

    public GetAppointmentsHandlerTests()
    {
        _handler = new GetAppointmentsHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedList_WhenAppointmentsExist()
    {
        var appointments = new List<Appointment>
        {
            BuildAppointment(AppointmentStatus.Scheduled, 100m),
            BuildAppointment(AppointmentStatus.Completed, 200m)
        };
        var summary = new AppointmentSummaryData(1, 100m, 1, 200m, 0, 0m);

        _repoMock.Setup(r => r.GetAllAsync(null, null, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((appointments, 2));
        _repoMock.Setup(r => r.GetSummaryAsync(null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _handler.Handle(new GetAppointmentsQuery(null, null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Data.Data.Should().HaveCount(2);
        result.Value.Data.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoAppointmentsMatchPeriod()
    {
        var summary = new AppointmentSummaryData(0, 0m, 0, 0m, 0, 0m);

        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<Appointment>(), 0));
        _repoMock.Setup(r => r.GetSummaryAsync(It.IsAny<DateOnly?>(), It.IsAny<DateOnly?>(), null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var result = await _handler.Handle(new GetAppointmentsQuery(startDate, null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Data.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectSummary_WhenAppointmentsHaveMultipleStatuses()
    {
        var appointments = new List<Appointment>
        {
            BuildAppointment(AppointmentStatus.Scheduled, 100m),
            BuildAppointment(AppointmentStatus.Completed, 200m),
            BuildAppointment(AppointmentStatus.Canceled, 150m)
        };
        var summary = new AppointmentSummaryData(1, 100m, 1, 200m, 1, 150m);

        _repoMock.Setup(r => r.GetAllAsync(null, null, null, null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((appointments, 3));
        _repoMock.Setup(r => r.GetSummaryAsync(null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var result = await _handler.Handle(new GetAppointmentsQuery(null, null), default);

        result.Value!.Summary.ScheduledCount.Should().Be(1);
        result.Value.Summary.ScheduledTotal.Should().Be(100m);
        result.Value.Summary.CompletedCount.Should().Be(1);
        result.Value.Summary.CompletedTotal.Should().Be(200m);
        result.Value.Summary.CanceledCount.Should().Be(1);
        result.Value.Summary.CanceledTotal.Should().Be(150m);
    }

    private static Appointment BuildAppointment(AppointmentStatus status, decimal total) => new()
    {
        Id = Guid.NewGuid(),
        Status = status,
        TotalValue = total,
        ClientId = Guid.NewGuid(),
        ProductId = Guid.NewGuid(),
        ServiceId = Guid.NewGuid(),
        StartAt = DateTimeOffset.UtcNow,
        ExpectedEndAt = DateTimeOffset.UtcNow.AddHours(1),
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        Client = new Client { Name = "Test Client" },
        Product = new Product { Name = "Test Product" },
        Service = new Service { Name = "Test Service" },
    };
}
