using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JewelryHub.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JewelryHub.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public TokenPair GenerateTokenPair(Guid userId, string email, IReadOnlyCollection<string> roles)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpires = now.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpires,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Opaque, high-entropy refresh token — not a JWT itself, so it
        // carries no decodable claims and only has meaning by lookup
        // against the stored (hashed) RefreshToken row.
        var refreshToken = GenerateSecureRandomToken();
        var refreshTokenExpires = now.AddDays(_settings.RefreshTokenDays);

        return new TokenPair(accessToken, accessTokenExpires, refreshToken, refreshTokenExpires);
    }

    public string HashRefreshToken(string rawRefreshToken)
    {
        // SHA-256 (not BCrypt) is intentional here: the refresh token is
        // already 256 bits of CSPRNG entropy, not a low-entropy password,
        // so it needs no salted/slow KDF — a fast, deterministic hash is
        // correct and lets the DB look it up by equality instead of
        // iterating every stored token to run a slow Verify().
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawRefreshToken));
        return Convert.ToHexString(bytes);
    }

    private static string GenerateSecureRandomToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
