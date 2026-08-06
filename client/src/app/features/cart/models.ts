// Mirrors JewelryHub.Application.Features.Cart.Common.CartDto/CartItemDto.
export interface CartItem {
  id: string;
  productId: string;
  productName: string;
  productImageUrl: string | null;
  productSlug: string;
  unitPriceSnapshot: number;
  currentUnitPrice: number;
  priceHasChanged: boolean;
  quantity: number;
  lineTotal: number;
  isInStock: boolean;
}

export interface Cart {
  id: string;
  items: CartItem[];
  subtotal: number;
  totalItemCount: number;
}
