using FluentAssertions;
using GaragePro.Application.Common;
using GaragePro.Application.Features.Appointments.Create;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Appointments;

public class CreateAppointmentHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock = new();
    private readonly Mock<IClientRepository> _clientRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IServiceRepository> _serviceRepoMock = new();
    private readonly CreateAppointmentHandler _handler;

    public CreateAppointmentHandlerTests()
    {
        _handler = new CreateAppointmentHandler(
            _appointmentRepoMock.Object,
            _clientRepoMock.Object,
            _productRepoMock.Object,
            _serviceRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateAppointment_WhenAllDataIsValid()
    {
        SetupValidEntities();
        _appointmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment a, CancellationToken _) => a.Id);

        var result = await _handler.Handle(BuildCommand(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldSnapshotProductAndServicePrices_WhenCreated()
    {
        SetupValidEntities(productPrice: 150m, servicePrice: 75m);
        Appointment? captured = null;
        _appointmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((a, _) => captured = a)
            .ReturnsAsync((Appointment a, CancellationToken _) => a.Id);

        await _handler.Handle(BuildCommand(), default);

        captured!.ProductValueSnapshot.Should().Be(150m);
        captured.ServiceValueSnapshot.Should().Be(75m);
        captured.TotalValue.Should().Be(225m);
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToScheduled_WhenCreated()
    {
        SetupValidEntities();
        Appointment? captured = null;
        _appointmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((a, _) => captured = a)
            .ReturnsAsync((Appointment a, CancellationToken _) => a.Id);

        await _handler.Handle(BuildCommand(), default);

        captured!.Status.Should().Be(AppointmentStatus.Scheduled);
        captured.IsRescheduled.Should().BeFalse();
        captured.RescheduleCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenClientDoesNotExist()
    {
        _clientRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Client?)null);

        var result = await _handler.Handle(BuildCommand(), default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductIsInactive()
    {
        _clientRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Client { Name = "C" });
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Product { IsActive = false, Price = 10m });

        var result = await _handler.Handle(BuildCommand(), default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenServiceIsInactive()
    {
        _clientRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Client { Name = "C" });
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Product { IsActive = true, Price = 10m });
        _serviceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Service { IsActive = false, Price = 10m });

        var result = await _handler.Handle(BuildCommand(), default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    private void SetupValidEntities(decimal productPrice = 100m, decimal servicePrice = 50m)
    {
        _clientRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Client { Name = "C" });
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Product { IsActive = true, Price = productPrice });
        _serviceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Service { IsActive = true, Price = servicePrice });
    }

    private static CreateAppointmentCommand BuildCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2),
        null);
}
