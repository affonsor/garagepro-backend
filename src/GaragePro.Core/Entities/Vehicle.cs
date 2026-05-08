using GaragePro.Core.Enums;

namespace GaragePro.Core.Entities;

public class Vehicle
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public VehicleType Type { get; set; }
    public int Year { get; set; }
    public string? Color { get; set; }
    public string? VIN { get; set; }
    public List<VehicleTransferRecord> TransferHistory { get; set; } = [];
    public ICollection<ServiceOrder> ServiceOrders { get; set; } = [];
    public Client Client { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
