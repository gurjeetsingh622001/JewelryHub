// Mirrors JewelryHub.Domain.Enums.UnionEnums — numeric on the wire, see
// features/products/models.ts for why (no JsonStringEnumConverter).
export enum UnionMemberRole {
  Member = 0,
  Treasurer = 1,
  Secretary = 2,
  VicePresident = 3,
  President = 4,
}

export enum UnionMembershipStatus {
  PendingApproval = 0,
  Active = 1,
  Inactive = 2,
  Removed = 3,
}

export enum UnionEventStatus {
  Upcoming = 0,
  Ongoing = 1,
  Completed = 2,
  Cancelled = 3,
}

export const UNION_MEMBER_ROLE_LABELS: Record<UnionMemberRole, string> = {
  [UnionMemberRole.Member]: 'Member',
  [UnionMemberRole.Treasurer]: 'Treasurer',
  [UnionMemberRole.Secretary]: 'Secretary',
  [UnionMemberRole.VicePresident]: 'Vice President',
  [UnionMemberRole.President]: 'President',
};

export const UNION_MEMBERSHIP_STATUS_LABELS: Record<UnionMembershipStatus, string> = {
  [UnionMembershipStatus.PendingApproval]: 'Pending Approval',
  [UnionMembershipStatus.Active]: 'Active',
  [UnionMembershipStatus.Inactive]: 'Inactive',
  [UnionMembershipStatus.Removed]: 'Removed',
};

export const UNION_EVENT_STATUS_LABELS: Record<UnionEventStatus, string> = {
  [UnionEventStatus.Upcoming]: 'Upcoming',
  [UnionEventStatus.Ongoing]: 'Ongoing',
  [UnionEventStatus.Completed]: 'Completed',
  [UnionEventStatus.Cancelled]: 'Cancelled',
};

/** An officer (President/VicePresident/Secretary) can perform governance actions — mirrors UnionAuthorization.OfficerRoles. */
export const OFFICER_ROLES: ReadonlySet<UnionMemberRole> = new Set([
  UnionMemberRole.President,
  UnionMemberRole.VicePresident,
  UnionMemberRole.Secretary,
]);

// Mirrors UnionDto.
export interface Union {
  id: string;
  name: string;
  description: string | null;
  logoUrl: string | null;
  city: string;
  state: string;
  createdBySellerId: string;
  createdBySellerBusinessName: string;
  isApprovedByAdmin: boolean;
  approvedAtUtc: string | null;
  annualMembershipFee: number | null;
  activeMemberCount: number;
  createdAtUtc: string;
}

// Mirrors UnionMemberDto.
export interface UnionMember {
  id: string;
  unionId: string;
  sellerId: string;
  sellerBusinessName: string;
  role: UnionMemberRole;
  status: UnionMembershipStatus;
  joinedAtUtc: string;
  membershipFeePaidThroughUtc: string | null;
}

// Mirrors UnionAnnouncementDto.
export interface UnionAnnouncement {
  id: string;
  unionId: string;
  title: string;
  body: string;
  isPinned: boolean;
  publishedByMemberId: string;
  publishedByBusinessName: string;
  createdAtUtc: string;
}

// Mirrors UnionDocumentDto.
export interface UnionDocument {
  id: string;
  unionId: string;
  title: string;
  fileUrl: string;
  category: string | null;
  uploadedByMemberId: string;
  uploadedByBusinessName: string;
  createdAtUtc: string;
}

// Mirrors UnionEventDto.
export interface UnionEvent {
  id: string;
  unionId: string;
  title: string;
  description: string | null;
  location: string | null;
  startsAtUtc: string;
  endsAtUtc: string | null;
  status: UnionEventStatus;
}

// Mirrors CreateUnionCommand.
export interface CreateUnionRequest {
  name: string;
  description?: string | null;
  logoUrl?: string | null;
  city: string;
  state: string;
  annualMembershipFee?: number | null;
}

// Mirrors CreateAnnouncementRequestBody.
export interface CreateAnnouncementRequest {
  title: string;
  body: string;
  isPinned: boolean;
}

// Mirrors UploadDocumentRequestBody.
export interface UploadDocumentRequest {
  title: string;
  fileUrl: string;
  category?: string | null;
}

// Mirrors CreateEventRequestBody.
export interface CreateEventRequest {
  title: string;
  description?: string | null;
  location?: string | null;
  startsAtUtc: string;
  endsAtUtc?: string | null;
}

// Mirrors GetUnionsQuery's query-string parameters.
export interface UnionFilters {
  search?: string;
  city?: string;
  state?: string;
  pageNumber?: number;
  pageSize?: number;
}
