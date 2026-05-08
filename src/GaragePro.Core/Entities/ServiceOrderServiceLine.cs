namespace GaragePro.Core.Entities;

public class ServiceOrderServiceLine
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public ServiceOrder ServiceOrder { get; set; } = null!;
    public Service Service { get; set; } = null!;
}
