namespace GaragePro.Core.Entities;

public class Service
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Tier { get; set; } = "standard";
    public string? Category { get; set; }
    public string? Duration { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public List<ServiceVehiclePrice> VehiclePrices { get; set; } = [];
    public List<ServiceMaterial> Materials { get; set; } = [];
    public List<ServiceStep> Steps { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ServiceOrderServiceLine> ServiceOrderServiceLines { get; set; } = [];
}
