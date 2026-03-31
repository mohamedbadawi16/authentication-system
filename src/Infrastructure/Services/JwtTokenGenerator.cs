using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationSystem.Application.Abstractions;
using AuthenticationSystem.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationSystem.Infrastructure.Services;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly IClock _clock;
    private readonly SigningCredentials _credentials;

    public JwtTokenGenerator(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        _credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public (string Token, DateTimeOffset ExpiresAt) GenerateToken(Guid userId, string email)
    {
        var expiresAt = _clock.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: _credentials);

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenValue, expiresAt);
    }
}
