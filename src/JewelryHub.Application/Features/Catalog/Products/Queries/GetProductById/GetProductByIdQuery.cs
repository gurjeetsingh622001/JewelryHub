using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Catalog.Products.Common;
using JewelryHub.Domain.Catalog;
using JewelryHub.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Catalog.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Seller).Include(p => p.Category)
            .Include(p => p.Images).Include(p => p.Gemstones).Include(p => p.Certificates).Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        // Draft/PendingReview/Rejected listings are only visible to their
        // owner or an admin — a public catalog page must never expose a
        // still-in-progress or rejected listing to other shoppers.
        var isPubliclyVisible = product.Status is ProductStatus.Active or ProductStatus.OutOfStock;
        var isOwnerOrAdmin = product.Seller.UserId == _currentUser.UserId || _currentUser.IsInRole("Admin");

        if (!isPubliclyVisible && !isOwnerOrAdmin)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        return ProductMapper.ToDto(product);
    }
}
