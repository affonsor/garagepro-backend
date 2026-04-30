using FluentAssertions;
using GaragePro.Application.Common;
using GaragePro.Application.Features.Appointments.Cancel;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Appointments;

public class CancelAppointmentHandlerTests
{
    private readonly Mock<IAppointmentRepository> _repoMock = new();
    private readonly CancelAppointmentHandler _handler;

    public CancelAppointmentHandlerTests()
    {
        _handler = new CancelAppointmentHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCancelAppointment_WhenStatusIsScheduled()
    {
        var appointment = BuildAppointment(AppointmentStatus.Scheduled);
        _repoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var result = await _handler.Handle(new CancelAppointmentCommand(appointment.Id), default);

        result.IsSuccess.Should().BeTrue();
        appointment.Status.Should().Be(AppointmentStatus.Canceled);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStatusIsAlreadyCompleted()
    {
        var appointment = BuildAppointment(AppointmentStatus.Completed);
        _repoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var result = await _handler.Handle(new CancelAppointmentCommand(appointment.Id), default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStatusIsCanceled()
    {
        var appointment = BuildAppointment(AppointmentStatus.Canceled);
        _repoMock.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var result = await _handler.Handle(new CancelAppointmentCommand(appointment.Id), default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAppointmentNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var result = await _handler.Handle(new CancelAppointmentCommand(Guid.NewGuid()), default);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    private static Appointment BuildAppointment(AppointmentStatus status) => new()
    {
        Id = Guid.NewGuid(),
        Status = status,
        Client = new Client { Name = "C" },
        Product = new Product { Name = "P" },
        Service = new Service { Name = "S" },
        RescheduleHistory = new List<AppointmentRescheduleHistory>(),
    };
}
