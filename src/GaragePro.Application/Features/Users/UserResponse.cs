namespace GaragePro.Application.Features.Users;

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    IEnumerable<string> Roles,
    DateTimeOffset CreatedAt);
