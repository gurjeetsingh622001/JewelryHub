using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Unions.Common;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Queries.GetMeetings;

/// <summary>Members-only meeting calendar for a union. Full detail (agenda/attendees/minutes) is a separate GetMeetingByIdQuery call.</summary>
public record GetMeetingsQuery(Guid UnionId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<MeetingSummaryDto>>;

public class GetMeetingsQueryHandler : IRequestHandler<GetMeetingsQuery, PagedResult<MeetingSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMeetingsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<MeetingSummaryDto>> Handle(GetMeetingsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsInRole("Admin"))
        {
            await UnionAuthorization.GetActiveMembershipAsync(_unitOfWork, _currentUser, request.UnionId, cancellationToken);
        }

        var query = _unitOfWork.Meetings.Query()
            .Where(m => m.UnionId == request.UnionId)
            .OrderByDescending(m => m.ScheduledAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(MeetingMapper.ToSummaryDto).ToList();
        return new PagedResult<MeetingSummaryDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
