using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Queries.GetMeetingById;

public record GetMeetingByIdQuery(Guid MeetingId) : IRequest<MeetingDto>;

public class GetMeetingByIdQueryHandler : IRequestHandler<GetMeetingByIdQuery, MeetingDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMeetingByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<MeetingDto> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.Query().WithFullDetails()
            .FirstOrDefaultAsync(m => m.Id == request.MeetingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Meeting), request.MeetingId);

        if (!_currentUser.IsInRole("Admin"))
        {
            await UnionAuthorization.GetActiveMembershipAsync(_unitOfWork, _currentUser, meeting.UnionId, cancellationToken);
        }

        return MeetingMapper.ToDto(meeting);
    }
}
