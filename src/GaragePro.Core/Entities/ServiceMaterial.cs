namespace GaragePro.Core.Entities;

public class ServiceMaterial
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }

    public Service Service { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
