using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Notifications;

namespace JewelryHub.Application.Common.Services;

public interface INotificationService
{
    Task NotifyAsync(Guid recipientUserId, string type, string title, string message, string? linkUrl = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Persists first, then pushes — a client that's offline when the event
/// happens still sees the notification in their inbox next time they log
/// in, since the realtime push is a delivery optimization, not the source
/// of truth.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public NotificationService(IUnitOfWork unitOfWork, IRealtimeNotifier realtimeNotifier)
    {
        _unitOfWork = unitOfWork;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task NotifyAsync(Guid recipientUserId, string type, string title, string message, string? linkUrl = null, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            LinkUrl = linkUrl,
        };

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtimeNotifier.NotifyUserAsync(recipientUserId, new
        {
            notification.Id,
            notification.Type,
            notification.Title,
            notification.Message,
            notification.LinkUrl,
            notification.CreatedAtUtc,
        }, cancellationToken);
    }
}
