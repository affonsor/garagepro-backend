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
            Description = request.Description,
            Price = request.Price,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await productRepository.CreateAsync(product);
        return Result<Guid>.Success(product.Id);
    }
}
