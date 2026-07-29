using JewelryHub.Domain.Common;

namespace JewelryHub.Domain.Identity;

/// <summary>
/// Platform-level role (Customer, Seller, Admin, SuperAdmin, ...). Kept as
/// data rather than a hardcoded enum so admins can introduce new roles
/// (e.g. "Support Agent") without a code deployment.
/// </summary>
public class Role : AuditableEntity
{
    public string Name { get; set; } = default!;          // "Admin", "Seller", "Customer"
    public string NormalizedName { get; set; } = default!; // uppercase, for unique index / lookups
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; } // system roles cannot be deleted from the admin UI

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>
/// A granular capability (e.g. "products.approve", "orders.refund",
/// "unions.manage"). Permissions let the API enforce authorization more
/// precisely than role checks alone, and let an admin grant a narrow
/// capability without inventing a whole new role.
/// </summary>
public class Permission : BaseEntity
{
    public string Code { get; set; } = default!; // "products.approve"
    public string Module { get; set; } = default!; // "Products", "Orders", "Unions" — for grouping in the admin UI
    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>Join entity: which users hold which roles (a user may hold more than one).</summary>
public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Join entity: which permissions a role grants.</summary>
public class RolePermission : BaseEntity
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = default!;
}
