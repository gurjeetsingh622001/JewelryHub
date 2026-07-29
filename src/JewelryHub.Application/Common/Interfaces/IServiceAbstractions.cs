namespace JewelryHub.Application.Common.Interfaces;

/// <summary>Implemented in Infrastructure (BCrypt). Application never touches a hashing library directly.</summary>
public interface IPasswordHasher
{
    string Hash(string plainTextPassword);
    bool Verify(string plainTextPassword, string passwordHash);
}

/// <summary>Result of issuing a token pair, returned to the caller and to the API layer for the HTTP response.</summary>
public record TokenPair(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);

/// <summary>
/// Implemented in Infrastructure. Takes a User plus its resolved role
/// names (Application resolves roles via IUnitOfWork before calling this,
/// so Infrastructure stays free of EF Core/domain-query concerns) and
/// issues a signed JWT plus an opaque refresh token.
/// </summary>
public interface IJwtTokenService
{
    TokenPair GenerateTokenPair(Guid userId, string email, IReadOnlyCollection<string> roles);

    /// <summary>Hashes a refresh token the same way it's hashed at issuance, for lookup/comparison against the stored RefreshToken.TokenHash.</summary>
    string HashRefreshToken(string rawRefreshToken);
}

/// <summary>
/// Abstracts "who is making this request" away from HttpContext so the
/// Application and Persistence layers (audit interceptor) never take a
/// dependency on ASP.NET Core.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsInRole(string role);
}
