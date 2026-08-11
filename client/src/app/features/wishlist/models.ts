// Mirrors JewelryHub.Application.Features.Wishlist.Common.WishlistDto/WishlistItemDto.
export interface WishlistItem {
  id: string;
  productId: string;
  productName: string;
  productSlug: string;
  productImageUrl: string | null;
  currentPrice: number;
  isInStock: boolean;
  addedAtUtc: string;
}

export interface Wishlist {
  id: string;
  items: WishlistItem[];
}
