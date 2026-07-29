namespace JewelryHub.Application.Common.Interfaces;

/// <summary>
/// Pushes a live update to a connected client. Deliberately separate from
/// persisting the Notification row (INotificationService does both) —
/// this piece only fires the SignalR message and is a no-op if the user
/// isn't currently connected; the persisted row is what makes the
/// notification durable/visible later regardless of connection state.
/// </summary>
public interface IRealtimeNotifier
{
    Task NotifyUserAsync(Guid userId, object payload, CancellationToken cancellationToken = default);
}
