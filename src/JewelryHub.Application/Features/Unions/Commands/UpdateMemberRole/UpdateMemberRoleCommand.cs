using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.UpdateMemberRole;

public record UpdateMemberRoleCommand(Guid MembershipId, UnionMemberRole Role) : IRequest;

public class UpdateMemberRoleCommandValidator : AbstractValidator<UpdateMemberRoleCommand>
{
    public UpdateMemberRoleCommandValidator()
    {
        RuleFor(x => x.Role).IsInEnum();
    }
}

public class UpdateMemberRoleCommandHandler : IRequestHandler<UpdateMemberRoleCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public UpdateMemberRoleCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var membership = await _unitOfWork.UnionMembers.QueryTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MembershipId, cancellationToken)
            ?? throw new NotFoundException(nameof(UnionMember), request.MembershipId);

        await UnionAuthorization.EnsureOfficerOrAdminAsync(_unitOfWork, _currentUser, membership.UnionId, cancellationToken);

        if (membership.Status != UnionMembershipStatus.Active)
        {
            throw new BusinessRuleException("Only an active member's role can be changed.");
        }

        membership.Role = request.Role;
        _unitOfWork.UnionMembers.Update(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
