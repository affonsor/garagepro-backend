using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Products.Update;

public class UpdateProductHandler(IProductRepository productRepository) : IRequestHandler<UpdateProductCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.Id);
        if (product is null)
            return Result<Guid>.NotFound("Product not found");

        product.Name = request.Name;
        product.Sku = NormalizeOptional(request.Sku);
        product.Brand = request.Brand;
        product.Category = request.Category;
        product.Size = request.Size;
        product.Unit = string.IsNullOrWhiteSpace(request.Unit) ? "un" : request.Unit.Trim();
        product.Description = request.Description;
        product.Cost = request.Cost;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.MinStock = request.MinStock;
        product.Supplier = request.Supplier;
        product.Barcode = request.Barcode;
        product.IsActive = request.Active;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await productRepository.UpdateAsync(product);
        return Result<Guid>.Success(product.Id);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
