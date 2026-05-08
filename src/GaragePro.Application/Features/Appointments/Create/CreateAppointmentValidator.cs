using FluentValidation;

namespace GaragePro.Application.Features.Appointments.Create;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.StartAt).NotEmpty();
        RuleFor(x => x.ExpectedEndAt)
            .Must((cmd, end) => end > cmd.StartAt)
            .WithMessage("Previsão de término deve ser posterior ao início.");
    }
}
