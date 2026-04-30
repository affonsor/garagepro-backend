using FluentValidation;

namespace GaragePro.Application.Features.Vehicles.Transfer;

public class TransferVehicleValidator : AbstractValidator<TransferVehicleCommand>
{
    public TransferVehicleValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.ToClientId).NotEmpty();
    }
}
