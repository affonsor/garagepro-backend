using GaragePro.Core.Entities;

namespace GaragePro.Core.Interfaces.Services;

public interface IAuthService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user);
    string HashPassword(string plain);
    bool VerifyPassword(string plain, string hash);
}
