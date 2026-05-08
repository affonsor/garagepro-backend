namespace GaragePro.Core.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string Document { get; set; } = string.Empty;
    public string Tier { get; set; } = "standard";
    public DateOnly? Birthday { get; set; }
    public string? AddressText { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Address> Addresses { get; set; } = [];
    public List<Vehicle> Vehicles { get; set; } = [];
    public ICollection<ServiceOrder> ServiceOrders { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
