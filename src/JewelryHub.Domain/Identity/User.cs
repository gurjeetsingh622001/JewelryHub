using JewelryHub.Domain.Common;

namespace JewelryHub.Domain.Identity;

/// <summary>
/// A platform account. Customers, Sellers and Admins are all Users;
/// Customer/Seller are separate profile entities linked 1:1 to a User
/// (composition over a bloated "god" User class, and it lets a single
/// account cleanly hold multiple roles, e.g. an admin who is also buying).
/// </summary>
public class User : AuditableEntity, ISoftDelete
{
    public string Email { get; set; } = default!;
    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; } = default!;
    public string SecurityStamp { get; set; } = default!; // invalidates issued JWTs on password/role change

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public int AccessFailedCount { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

/// <summary>
/// Long-lived opaque token used to silently obtain new short-lived JWT
/// access tokens without forcing re-login. Stored server-side so tokens
/// can be revoked (logout, password change, compromise) — a bare JWT
/// cannot be revoked before it expires.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAtUtc { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;
}
