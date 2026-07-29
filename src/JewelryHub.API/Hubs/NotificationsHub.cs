using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace JewelryHub.API.Hubs;

[Authorize]
public class NotificationsHub : Hub
{
    /// <summary>Group name convention: every connection for a given user joins "user:{userId}", so NotifyUserAsync can target them without a connection-id lookup table.</summary>
    public static string GroupForUser(Guid userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupForUser(Guid.Parse(userId)));
        }

        await base.OnConnectedAsync();
    }
}
