using FluentAssertions;
using GaragePro.Application.Common;
using GaragePro.Application.Features.Vehicles.Create;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Vehicles;

public class CreateVehicleHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly Mock<IVehicleRepository> _vehicleRepository = new();
    private readonly CreateVehicleHandler _handler;

    public CreateVehicleHandlerTests()
    {
        _handler = new CreateVehicleHandler(_clientRepository.Object, _vehicleRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenClientIsInactive()
    {
        var clientId = Guid.NewGuid();
        _clientRepository.Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(new Client { Id = clientId, Name = "Maria", Document = "52998224725", IsActive = false });

        var result = await _handler.Handle(BuildCommand(clientId), default);

        result.Status.Should().Be(ResultStatus.Failure);
        _vehicleRepository.Verify(r => r.CreateAsync(It.IsAny<Vehicle>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenLicensePlateAlreadyExists()
    {
        var clientId = Guid.NewGuid();
        _clientRepository.Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(new Client { Id = clientId, Name = "Maria", Document = "52998224725", IsActive = true });
        _vehicleRepository.Setup(r => r.ExistsByLicensePlateAsync("ABC1D23", null)).ReturnsAsync(true);

        var result = await _handler.Handle(BuildCommand(clientId), default);

        result.Status.Should().Be(ResultStatus.Conflict);
        _vehicleRepository.Verify(r => r.CreateAsync(It.IsAny<Vehicle>()), Times.Never);
    }

    private static CreateVehicleCommand BuildCommand(Guid clientId) => new(
        clientId,
        "abc1d23",
        "Honda",
        "Civic",
        VehicleType.Car,
        2020,
        "Prata",
        null);
}
