using GaragePro.Core.Enums;

namespace GaragePro.Core.Entities;

public class ServiceVehiclePrice
{
    public Guid ServiceId { get; set; }
    public VehicleType VehicleType { get; set; }
    public decimal Price { get; set; }

    public Service Service { get; set; } = null!;
}
