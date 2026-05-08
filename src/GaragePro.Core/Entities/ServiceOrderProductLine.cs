namespace GaragePro.Core.Entities;

public class ServiceOrderProductLine
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public ServiceOrder ServiceOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
