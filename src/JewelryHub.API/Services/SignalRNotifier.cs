using JewelryHub.API.Hubs;
using JewelryHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace JewelryHub.API.Services;

/// <summary>
/// Lives in the API project (not Infrastructure) because IHubContext is
/// tied to the specific Hub type registered by the ASP.NET Core host —
/// this is host/composition-layer code, not swappable infrastructure like
/// email or file storage.
/// </summary>
public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationsHub> _hubContext;

    public SignalRNotifier(IHubContext<NotificationsHub> hubContext) => _hubContext = hubContext;

    public Task NotifyUserAsync(Guid userId, object payload, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group(NotificationsHub.GroupForUser(userId))
            .SendAsync("notification", payload, cancellationToken);
}
