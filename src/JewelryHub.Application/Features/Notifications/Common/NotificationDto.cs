namespace JewelryHub.Application.Features.Notifications.Common;

public record NotificationDto(Guid Id, string Type, string Title, string Message, string? LinkUrl, bool IsRead, DateTime CreatedAtUtc);
