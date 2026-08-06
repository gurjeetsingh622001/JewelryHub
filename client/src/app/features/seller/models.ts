import { OrderStatus, ShipmentStatus } from '../orders/models';

// Mirrors JewelryHub.Domain.Enums — numeric on the wire, see
// features/products/models.ts for why (no JsonStringEnumConverter).
export enum SellerVerificationStatus {
  PendingApproval = 0,
  UnderReview = 1,
  Approved = 2,
  Rejected = 3,
  Suspended = 4,
}

export enum DocumentVerificationStatus {
  Pending = 0,
  Verified = 1,
  Rejected = 2,
}

export const SELLER_STATUS_LABELS: Record<SellerVerificationStatus, string> = {
  [SellerVerificationStatus.PendingApproval]: 'Pending Approval',
  [SellerVerificationStatus.UnderReview]: 'Under Review',
  [SellerVerificationStatus.Approved]: 'Approved',
  [SellerVerificationStatus.Rejected]: 'Rejected',
  [SellerVerificationStatus.Suspended]: 'Suspended',
};

export const DOCUMENT_STATUS_LABELS: Record<DocumentVerificationStatus, string> = {
  [DocumentVerificationStatus.Pending]: 'Pending Review',
  [DocumentVerificationStatus.Verified]: 'Verified',
  [DocumentVerificationStatus.Rejected]: 'Rejected',
};

// Mirrors SellerDocumentDto.
export interface SellerDocument {
  id: string;
  documentType: string;
  fileUrl: string;
  fileName: string | null;
  status: DocumentVerificationStatus;
  reviewerNote: string | null;
  createdAtUtc: string;
}

// Mirrors SellerDto.
export interface SellerProfile {
  id: string;
  userId: string;
  businessName: string;
  businessDescription: string | null;
  logoUrl: string | null;
  gstNumber: string;
  city: string;
  state: string;
  verificationStatus: SellerVerificationStatus;
  rejectionReason: string | null;
  totalRevenue: number;
  totalOrdersFulfilled: number;
  averageRating: number;
  reviewCount: number;
  documents: SellerDocument[];
}

// Mirrors SubmitSellerDocumentCommand.
export interface SubmitDocumentRequest {
  documentType: string;
  fileUrl: string;
  fileName?: string | null;
}

// Mirrors CreateProductGemstoneRequest.
export interface ProductGemstoneRequest {
  gemstoneType: string;
  weightCarats: number;
  clarityGrade?: string | null;
  colorGrade?: string | null;
  cutGrade?: string | null;
  quantity: number;
  value: number;
}

// Mirrors CreateProductImageRequest.
export interface ProductImageRequest {
  url: string;
  altText?: string | null;
  displayOrder: number;
  isPrimary: boolean;
}

// Mirrors CreateProductCommand.
export interface CreateProductRequest {
  categoryId: string;
  name: string;
  sku: string;
  description?: string | null;
  productType: number;
  metalType: number;
  purity: number;
  grossWeightGrams: number;
  netWeightGrams: number;
  metalRatePerGramAtListing: number;
  size?: string | null;
  sizeUnit?: string | null;
  makingCharges: number;
  makingChargesArePercentage: boolean;
  wastageCharges: number;
  discountPercentage?: number | null;
  isHallmarked: boolean;
  hallmarkUniqueId?: string | null;
  certificationAuthority?: string | null;
  initialQuantity: number;
  gemstones: ProductGemstoneRequest[];
  images: ProductImageRequest[];
}

// Mirrors UpdateProductRequestBody.
export interface UpdateProductRequest {
  name: string;
  description?: string | null;
  grossWeightGrams: number;
  netWeightGrams: number;
  metalRatePerGramAtListing: number;
  size?: string | null;
  sizeUnit?: string | null;
  makingCharges: number;
  makingChargesArePercentage: boolean;
  wastageCharges: number;
  discountPercentage?: number | null;
}

// Mirrors SellerOrderItemDto.
export interface SellerOrderItem {
  orderItemId: string;
  orderId: string;
  orderNumber: string;
  productNameSnapshot: string;
  unitPriceSnapshot: number;
  quantity: number;
  lineTotal: number;
  itemStatus: OrderStatus;
  orderCreatedAtUtc: string;
  shipment: {
    status: ShipmentStatus;
    carrier: string | null;
    trackingNumber: string | null;
    trackingUrl: string | null;
    shippedAtUtc: string | null;
    deliveredAtUtc: string | null;
  } | null;
}

// Mirrors UpdateOrderItemStatusRequestBody. NewStatus is always OrderStatus.Processing,
// Shipped, or Delivered here — a seller only ever advances an item forward.
export interface UpdateItemStatusRequest {
  newStatus: OrderStatus;
  carrier?: string | null;
  trackingNumber?: string | null;
  trackingUrl?: string | null;
}
