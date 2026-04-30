using FluentValidation;

namespace GaragePro.Application.Features.Appointments.Reschedule;

public class RescheduleAppointmentValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentValidator()
    {
        RuleFor(x => x.NewStartAt)
            .Must(d => d > DateTimeOffset.UtcNow)
            .WithMessage("A nova data de início deve ser no futuro.");

        RuleFor(x => x.NewExpectedEndAt)
            .Must((cmd, end) => end > cmd.NewStartAt)
            .WithMessage("Previsão de término deve ser posterior ao início.");
    }
}
