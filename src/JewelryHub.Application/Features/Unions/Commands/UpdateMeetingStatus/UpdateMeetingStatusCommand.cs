using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Commands.UpdateMeetingStatus;

public record UpdateMeetingStatusCommand(Guid MeetingId, MeetingStatus Status) : IRequest;

public class UpdateMeetingStatusCommandHandler : IRequestHandler<UpdateMeetingStatusCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateMeetingStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateMeetingStatusCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetByIdAsync(request.MeetingId, cancellationToken)
            ?? throw new NotFoundException(nameof(Meeting), request.MeetingId);

        await UnionAuthorization.EnsureOfficerOrAdminAsync(_unitOfWork, _currentUser, meeting.UnionId, cancellationToken);

        meeting.Status = request.Status;
        _unitOfWork.Meetings.Update(meeting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
