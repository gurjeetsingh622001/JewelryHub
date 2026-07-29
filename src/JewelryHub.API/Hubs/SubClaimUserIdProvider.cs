using Microsoft.AspNetCore.SignalR;

namespace JewelryHub.API.Hubs;

public class SubClaimUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) => connection.User?.FindFirst("sub")?.Value;
}
