using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Unions;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Commands.DeleteAnnouncement;

public record DeleteAnnouncementCommand(Guid AnnouncementId) : IRequest;

public class DeleteAnnouncementCommandHandler : IRequestHandler<DeleteAnnouncementCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DeleteAnnouncementCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _unitOfWork.UnionAnnouncements.GetByIdAsync(request.AnnouncementId, cancellationToken)
            ?? throw new NotFoundException(nameof(UnionAnnouncement), request.AnnouncementId);

        await UnionAuthorization.EnsureOfficerOrAdminAsync(_unitOfWork, _currentUser, announcement.UnionId, cancellationToken);

        _unitOfWork.UnionAnnouncements.Remove(announcement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
