using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

namespace GaragePro.Infrastructure.Services;

public class AuthService(IConfiguration config) : IAuthService
{
    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(config["Jwt:Secret"]!);
        var expiryMinutes = config.GetValue<int>("Jwt:ExpiryMinutes");
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in user.Roles)
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt.UtcDateTime,
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return (handler.WriteToken(token), expiresAt);
    }

    public string HashPassword(string plain) =>
        BC.HashPassword(plain, workFactor: 12);

    public bool VerifyPassword(string plain, string hash) =>
        BC.Verify(plain, hash);
}
