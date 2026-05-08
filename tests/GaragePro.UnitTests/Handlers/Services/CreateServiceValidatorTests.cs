using FluentAssertions;
using GaragePro.Application.Features.Services;
using GaragePro.Application.Features.Services.Create;
using GaragePro.Core.Enums;

namespace GaragePro.UnitTests.Handlers.Services;

public class CreateServiceValidatorTests
{
    private readonly CreateServiceValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenVehiclePricesContainMultipleTypes()
    {
        var command = new CreateServiceCommand(
            "Lavagem simples",
            "Lavagem externa",
            [
                new ServiceVehiclePriceDto(VehicleType.Car, 70m),
                new ServiceVehiclePriceDto(VehicleType.Suv, 90m),
                new ServiceVehiclePriceDto(VehicleType.Pickup, 120m)
            ]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenVehiclePricesAreEmpty()
    {
        var command = new CreateServiceCommand("Lavagem simples", null, []);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VehiclePrices");
    }

    [Fact]
    public void Validate_ShouldFail_WhenVehiclePricesContainDuplicateVehicleType()
    {
        var command = new CreateServiceCommand(
            "Lavagem simples",
            null,
            [
                new ServiceVehiclePriceDto(VehicleType.Car, 70m),
                new ServiceVehiclePriceDto(VehicleType.Car, 80m)
            ]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VehiclePrices");
    }

    [Fact]
    public void Validate_ShouldFail_WhenVehiclePriceIsNegative()
    {
        var command = new CreateServiceCommand(
            "Lavagem simples",
            null,
            [new ServiceVehiclePriceDto(VehicleType.Car, -1m)]);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "VehiclePrices[0].Price");
    }
}
