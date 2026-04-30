namespace GaragePro.Application.Features.Services;

public record ServiceResponse(Guid Id, string Name, string? Description, decimal Price, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
