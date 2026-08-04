using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;

namespace JewelryHub.Application.Features.Unions.Commands.UpdateEventStatus;

public record UpdateEventStatusCommand(Guid EventId, UnionEventStatus Status) : IRequest;

public class UpdateEventStatusCommandHandler : IRequestHandler<UpdateEventStatusCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateEventStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateEventStatusCommand request, CancellationToken cancellationToken)
    {
        var unionEvent = await _unitOfWork.UnionEvents.GetByIdAsync(request.EventId, cancellationToken)
            ?? throw new NotFoundException(nameof(UnionEvent), request.EventId);

        await UnionAuthorization.EnsureOfficerOrAdminAsync(_unitOfWork, _currentUser, unionEvent.UnionId, cancellationToken);

        unionEvent.Status = request.Status;
        _unitOfWork.UnionEvents.Update(unionEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
