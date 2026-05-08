using FluentAssertions;
using GaragePro.Application.Features.Clients.Update;

namespace GaragePro.UnitTests.Handlers.Clients;

public class UpdateClientValidatorTests
{
    private readonly UpdateClientValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenDocumentIsAValidCnpj()
    {
        var command = new UpdateClientCommand(
            Guid.NewGuid(),
            "Oficina Cliente Ltda",
            null,
            null,
            "04.252.011/0001-10");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenDocumentIsEmpty()
    {
        var command = new UpdateClientCommand(Guid.NewGuid(), "Maria", null, null, "");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Document");
    }
}
