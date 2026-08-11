using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Admin.Commands.SetUserActiveStatus;

/// <summary>
/// Deactivating a user blocks future logins (LoginCommand already checks
/// User.IsActive) — it does not revoke sessions already in flight, since
/// that would need every RefreshToken for the user explicitly revoked too.
/// Good enough for a v1 "stop a problem account from logging in again."
/// </summary>
public record SetUserActiveStatusCommand(Guid UserId, bool IsActive) : IRequest;

public class SetUserActiveStatusCommandHandler : IRequestHandler<SetUserActiveStatusCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public SetUserActiveStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(SetUserActiveStatusCommand request, CancellationToken cancellationToken)
    {
        if (!request.IsActive && request.UserId == _currentUser.UserId)
        {
            throw new BusinessRuleException("You cannot deactivate your own account.");
        }

        var user = await _unitOfWork.Users.QueryTracking().FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        user.IsActive = request.IsActive;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
