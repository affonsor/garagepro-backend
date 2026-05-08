using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Products.Create;

public class CreateProductHandler(IProductRepository productRepository) : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Sku = NormalizeOptional(request.Sku),
            Brand = request.Brand,
            Category = request.Category,
            Size = request.Size,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? "un" : request.Unit.Trim(),
            Description = request.Description,
            Cost = request.Cost,
            Price = request.Price,
            Stock = request.Stock,
            MinStock = request.MinStock,
            Supplier = request.Supplier,
            Barcode = request.Barcode,
            IsActive = request.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await productRepository.CreateAsync(product);
        return Result<Guid>.Success(product.Id);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
