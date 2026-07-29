using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Cart.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Cart.Queries.GetCart;

public record GetCartQuery : IRequest<CartDto>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetCartQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.Query()
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Inventory)
            .FirstOrDefaultAsync(c => c.Customer.UserId == _currentUser.UserId, cancellationToken);

        // An empty/never-created cart is a perfectly normal state for a
        // browsing customer — return an empty cart rather than a 404.
        if (cart is null)
        {
            return new CartDto(Guid.Empty, Array.Empty<CartItemDto>(), 0m, 0);
        }

        return CartMapper.ToDto(cart);
    }
}
