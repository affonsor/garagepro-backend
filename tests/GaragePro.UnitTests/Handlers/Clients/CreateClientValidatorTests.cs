using FluentAssertions;
using GaragePro.Application.Features.Clients.Create;
using GaragePro.Core.Enums;

namespace GaragePro.UnitTests.Handlers.Clients;

public class CreateClientValidatorTests
{
    private readonly CreateClientValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        var result = _validator.Validate(BuildCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenDocumentIsInvalid()
    {
        var command = BuildCommand(document: "11111111111");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Document");
    }

    [Fact]
    public void Validate_ShouldFail_WhenAddressesAreEmpty()
    {
        var command = BuildCommand(addresses: []);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Addresses");
    }

    [Fact]
    public void Validate_ShouldFail_WhenVehiclesAreEmpty()
    {
        var command = BuildCommand(vehicles: []);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Vehicles");
    }

    private static CreateClientCommand BuildCommand(
        string document = "529.982.247-25",
        List<CreateAddressDto>? addresses = null,
        List<CreateClientVehicleDto>? vehicles = null) => new(
            "Maria Oliveira",
            "maria@email.com",
            null,
            document,
            addresses ?? [BuildAddress()],
            vehicles ?? [BuildVehicle()]);

    private static CreateAddressDto BuildAddress() => new(
        AddressType.Residential,
        "Rua das Flores",
        "100",
        null,
        "Centro",
        "Sao Paulo",
        "SP",
        "01310-100");

    private static CreateClientVehicleDto BuildVehicle() => new(
        "abc1d23",
        "Honda",
        "Civic",
        VehicleType.Car,
        2020,
        "Prata",
        null);
}
