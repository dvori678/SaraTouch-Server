using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaraTouch.Core.Entities;
using SaraTouch.Core.Interfaces.Infrastructure;
using SaraTouch.Core.Settings;

namespace SaraTouch.API.Infrastructure;

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly IClock _clock;

    public JwtTokenService(IOptions<JwtSettings> options, IClock clock)
    {
        _settings = options.Value;
        _clock = clock;
    }

    public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
    {
        var now = _clock.UtcNow;
        var expires = now.AddMinutes(_settings.ExpiryMinutes);

        // ה-claims: מה שהשרת יידע על המשתמשת בכל בקשה, בלי לגשת לבסיס הנתונים
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}