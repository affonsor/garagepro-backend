namespace GaragePro.Core.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Size { get; set; }
    public string Unit { get; set; } = "un";
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal Stock { get; set; }
    public decimal MinStock { get; set; }
    public string? Supplier { get; set; }
    public string? Barcode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ServiceMaterial> ServiceMaterials { get; set; } = [];
    public ICollection<ServiceOrderProductLine> ServiceOrderProductLines { get; set; } = [];
}
