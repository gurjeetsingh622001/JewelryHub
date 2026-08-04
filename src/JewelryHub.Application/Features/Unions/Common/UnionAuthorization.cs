using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Common;

/// <summary>
/// Every union governance command (meetings, polls, announcements, events,
/// membership decisions) needs the same "is this caller an officer of this
/// specific union, or a platform Admin" check — pulled out here instead of
/// repeating the query in every handler, since it's identical across ~10
/// commands rather than the usual one-off per-handler logic.
/// </summary>
public static class UnionAuthorization
{
    private static readonly UnionMemberRole[] OfficerRoles =
    {
        UnionMemberRole.President, UnionMemberRole.VicePresident, UnionMemberRole.Secretary,
    };

    public static async Task EnsureOfficerOrAdminAsync(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser, Guid unionId, CancellationToken cancellationToken)
    {
        if (currentUser.IsInRole("Admin"))
        {
            return;
        }

        var isOfficer = await unitOfWork.UnionMembers.Query().AnyAsync(m =>
            m.UnionId == unionId &&
            m.Seller.UserId == currentUser.UserId &&
            m.Status == UnionMembershipStatus.Active &&
            OfficerRoles.Contains(m.Role),
            cancellationToken);

        if (!isOfficer)
        {
            throw new ForbiddenAccessException("Only a union officer (President, Vice President, or Secretary) or an Admin can perform this action.");
        }
    }

    /// <summary>Resolves the caller's own active membership row for a union, or throws if they aren't one.</summary>
    public static async Task<UnionMember> GetActiveMembershipAsync(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser, Guid unionId, CancellationToken cancellationToken)
    {
        return await unitOfWork.UnionMembers.QueryTracking().Include(m => m.Seller).FirstOrDefaultAsync(m =>
            m.UnionId == unionId && m.Seller.UserId == currentUser.UserId && m.Status == UnionMembershipStatus.Active,
            cancellationToken)
            ?? throw new BusinessRuleException("You must be an active member of this union to perform this action.");
    }

    /// <summary>
    /// Like GetActiveMembershipAsync, but also requires an officer role.
    /// Used for actions that need to record "which member did this"
    /// (creating a meeting, poll, or announcement) — there's no bare-Admin
    /// bypass here, because Admin accounts don't have a UnionMember row to
    /// attribute the record to.
    /// </summary>
    public static async Task<UnionMember> GetActiveOfficerMembershipAsync(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser, Guid unionId, CancellationToken cancellationToken)
    {
        var membership = await GetActiveMembershipAsync(unitOfWork, currentUser, unionId, cancellationToken);
        if (!OfficerRoles.Contains(membership.Role))
        {
            throw new ForbiddenAccessException("Only a union officer (President, Vice President, or Secretary) can perform this action.");
        }
        return membership;
    }
}
