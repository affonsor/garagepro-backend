using GaragePro.Core.Enums;

namespace GaragePro.Core.Entities;

public class ServiceOrder
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid VehicleId { get; set; }
    public int BoxNumber { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Client Client { get; set; } = null!;
    public Vehicle Vehicle { get; set; } = null!;
    public ICollection<ServiceOrderServiceLine> ServiceLines { get; set; } = [];
    public ICollection<ServiceOrderProductLine> ProductLines { get; set; } = [];
}
