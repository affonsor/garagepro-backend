using FluentAssertions;
using GaragePro.Application.Features.Appointments.Reschedule;

namespace GaragePro.UnitTests.Handlers.Appointments;

public class RescheduleAppointmentValidatorTests
{
    private readonly RescheduleAppointmentValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenNewStartAtIsInThePast()
    {
        var past = DateTimeOffset.UtcNow.AddHours(-1);
        var cmd = new RescheduleAppointmentCommand(Guid.NewGuid(), past, past.AddHours(1), null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewStartAt");
    }

    [Fact]
    public void Validate_ShouldFail_WhenNewExpectedEndAtIsBeforeNewStartAt()
    {
        var start = DateTimeOffset.UtcNow.AddHours(2);
        var cmd = new RescheduleAppointmentCommand(Guid.NewGuid(), start, start.AddMinutes(-30), null);

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewExpectedEndAt");
    }

    [Fact]
    public void Validate_ShouldPass_WhenAllFieldsAreValid()
    {
        var start = DateTimeOffset.UtcNow.AddHours(2);
        var cmd = new RescheduleAppointmentCommand(Guid.NewGuid(), start, start.AddHours(2), "Motivo");

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }
}
