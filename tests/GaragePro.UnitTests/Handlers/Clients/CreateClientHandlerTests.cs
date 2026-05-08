using FluentAssertions;
using GaragePro.Application.Common;
using GaragePro.Application.Features.Clients.Create;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Clients;

public class CreateClientHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly Mock<IVehicleRepository> _vehicleRepository = new();
    private readonly CreateClientHandler _handler;

    public CreateClientHandlerTests()
    {
        _handler = new CreateClientHandler(_clientRepository.Object, _vehicleRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateClientAggregate_WhenDataIsValid()
    {
        Client? captured = null;
        _clientRepository.Setup(r => r.ExistsByDocumentAsync("52998224725", null)).ReturnsAsync(false);
        _vehicleRepository.Setup(r => r.ExistsByLicensePlateAsync("ABC1D23", null)).ReturnsAsync(false);
        _clientRepository.Setup(r => r.CreateAsync(It.IsAny<Client>()))
            .Callback<Client>(client => captured = client)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(BuildCommand(), default);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Document.Should().Be("52998224725");
        captured.IsActive.Should().BeTrue();
        captured.Addresses.Should().HaveCount(1);
        captured.Vehicles.Should().HaveCount(1);
        captured.Vehicles[0].LicensePlate.Should().Be("ABC1D23");
        captured.Vehicles[0].Type.Should().Be(VehicleType.Car);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenDocumentAlreadyExists()
    {
        _clientRepository.Setup(r => r.ExistsByDocumentAsync("52998224725", null)).ReturnsAsync(true);

        var result = await _handler.Handle(BuildCommand(), default);

        result.Status.Should().Be(ResultStatus.Conflict);
        _clientRepository.Verify(r => r.CreateAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenVehiclePlateAlreadyExists()
    {
        _clientRepository.Setup(r => r.ExistsByDocumentAsync("52998224725", null)).ReturnsAsync(false);
        _vehicleRepository.Setup(r => r.ExistsByLicensePlateAsync("ABC1D23", null)).ReturnsAsync(true);

        var result = await _handler.Handle(BuildCommand(), default);

        result.Status.Should().Be(ResultStatus.Conflict);
        _clientRepository.Verify(r => r.CreateAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenRequestRepeatsVehiclePlate()
    {
        var command = BuildCommand(vehicles:
        [
            BuildVehicle("abc1d23"),
            BuildVehicle("ABC1D23")
        ]);
        _clientRepository.Setup(r => r.ExistsByDocumentAsync("52998224725", null)).ReturnsAsync(false);

        var result = await _handler.Handle(command, default);

        result.Status.Should().Be(ResultStatus.Conflict);
        _clientRepository.Verify(r => r.CreateAsync(It.IsAny<Client>()), Times.Never);
    }

    private static CreateClientCommand BuildCommand(List<CreateClientVehicleDto>? vehicles = null) => new(
        "Maria Oliveira",
        "maria@email.com",
        null,
        "529.982.247-25",
        [BuildAddress()],
        vehicles ?? [BuildVehicle("abc1d23")]);

    private static CreateAddressDto BuildAddress() => new(
        AddressType.Residential,
        "Rua das Flores",
        "100",
        null,
        "Centro",
        "Sao Paulo",
        "SP",
        "01310-100");

    private static CreateClientVehicleDto BuildVehicle(string licensePlate) => new(
        licensePlate,
        "Honda",
        "Civic",
        VehicleType.Car,
        2020,
        "Prata",
        null);
}
