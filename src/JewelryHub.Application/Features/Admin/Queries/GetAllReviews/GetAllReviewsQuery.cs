using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Admin.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Admin.Queries.GetAllReviews;

/// <summary>
/// Platform-wide review browsing for moderation — unlike GetProductReviews/
/// GetSellerReviews, this deliberately does NOT filter to IsApproved only,
/// since an admin needs to see hidden/flagged reviews too, and there's no
/// existing "queue" of reviews awaiting first moderation (IsApproved
/// defaults to true; moderation here is reactive, not a review queue).
/// </summary>
public record GetAllReviewsQuery(bool? IsApproved, bool? IsFlagged, int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<AdminReviewDto>>;

public class GetAllReviewsQueryHandler : IRequestHandler<GetAllReviewsQuery, PagedResult<AdminReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllReviewsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<AdminReviewDto>> Handle(GetAllReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Reviews.Query()
            .Include(r => r.Customer).ThenInclude(c => c.User)
            .Include(r => r.Product)
            .Include(r => r.Seller)
            .AsQueryable();

        if (request.IsApproved is not null)
        {
            query = query.Where(r => r.IsApproved == request.IsApproved.Value);
        }

        if (request.IsFlagged is not null)
        {
            query = query.Where(r => r.IsFlagged == request.IsFlagged.Value);
        }

        query = query.OrderByDescending(r => r.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(r => new AdminReviewDto(
            r.Id, r.ProductId, r.Product.Name, r.SellerId, r.Seller.BusinessName, r.Customer.User.FirstName,
            r.ProductRating, r.SellerRating, r.Title, r.Comment, r.IsApproved, r.IsFlagged, r.CreatedAtUtc)).ToList();

        return new PagedResult<AdminReviewDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
