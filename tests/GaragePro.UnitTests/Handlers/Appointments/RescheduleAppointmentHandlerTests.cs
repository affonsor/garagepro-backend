using FluentAssertions;
using GaragePro.Application.Common;
using GaragePro.Application.Features.Appointments.Reschedule;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Appointments;

public class RescheduleAppointmentHandlerTests
{
    private readonly Mock<IAppointmentRepository> _repoMock = new();
    private readonly RescheduleAppointmentHandler _handler;

    public RescheduleAppointmentHandlerTests()
    {
        _handler = new RescheduleAppointmentHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldRescheduleAppointment_WhenStatusIsScheduled()
    {
        var appointment = BuildAppointment();
        _repoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var newStart = DateTimeOffset.UtcNow.AddDays(1);
        var result = await _handler.Handle(BuildCommand(appointment.Id, newStart), default);

        result.IsSuccess.Should().BeTrue();
        appointment.StartAt.Should().Be(newStart);
    }

    [Fact]
    public async Task Handle_ShouldSetIsRescheduledTrue_WhenRescheduled()
    {
        var appointment = BuildAppointment();
        _repoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await _handler.Handle(BuildCommand(appointment.Id), default);

        appointment.IsRescheduled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldIncrementRescheduleCount_WhenRescheduledMultipleTimes()
    {
        var appointment = BuildAppointment();
        appointment.RescheduleCount = 2;
        _repoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await _handler.Handle(BuildCommand(appointment.Id), default);

        appointment.RescheduleCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldCreateHistoryRecord_WhenRescheduled()
    {
        var appointment = BuildAppointment();
        _repoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await _handler.Handle(BuildCommand(appointment.Id), default);

        appointment.RescheduleHistory.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStatusIsNotScheduled()
    {
        var appointment = BuildAppointment(AppointmentStatus.Completed);
        _repoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var result = await _handler.Handle(BuildCommand(appointment.Id), default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAppointmentNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var result = await _handler.Handle(BuildCommand(Guid.NewGuid()), default);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    private static Appointment BuildAppointment(AppointmentStatus status = AppointmentStatus.Scheduled) => new()
    {
        Id = Guid.NewGuid(),
        Status = status,
        StartAt = DateTimeOffset.UtcNow,
        ExpectedEndAt = DateTimeOffset.UtcNow.AddHours(1),
        Client = new Client { Name = "C" },
        Product = new Product { Name = "P" },
        Service = new Service { Name = "S" },
        RescheduleHistory = new List<AppointmentRescheduleHistory>(),
    };

    private static RescheduleAppointmentCommand BuildCommand(Guid id, DateTimeOffset? newStart = null)
    {
        var start = newStart ?? DateTimeOffset.UtcNow.AddDays(1);
        return new RescheduleAppointmentCommand(id, start, start.AddHours(2), null);
    }
}
