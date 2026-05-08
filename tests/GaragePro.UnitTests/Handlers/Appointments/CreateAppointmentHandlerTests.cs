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
    private readonly Mock<IVehicleRepository> _vehicleRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IServiceRepository> _serviceRepoMock = new();
    private readonly CreateAppointmentHandler _handler;

    public CreateAppointmentHandlerTests()
    {
        _handler = new CreateAppointmentHandler(
            _appointmentRepoMock.Object,
            _clientRepoMock.Object,
            _vehicleRepoMock.Object,
            _productRepoMock.Object,
            _serviceRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateAppointment_WhenAllDataIsValid()
    {
        var command = BuildCommand();
        SetupValidEntities(command);
        _appointmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment a, CancellationToken _) => a.Id);

        var result = await _handler.Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldSnapshotProductAndServicePrices_WhenCreated()
    {
        var command = BuildCommand();
        SetupValidEntities(command, productPrice: 150m, servicePrice: 75m);
        Appointment? captured = null;
        _appointmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((a, _) => captured = a)
            .ReturnsAsync((Appointment a, CancellationToken _) => a.Id);

        await _handler.Handle(command, default);

        captured!.ProductValueSnapshot.Should().Be(150m);
        captured.ServiceValueSnapshot.Should().Be(75m);
        captured.TotalValue.Should().Be(225m);
    }

    [Fact]
    public async Task Handle_ShouldSetStatusToScheduled_WhenCreated()
    {
        var command = BuildCommand();
        SetupValidEntities(command);
        Appointment? captured = null;
        _appointmentRepoMock.Setup(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Callback<Appointment, CancellationToken>((a, _) => captured = a)
            .ReturnsAsync((Appointment a, CancellationToken _) => a.Id);

        await _handler.Handle(command, default);

        captured!.Status.Should().Be(AppointmentStatus.Scheduled);
        captured.IsRescheduled.Should().BeFalse();
        captured.RescheduleCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenClientDoesNotExist()
    {
        var command = BuildCommand();
        _clientRepoMock.Setup(r => r.GetByIdAsync(command.ClientId)).ReturnsAsync((Client?)null);

        var result = await _handler.Handle(command, default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenClientIsInactive()
    {
        var command = BuildCommand();
        _clientRepoMock.Setup(r => r.GetByIdAsync(command.ClientId))
            .ReturnsAsync(new Client { Name = "C", IsActive = false });

        var result = await _handler.Handle(command, default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductIsInactive()
    {
        var command = BuildCommand();
        SetupClientAndVehicle(command);
        _productRepoMock.Setup(r => r.GetByIdAsync(command.ProductId))
            .ReturnsAsync(new Product { IsActive = false, Price = 10m });

        var result = await _handler.Handle(command, default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenServiceIsInactive()
    {
        var command = BuildCommand();
        SetupClientAndVehicle(command);
        _productRepoMock.Setup(r => r.GetByIdAsync(command.ProductId))
            .ReturnsAsync(new Product { IsActive = true, Price = 10m });
        _serviceRepoMock.Setup(r => r.GetByIdAsync(command.ServiceId))
            .ReturnsAsync(new Service { IsActive = false, Price = 10m });

        var result = await _handler.Handle(command, default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenVehicleDoesNotBelongToClient()
    {
        var command = BuildCommand();
        _clientRepoMock.Setup(r => r.GetByIdAsync(command.ClientId)).ReturnsAsync(new Client { Name = "C", IsActive = true });
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(command.VehicleId))
            .ReturnsAsync(new Vehicle { Id = command.VehicleId, ClientId = Guid.NewGuid(), Type = VehicleType.Suv });

        var result = await _handler.Handle(command, default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenServiceDoesNotHavePriceForVehicleType()
    {
        var command = BuildCommand();
        SetupClientAndVehicle(command, VehicleType.Pickup);
        _productRepoMock.Setup(r => r.GetByIdAsync(command.ProductId))
            .ReturnsAsync(new Product { IsActive = true, Price = 10m });
        _serviceRepoMock.Setup(r => r.GetByIdAsync(command.ServiceId))
            .ReturnsAsync(new Service
            {
                IsActive = true,
                VehiclePrices = [new ServiceVehiclePrice { VehicleType = VehicleType.Car, Price = 50m }]
            });

        var result = await _handler.Handle(command, default);

        result.Status.Should().Be(ResultStatus.Failure);
    }

    private void SetupValidEntities(CreateAppointmentCommand command, decimal productPrice = 100m, decimal servicePrice = 50m)
    {
        SetupClientAndVehicle(command, VehicleType.Suv);
        _productRepoMock.Setup(r => r.GetByIdAsync(command.ProductId))
            .ReturnsAsync(new Product { IsActive = true, Price = productPrice });
        _serviceRepoMock.Setup(r => r.GetByIdAsync(command.ServiceId))
            .ReturnsAsync(new Service
            {
                IsActive = true,
                Price = servicePrice,
                VehiclePrices = [new ServiceVehiclePrice { VehicleType = VehicleType.Suv, Price = servicePrice }]
            });
    }

    private void SetupClientAndVehicle(CreateAppointmentCommand command, VehicleType vehicleType = VehicleType.Suv)
    {
        _clientRepoMock.Setup(r => r.GetByIdAsync(command.ClientId)).ReturnsAsync(new Client { Id = command.ClientId, Name = "C", IsActive = true });
        _vehicleRepoMock.Setup(r => r.GetByIdAsync(command.VehicleId))
            .ReturnsAsync(new Vehicle { Id = command.VehicleId, ClientId = command.ClientId, Type = vehicleType });
    }

    private static CreateAppointmentCommand BuildCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
        DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2),
        null);
}
