namespace GaragePro.Application.Features.Products;

public record ProductResponse(Guid Id, string Name, string? Description, decimal Price, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
