using GaragePro.Core.Entities;

namespace GaragePro.Application.Features.Products;

public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? Sku,
    string? Brand,
    string? Category,
    string? Size,
    string Unit,
    decimal Cost,
    decimal Stock,
    decimal MinStock,
    string? Supplier,
    string? Barcode,
    bool Active)
{
    public static ProductResponse From(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.CreatedAt,
        product.UpdatedAt,
        product.Sku,
        product.Brand,
        product.Category,
        product.Size,
        product.Unit,
        product.Cost,
        product.Stock,
        product.MinStock,
        product.Supplier,
        product.Barcode,
        product.IsActive);
}

public record ProductMovementSummaryResponse(
    decimal SoldThisMonth,
    decimal InternalConsumption,
    DateTimeOffset? LastPurchaseAt);
