namespace JewelryHub.Application.Features.Users.Common;

/// <summary>
/// PhotoUrl is sourced from whichever role-specific entity actually owns a
/// photo field — Customer.ProfileImageUrl or Seller.LogoUrl — since neither
/// concept lives on the shared User row (see User.cs's Customer/Seller
/// composition-over-inheritance note). Null for an Admin account, which has
/// no such entity.
/// </summary>
public record MyProfileDto(
    Guid UserId, string Email, string FirstName, string LastName,
    string? PhoneNumber, string? PhotoUrl, IReadOnlyList<string> Roles);
