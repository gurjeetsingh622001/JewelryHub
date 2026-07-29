using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Notifications.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery(bool UnreadOnly = false, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<NotificationDto>>;

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Notifications.Query().Where(n => n.RecipientUserId == _currentUser.UserId);
        if (request.UnreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }
        query = query.OrderByDescending(n => n.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items
            .Select(n => new NotificationDto(n.Id, n.Type, n.Title, n.Message, n.LinkUrl, n.IsRead, n.CreatedAtUtc))
            .ToList();
        return new PagedResult<NotificationDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
