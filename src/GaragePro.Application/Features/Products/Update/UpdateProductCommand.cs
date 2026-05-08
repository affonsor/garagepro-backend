using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Products.Update;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string? Sku = null,
    string? Brand = null,
    string? Category = null,
    string? Size = null,
    string Unit = "un",
    decimal Cost = 0,
    decimal Stock = 0,
    decimal MinStock = 0,
    string? Supplier = null,
    string? Barcode = null,
    bool Active = true) : IRequest<Result<Guid>>;
