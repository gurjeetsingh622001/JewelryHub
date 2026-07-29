using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Orders.Common;
using JewelryHub.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto>;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payments)
            .Include(o => o.Items).ThenInclude(i => i.Seller)
            .Include(o => o.Items).ThenInclude(i => i.Taxes)
            .Include(o => o.Items).ThenInclude(i => i.Shipment)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        var isOwner = order.Customer.UserId == _currentUser.UserId;
        var isInvolvedSeller = order.Items.Any(i => i.Seller.UserId == _currentUser.UserId);

        if (!isOwner && !isInvolvedSeller && !_currentUser.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("You do not have access to this order.");
        }

        return OrderMapper.ToDto(order);
    }
}
