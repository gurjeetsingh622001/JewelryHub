using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Sellers.Common;
using JewelryHub.Domain.Sellers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Sellers.Queries.GetSellerProfile;

/// <summary>SellerId null = "my own profile"; the controller only allows a non-null SellerId for Admin callers.</summary>
public record GetSellerProfileQuery(Guid? SellerId) : IRequest<SellerDto>;

public class GetSellerProfileQueryHandler : IRequestHandler<GetSellerProfileQuery, SellerDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetSellerProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<SellerDto> Handle(GetSellerProfileQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Sellers.Query().Include(s => s.Documents);

        var seller = request.SellerId is null
            ? await query.FirstOrDefaultAsync(s => s.UserId == _currentUser.UserId, cancellationToken)
            : await query.FirstOrDefaultAsync(s => s.Id == request.SellerId, cancellationToken);

        if (seller is null)
        {
            throw request.SellerId is null
                ? new BusinessRuleException("No seller profile is associated with this account.")
                : new NotFoundException(nameof(Seller), request.SellerId!);
        }

        return SellerMapper.ToDto(seller);
    }
}
