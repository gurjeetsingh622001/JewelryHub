using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Sellers.Common;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Sellers.Queries.GetPendingSellers;

public record GetPendingSellersQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<SellerDto>>;

public class GetPendingSellersQueryHandler : IRequestHandler<GetPendingSellersQuery, PagedResult<SellerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPendingSellersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<SellerDto>> Handle(GetPendingSellersQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Sellers.Query()
            .Include(s => s.Documents)
            .Where(s => s.VerificationStatus == SellerVerificationStatus.PendingApproval
                     || s.VerificationStatus == SellerVerificationStatus.UnderReview)
            .OrderBy(s => s.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(SellerMapper.ToDto).ToList();
        return new PagedResult<SellerDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
