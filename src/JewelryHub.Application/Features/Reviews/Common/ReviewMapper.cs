using JewelryHub.Domain.Reviews;

namespace JewelryHub.Application.Features.Reviews.Common;

/// <summary>Caller must have loaded Customer.User first.</summary>
public static class ReviewMapper
{
    public static ReviewDto ToDto(Review r) => new(
        r.Id, r.ProductId, r.SellerId, r.Customer.User.FirstName,
        r.ProductRating, r.SellerRating, r.Title, r.Comment,
        r.SellerResponse, r.SellerRespondedAtUtc, r.CreatedAtUtc);
}
