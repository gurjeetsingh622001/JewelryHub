using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Reviews.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Reviews.Queries.GetProductReviews;

public record GetProductReviewsQuery(Guid ProductId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<ReviewDto>>;

public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, PagedResult<ReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductReviewsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<ReviewDto>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Reviews.Query()
            .Include(r => r.Customer).ThenInclude(c => c.User)
            .Where(r => r.ProductId == request.ProductId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(ReviewMapper.ToDto).ToList();
        return new PagedResult<ReviewDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
