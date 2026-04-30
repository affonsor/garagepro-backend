using FluentAssertions;
using GaragePro.Application.Features.Appointments.Create;

namespace GaragePro.UnitTests.Handlers.Appointments;

public class CreateAppointmentValidatorTests
{
    private readonly CreateAppointmentValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenExpectedEndAtIsBeforeStartAt()
    {
        var start = DateTimeOffset.UtcNow.AddHours(2);
        var cmd = new CreateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            start, start.AddMinutes(-30), null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ExpectedEndAt");
    }

    [Fact]
    public void Validate_ShouldFail_WhenClientIdIsEmpty()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var cmd = new CreateAppointmentCommand(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(),
            start, start.AddHours(1), null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClientId");
    }

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var cmd = new CreateAppointmentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            start, start.AddHours(2), null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }
}
