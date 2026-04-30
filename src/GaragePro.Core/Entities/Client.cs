namespace GaragePro.Core.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Document { get; set; }
    public List<Address> Addresses { get; set; } = [];
    public List<Vehicle> Vehicles { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
