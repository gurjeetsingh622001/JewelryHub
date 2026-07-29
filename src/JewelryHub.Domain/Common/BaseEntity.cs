namespace JewelryHub.Domain.Common;

/// <summary>
/// Base class for every entity in the domain. Guid keys are used instead of
/// sequential ints so identifiers never leak business volume (e.g. total
/// order count) and can be generated client-side before persistence.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    // Optimistic concurrency token (maps to SQL Server rowversion).
    public byte[]? RowVersion { get; set; }
}

/// <summary>
/// Adds the standard audit trail fields required across the platform
/// (who created/modified a record, and when). Kept separate from
/// BaseEntity because a handful of pure lookup/junction entities don't
/// need a full audit trail.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }
    public Guid? ModifiedBy { get; set; }
}

/// <summary>
/// Implemented by entities that must be soft-deleted rather than physically
/// removed (financial records, orders, union governance records, etc.).
/// EF Core global query filters use this to hide deleted rows by default.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
    Guid? DeletedBy { get; set; }
}
