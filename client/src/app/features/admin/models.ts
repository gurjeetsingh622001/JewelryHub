// Mirrors DashboardOverviewDto.
export interface DashboardOverview {
  totalCustomers: number;
  totalSellers: number;
  pendingSellers: number;
  approvedSellers: number;
  totalUnions: number;
  pendingUnions: number;
  approvedUnions: number;
  totalOrders: number;
  ordersThisMonth: number;
  revenueThisMonth: number;
  totalReviews: number;
  flaggedReviews: number;
}

// Mirrors UserSummaryDto.
export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  isActive: boolean;
  isLockedOut: boolean;
  createdAtUtc: string;
  lastLoginAtUtc: string | null;
}

// Mirrors GetUsersQuery's query-string parameters.
export interface UserFilters {
  search?: string;
  role?: string;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

// Mirrors AdminReviewDto.
export interface AdminReview {
  id: string;
  productId: string;
  productName: string;
  sellerId: string;
  sellerBusinessName: string;
  customerFirstName: string;
  productRating: number;
  sellerRating: number;
  title: string | null;
  comment: string | null;
  isApproved: boolean;
  isFlagged: boolean;
  createdAtUtc: string;
}

// Mirrors GetAllReviewsQuery's query-string parameters.
export interface AdminReviewFilters {
  isApproved?: boolean;
  isFlagged?: boolean;
  pageNumber?: number;
  pageSize?: number;
}
