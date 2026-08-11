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

export enum MeetingStatus {
  Scheduled = 0,
  Ongoing = 1,
  Completed = 2,
  Cancelled = 3,
}

export enum AgendaItemStatus {
  Pending = 0,
  InDiscussion = 1,
  Completed = 2,
  Postponed = 3,
}

export enum MeetingAttendanceStatus {
  Invited = 0,
  Confirmed = 1,
  Attended = 2,
  Absent = 3,
  Excused = 4,
}

export enum ActionItemStatus {
  Open = 0,
  InProgress = 1,
  Completed = 2,
  Overdue = 3,
}

export enum PollStatus {
  Draft = 0,
  Active = 1,
  Closed = 2,
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

export const MEETING_STATUS_LABELS: Record<MeetingStatus, string> = {
  [MeetingStatus.Scheduled]: 'Scheduled',
  [MeetingStatus.Ongoing]: 'Ongoing',
  [MeetingStatus.Completed]: 'Completed',
  [MeetingStatus.Cancelled]: 'Cancelled',
};

export const AGENDA_ITEM_STATUS_LABELS: Record<AgendaItemStatus, string> = {
  [AgendaItemStatus.Pending]: 'Pending',
  [AgendaItemStatus.InDiscussion]: 'In Discussion',
  [AgendaItemStatus.Completed]: 'Completed',
  [AgendaItemStatus.Postponed]: 'Postponed',
};

export const MEETING_ATTENDANCE_STATUS_LABELS: Record<MeetingAttendanceStatus, string> = {
  [MeetingAttendanceStatus.Invited]: 'Invited',
  [MeetingAttendanceStatus.Confirmed]: 'Confirmed',
  [MeetingAttendanceStatus.Attended]: 'Attended',
  [MeetingAttendanceStatus.Absent]: 'Absent',
  [MeetingAttendanceStatus.Excused]: 'Excused',
};

export const ACTION_ITEM_STATUS_LABELS: Record<ActionItemStatus, string> = {
  [ActionItemStatus.Open]: 'Open',
  [ActionItemStatus.InProgress]: 'In Progress',
  [ActionItemStatus.Completed]: 'Completed',
  [ActionItemStatus.Overdue]: 'Overdue',
};

export const POLL_STATUS_LABELS: Record<PollStatus, string> = {
  [PollStatus.Draft]: 'Draft',
  [PollStatus.Active]: 'Active',
  [PollStatus.Closed]: 'Closed',
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
  sellerLogoUrl: string | null;
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

// Mirrors MeetingSummaryDto.
export interface MeetingSummary {
  id: string;
  unionId: string;
  title: string;
  scheduledAtUtc: string;
  durationMinutes: number;
  status: MeetingStatus;
}

// Mirrors MeetingAgendaItemDto.
export interface MeetingAgendaItem {
  id: string;
  displayOrder: number;
  topic: string;
  description: string | null;
  status: AgendaItemStatus;
}

// Mirrors MeetingAttendeeDto.
export interface MeetingAttendee {
  id: string;
  unionMemberId: string;
  sellerBusinessName: string;
  status: MeetingAttendanceStatus;
}

// Mirrors ActionItemDto.
export interface ActionItem {
  id: string;
  description: string;
  responsibleMemberId: string;
  responsibleBusinessName: string;
  dueDateUtc: string | null;
  status: ActionItemStatus;
}

// Mirrors MeetingMinuteDto.
export interface MeetingMinute {
  id: string;
  agendaItemId: string | null;
  decisionSummary: string;
  discussionNotes: string | null;
  recordedByMemberId: string;
  recordedByBusinessName: string;
  createdAtUtc: string;
  actionItems: ActionItem[];
}

// Mirrors MeetingDto.
export interface Meeting {
  id: string;
  unionId: string;
  title: string;
  location: string | null;
  virtualMeetingLink: string | null;
  scheduledAtUtc: string;
  durationMinutes: number;
  status: MeetingStatus;
  createdByMemberId: string;
  createdByBusinessName: string;
  agendaItems: MeetingAgendaItem[];
  attendees: MeetingAttendee[];
  minutes: MeetingMinute[];
}

// Mirrors CreateMeetingCommand.
export interface CreateMeetingRequest {
  unionId: string;
  title: string;
  location?: string | null;
  virtualMeetingLink?: string | null;
  scheduledAtUtc: string;
  durationMinutes: number;
  agendaTopics: string[];
}

// Mirrors ActionItemInput.
export interface ActionItemInputRequest {
  description: string;
  responsibleMemberId: string;
  dueDateUtc?: string | null;
}

// Mirrors RecordMinuteRequestBody.
export interface RecordMinuteRequest {
  agendaItemId?: string | null;
  decisionSummary: string;
  discussionNotes?: string | null;
  actionItems: ActionItemInputRequest[];
}

// Mirrors PollOptionResultDto.
export interface PollOptionResult {
  id: string;
  text: string;
  displayOrder: number;
  voteCount: number;
}

// Mirrors PollSummaryDto.
export interface PollSummary {
  id: string;
  unionId: string;
  question: string;
  status: PollStatus;
  closesAtUtc: string | null;
}

// Mirrors PollDto.
export interface Poll {
  id: string;
  unionId: string;
  question: string;
  allowMultipleSelections: boolean;
  status: PollStatus;
  createdByMemberId: string;
  createdByBusinessName: string;
  closesAtUtc: string | null;
  createdAtUtc: string;
  options: PollOptionResult[];
  totalVotes: number;
}

// Mirrors CreatePollCommand.
export interface CreatePollRequest {
  unionId: string;
  question: string;
  allowMultipleSelections: boolean;
  closesAtUtc?: string | null;
  options: string[];
}
