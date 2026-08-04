using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.RequestMembership;

/// <summary>
/// A seller asks to join an already-approved union. If they were a member
/// before and later removed/deactivated, this re-opens that same row for
/// review instead of creating a second one — UnionMember has a unique
/// index on (UnionId, SellerId).
/// </summary>
public record RequestMembershipCommand(Guid UnionId) : IRequest<UnionMemberDto>;

public class RequestMembershipCommandHandler : IRequestHandler<RequestMembershipCommand, UnionMemberDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public RequestMembershipCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<UnionMemberDto> Handle(RequestMembershipCommand request, CancellationToken cancellationToken)
    {
        var seller = await _unitOfWork.Sellers.Query().FirstOrDefaultAsync(s => s.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("No seller profile is associated with this account.");

        if (seller.VerificationStatus != SellerVerificationStatus.Approved)
        {
            throw new BusinessRuleException("Only a KYC-approved seller can join a union.");
        }

        var union = await _unitOfWork.Unions.GetByIdAsync(request.UnionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Union), request.UnionId);

        if (!union.IsApprovedByAdmin)
        {
            throw new BusinessRuleException("This union is not yet approved and cannot accept new members.");
        }

        var existing = await _unitOfWork.UnionMembers.QueryTracking()
            .FirstOrDefaultAsync(m => m.UnionId == request.UnionId && m.SellerId == seller.Id, cancellationToken);

        UnionMember member;
        if (existing is not null)
        {
            if (existing.Status is UnionMembershipStatus.Active or UnionMembershipStatus.PendingApproval)
            {
                throw new BusinessRuleException("You already have a membership request or active membership in this union.");
            }

            existing.Status = UnionMembershipStatus.PendingApproval;
            existing.Role = UnionMemberRole.Member;
            existing.JoinedAtUtc = DateTime.UtcNow;
            _unitOfWork.UnionMembers.Update(existing);
            member = existing;
        }
        else
        {
            member = new UnionMember { UnionId = request.UnionId, SellerId = seller.Id };
            await _unitOfWork.UnionMembers.AddAsync(member, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        member.Seller = seller;
        return UnionMapper.ToDto(member);
    }
}
