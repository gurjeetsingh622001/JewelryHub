using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Unions.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Unions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Unions.Commands.CreateUnion;

/// <summary>
/// Only an already-KYC-approved seller can found a union — it's a
/// governance body for verified professionals, not an open sign-up. The
/// founder becomes the union's first member with the President role, and
/// the union itself still needs an Admin's approval before it's visible
/// to anyone else (see ApproveUnionCommand).
/// </summary>
public record CreateUnionCommand(
    string Name, string? Description, string? LogoUrl, string City, string State, decimal? AnnualMembershipFee)
    : IRequest<UnionDto>;

public class CreateUnionCommandValidator : AbstractValidator<CreateUnionCommand>
{
    public CreateUnionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AnnualMembershipFee).GreaterThanOrEqualTo(0).When(x => x.AnnualMembershipFee is not null);
    }
}

public class CreateUnionCommandHandler : IRequestHandler<CreateUnionCommand, UnionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateUnionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<UnionDto> Handle(CreateUnionCommand request, CancellationToken cancellationToken)
    {
        var seller = await _unitOfWork.Sellers.Query().FirstOrDefaultAsync(s => s.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("No seller profile is associated with this account.");

        if (seller.VerificationStatus != SellerVerificationStatus.Approved)
        {
            throw new BusinessRuleException("Only a KYC-approved seller can create a union.");
        }

        var union = new Union
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            City = request.City.Trim(),
            State = request.State.Trim(),
            CreatedBySellerId = seller.Id,
            AnnualMembershipFee = request.AnnualMembershipFee,
        };
        await _unitOfWork.Unions.AddAsync(union, cancellationToken);

        var founder = new UnionMember
        {
            UnionId = union.Id,
            SellerId = seller.Id,
            Role = UnionMemberRole.President,
            Status = UnionMembershipStatus.Active,
        };
        await _unitOfWork.UnionMembers.AddAsync(founder, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        union.CreatedBySeller = seller;
        return UnionMapper.ToDto(union, activeMemberCount: 1);
    }
}
