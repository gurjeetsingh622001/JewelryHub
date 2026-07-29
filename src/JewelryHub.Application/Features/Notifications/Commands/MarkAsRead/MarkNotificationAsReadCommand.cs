using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Notifications;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Notifications.Commands.MarkAsRead;

public record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.Notifications.QueryTracking()
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.NotificationId);

        if (notification.RecipientUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You can only manage your own notifications.");
        }

        if (notification.IsRead) return;

        notification.IsRead = true;
        notification.ReadAtUtc = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
