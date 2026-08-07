// Mirrors ReviewDto.
export interface Review {
  id: string;
  productId: string;
  sellerId: string;
  customerFirstName: string;
  productRating: number;
  sellerRating: number;
  title: string | null;
  comment: string | null;
  sellerResponse: string | null;
  sellerRespondedAtUtc: string | null;
  createdAtUtc: string;
}

// Mirrors CreateReviewCommand.
export interface CreateReviewRequest {
  orderItemId: string;
  productRating: number;
  sellerRating: number;
  title?: string | null;
  comment?: string | null;
}
