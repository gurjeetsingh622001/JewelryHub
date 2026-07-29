using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Reviews.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Reviews.Queries.GetSellerReviews;

public record GetSellerReviewsQuery(Guid SellerId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<ReviewDto>>;

public class GetSellerReviewsQueryHandler : IRequestHandler<GetSellerReviewsQuery, PagedResult<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSellerReviewsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<ReviewDto>> Handle(GetSellerReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Reviews.Query()
            .Include(r => r.Customer).ThenInclude(c => c.User)
            .Where(r => r.SellerId == request.SellerId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(ReviewMapper.ToDto).ToList();
        return new PagedResult<ReviewDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
