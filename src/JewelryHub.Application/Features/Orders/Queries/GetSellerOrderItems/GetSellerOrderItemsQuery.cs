using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Extensions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Common.Models;
using JewelryHub.Application.Features.Orders.Common;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Orders.Queries.GetSellerOrderItems;

public record GetSellerOrderItemsQuery(OrderStatus? Status = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<SellerOrderItemDto>>;

public class GetSellerOrderItemsQueryHandler : IRequestHandler<GetSellerOrderItemsQuery, PagedResult<SellerOrderItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetSellerOrderItemsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<SellerOrderItemDto>> Handle(GetSellerOrderItemsQuery request, CancellationToken cancellationToken)
    {
        var seller = await _unitOfWork.Sellers.Query()
            .FirstOrDefaultAsync(s => s.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new BusinessRuleException("No seller profile is associated with this account.");

        var query = _unitOfWork.Orders.Query()
            .SelectMany(o => o.Items)
            .Include(i => i.Order)
            .Include(i => i.Shipment)
            .Where(i => i.SellerId == seller.Id);

        if (request.Status is not null)
        {
            query = query.Where(i => i.ItemStatus == request.Status);
        }

        query = query.OrderByDescending(i => i.CreatedAtUtc);

        var paged = await query.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
        var items = paged.Items.Select(OrderMapper.ToSellerItemDto).ToList();
        return new PagedResult<SellerOrderItemDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
