using FluentValidation;
using JewelryHub.Application.Common.Exceptions;
using JewelryHub.Application.Common.Interfaces;
using JewelryHub.Application.Features.Reviews.Common;
using JewelryHub.Domain.Enums;
using JewelryHub.Domain.Orders;
using JewelryHub.Domain.Reviews;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JewelryHub.Application.Features.Reviews.Commands.CreateReview;

public record CreateReviewCommand(Guid OrderItemId, int ProductRating, int SellerRating, string? Title, string? Comment) : IRequest<ReviewDto>;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.ProductRating).InclusiveBetween(1, 5);
        RuleFor(x => x.SellerRating).InclusiveBetween(1, 5);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Comment).MaximumLength(4000);
    }
}

public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CreateReviewCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var orderItem = await _unitOfWork.Orders.QueryTracking()
            .SelectMany(o => o.Items)
            .Include(i => i.Order).ThenInclude(o => o.Customer).ThenInclude(c => c.User)
            .Include(i => i.Product)
            .Include(i => i.Seller)
            .FirstOrDefaultAsync(i => i.Id == request.OrderItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrderItem), request.OrderItemId);

        if (orderItem.Order.Customer.UserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You can only review your own purchases.");
        }

        if (orderItem.ItemStatus != OrderStatus.Delivered)
        {
            throw new BusinessRuleException("You can only review an item after it has been delivered.");
        }

        if (await _unitOfWork.Reviews.AnyAsync(r => r.OrderItemId == request.OrderItemId, cancellationToken))
        {
            throw new BusinessRuleException("You have already reviewed this purchase.");
        }

        var review = new Review
        {
            CustomerId = orderItem.Order.CustomerId,
            OrderItemId = orderItem.Id,
            ProductId = orderItem.ProductId,
            SellerId = orderItem.SellerId,
            ProductRating = request.ProductRating,
            SellerRating = request.SellerRating,
            Title = request.Title?.Trim(),
            Comment = request.Comment?.Trim(),
        };

        await _unitOfWork.Reviews.AddAsync(review, cancellationToken);

        // Incremental average update — avoids re-scanning every review for
        // this product/seller on every single new review.
        var product = orderItem.Product;
        product.AverageRating = RollingAverage(product.AverageRating, product.ReviewCount, request.ProductRating);
        product.ReviewCount += 1;
        _unitOfWork.Products.Update(product);

        var seller = orderItem.Seller;
        seller.AverageRating = RollingAverage(seller.AverageRating, seller.ReviewCount, request.SellerRating);
        seller.ReviewCount += 1;
        _unitOfWork.Sellers.Update(seller);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        review.Customer = orderItem.Order.Customer;
        return ReviewMapper.ToDto(review);
    }

    private static decimal RollingAverage(decimal currentAverage, int currentCount, int newRating) =>
        currentCount == 0 ? newRating : Math.Round((currentAverage * currentCount + newRating) / (currentCount + 1), 2);
}
