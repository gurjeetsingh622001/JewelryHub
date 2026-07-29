using JewelryHub.Domain.Common;
using JewelryHub.Domain.Identity;

namespace JewelryHub.Domain.Notifications;

/// <summary>
/// In-app notification, pushed in real time over SignalR when the
/// recipient is connected and always persisted so it also appears in a
/// notification inbox / can be delivered later (offline user).
/// </summary>
public class Notification : BaseEntity
{
    public Guid RecipientUserId { get; set; }
    public User RecipientUser { get; set; } = default!;

    public string Type { get; set; } = default!; // "Order", "Payment", "Union", "System" — see NotificationType-style constants in Application layer
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? LinkUrl { get; set; } // deep-link into the SPA, e.g. /orders/{id}

    public bool IsRead { get; set; }
    public DateTime? ReadAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
