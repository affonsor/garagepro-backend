using FluentAssertions;
using GaragePro.Application.Common;
using GaragePro.Application.Features.Vehicles.Delete;
using GaragePro.Application.Features.Vehicles.Transfer;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Vehicles;

public class VehicleInvariantHandlerTests
{
    private readonly Mock<IVehicleRepository> _vehicleRepository = new();
    private readonly Mock<IClientRepository> _clientRepository = new();

    [Fact]
    public async Task Delete_ShouldFail_WhenItWouldRemoveClientLastVehicle()
    {
        var vehicle = BuildVehicle();
        _vehicleRepository.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);
        _vehicleRepository.Setup(r => r.HasTransferHistoryAsync(vehicle.Id)).ReturnsAsync(false);
        _clientRepository.Setup(r => r.CountVehiclesByClientIdAsync(vehicle.ClientId)).ReturnsAsync(1);
        var handler = new DeleteVehicleHandler(_vehicleRepository.Object, _clientRepository.Object);

        var result = await handler.Handle(new DeleteVehicleCommand(vehicle.Id), default);

        result.Status.Should().Be(ResultStatus.Failure);
        _vehicleRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Delete_ShouldSucceed_WhenClientHasMoreThanOneVehicle()
    {
        var vehicle = BuildVehicle();
        _vehicleRepository.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);
        _vehicleRepository.Setup(r => r.HasTransferHistoryAsync(vehicle.Id)).ReturnsAsync(false);
        _clientRepository.Setup(r => r.CountVehiclesByClientIdAsync(vehicle.ClientId)).ReturnsAsync(2);
        _vehicleRepository.Setup(r => r.DeleteAsync(vehicle.Id)).Returns(Task.CompletedTask);
        var handler = new DeleteVehicleHandler(_vehicleRepository.Object, _clientRepository.Object);

        var result = await handler.Handle(new DeleteVehicleCommand(vehicle.Id), default);

        result.IsSuccess.Should().BeTrue();
        _vehicleRepository.Verify(r => r.DeleteAsync(vehicle.Id), Times.Once);
    }

    [Fact]
    public async Task Transfer_ShouldFail_WhenItWouldLeaveSourceClientWithoutVehicles()
    {
        var vehicle = BuildVehicle();
        _vehicleRepository.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);
        _clientRepository.Setup(r => r.CountVehiclesByClientIdAsync(vehicle.ClientId)).ReturnsAsync(1);
        var handler = new TransferVehicleHandler(_vehicleRepository.Object, _clientRepository.Object);

        var result = await handler.Handle(new TransferVehicleCommand(vehicle.Id, Guid.NewGuid(), null), default);

        result.Status.Should().Be(ResultStatus.Failure);
        _vehicleRepository.Verify(r => r.TransferAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Transfer_ShouldSucceed_WhenSourceClientHasMoreThanOneVehicle()
    {
        var vehicle = BuildVehicle();
        var targetClient = new Client { Id = Guid.NewGuid(), Name = "Joao", Document = "04252011000110", IsActive = true };
        _vehicleRepository.Setup(r => r.GetByIdAsync(vehicle.Id)).ReturnsAsync(vehicle);
        _clientRepository.Setup(r => r.CountVehiclesByClientIdAsync(vehicle.ClientId)).ReturnsAsync(2);
        _clientRepository.Setup(r => r.GetByIdAsync(targetClient.Id)).ReturnsAsync(targetClient);
        _vehicleRepository.Setup(r => r.TransferAsync(vehicle.Id, targetClient.Id, null))
            .ReturnsAsync((Guid.NewGuid(), DateTimeOffset.UtcNow));
        var handler = new TransferVehicleHandler(_vehicleRepository.Object, _clientRepository.Object);

        var result = await handler.Handle(new TransferVehicleCommand(vehicle.Id, targetClient.Id, null), default);

        result.IsSuccess.Should().BeTrue();
    }

    private static Vehicle BuildVehicle()
    {
        var clientId = Guid.NewGuid();
        return new Vehicle
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            LicensePlate = "ABC1D23",
            Client = new Client { Id = clientId, Name = "Maria", Document = "52998224725", IsActive = true }
        };
    }
}
