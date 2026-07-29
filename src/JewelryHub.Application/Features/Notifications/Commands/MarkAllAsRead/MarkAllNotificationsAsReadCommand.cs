using JewelryHub.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Notifications.Commands.MarkAllAsRead;

public record MarkAllNotificationsAsReadCommand : IRequest;

public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkAllNotificationsAsReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _unitOfWork.Notifications.QueryTracking()
            .Where(n => n.RecipientUserId == _currentUser.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAtUtc = now;
        }

        if (unread.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
