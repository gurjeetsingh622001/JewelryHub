// Mirrors JewelryHub.Domain.Enums.CoreEnums — the backend has no
// JsonStringEnumConverter registered, so System.Text.Json's default applies:
// enums serialize as their numeric ordinal, not a string. These numeric
// values must stay in exact sync with the C# enum definitions.
export enum MetalType {
  Gold = 0,
  Silver = 1,
  Platinum = 2,
  Palladium = 3,
  Other = 4,
}

export enum PurityType {
  K9 = 0,
  K14 = 1,
  K18 = 2,
  K20 = 3,
  K22 = 4,
  K24 = 5,
  Silver925 = 6,
  Silver999 = 7,
  Platinum950 = 8,
  Platinum900 = 9,
}

export enum ProductStatus {
  Draft = 0,
  PendingReview = 1,
  Active = 2,
  OutOfStock = 3,
  Rejected = 4,
  Archived = 5,
}

/** High-level jewelry category grouping, separate from the seller-managed Category tree. */
export enum ProductType {
  Ring = 0,
  Necklace = 1,
  Earring = 2,
  Bracelet = 3,
  Bangle = 4,
  Pendant = 5,
  Chain = 6,
  Anklet = 7,
  Nosepin = 8,
  Coin = 9,
  Other = 10,
}

export const PRODUCT_TYPE_LABELS: Record<ProductType, string> = {
  [ProductType.Ring]: 'Ring',
  [ProductType.Necklace]: 'Necklace',
  [ProductType.Earring]: 'Earring',
  [ProductType.Bracelet]: 'Bracelet',
  [ProductType.Bangle]: 'Bangle',
  [ProductType.Pendant]: 'Pendant',
  [ProductType.Chain]: 'Chain',
  [ProductType.Anklet]: 'Anklet',
  [ProductType.Nosepin]: 'Nosepin',
  [ProductType.Coin]: 'Coin',
  [ProductType.Other]: 'Other',
};

// Mirrors GetProductsQuery's ProductSortOption.
export enum ProductSortOption {
  Newest = 0,
  PriceLowToHigh = 1,
  PriceHighToLow = 2,
  RatingDesc = 3,
}

export const METAL_TYPE_LABELS: Record<MetalType, string> = {
  [MetalType.Gold]: 'Gold',
  [MetalType.Silver]: 'Silver',
  [MetalType.Platinum]: 'Platinum',
  [MetalType.Palladium]: 'Palladium',
  [MetalType.Other]: 'Other',
};

export const PURITY_TYPE_LABELS: Record<PurityType, string> = {
  [PurityType.K9]: '9K Gold',
  [PurityType.K14]: '14K Gold',
  [PurityType.K18]: '18K Gold',
  [PurityType.K20]: '20K Gold',
  [PurityType.K22]: '22K Gold',
  [PurityType.K24]: '24K Gold',
  [PurityType.Silver925]: 'Sterling Silver (925)',
  [PurityType.Silver999]: 'Fine Silver (999)',
  [PurityType.Platinum950]: 'Platinum 950',
  [PurityType.Platinum900]: 'Platinum 900',
};

// Mirrors ProductListItemDto.
export interface ProductListItem {
  id: string;
  name: string;
  slug: string;
  metalType: MetalType;
  purity: PurityType;
  basePrice: number;
  discountPercentage: number | null;
  primaryImageUrl: string | null;
  averageRating: number;
  reviewCount: number;
  status: ProductStatus;
  sellerId: string;
  sellerBusinessName: string;
  categoryName: string;
  inStock: boolean;
}

export interface ProductImage {
  id: string;
  url: string;
  altText: string | null;
  displayOrder: number;
  isPrimary: boolean;
}

export interface ProductGemstone {
  id: string;
  gemstoneType: string;
  weightCarats: number;
  clarityGrade: string | null;
  colorGrade: string | null;
  cutGrade: string | null;
  quantity: number;
  value: number;
}

export interface ProductCertificate {
  id: string;
  certificateType: string;
  issuingAuthority: string;
  certificateNumber: string | null;
  fileUrl: string;
}

// Mirrors ProductDto.
export interface Product {
  id: string;
  name: string;
  slug: string;
  sku: string;
  description: string | null;
  productType: number;
  status: ProductStatus;
  metalType: MetalType;
  purity: PurityType;
  grossWeightGrams: number;
  netWeightGrams: number;
  metalRatePerGramAtListing: number;
  size: string | null;
  sizeUnit: string | null;
  metalValue: number;
  makingCharges: number;
  makingChargesArePercentage: boolean;
  gemstoneValue: number;
  wastageCharges: number;
  basePrice: number;
  discountPercentage: number | null;
  isHallmarked: boolean;
  hallmarkUniqueId: string | null;
  certificationAuthority: string | null;
  averageRating: number;
  reviewCount: number;
  sellerId: string;
  sellerBusinessName: string;
  categoryId: string;
  categoryName: string;
  quantityAvailable: number;
  images: ProductImage[];
  gemstones: ProductGemstone[];
  certificates: ProductCertificate[];
}

// Mirrors CategoryDto.
export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  iconUrl: string | null;
  parentCategoryId: string | null;
  displayOrder: number;
  isActive: boolean;
}

// Mirrors GetProductsQuery's query-string parameters.
export interface ProductFilters {
  categoryId?: string;
  sellerId?: string;
  metalType?: MetalType;
  purity?: PurityType;
  minPrice?: number;
  maxPrice?: number;
  search?: string;
  sort?: ProductSortOption;
  pageNumber?: number;
  pageSize?: number;
}

/** The price actually charged, after DiscountPercentage — BasePrice is pre-discount. */
export function effectivePrice(basePrice: number, discountPercentage: number | null): number {
  if (!discountPercentage) return basePrice;
  return basePrice - basePrice * (discountPercentage / 100);
}
