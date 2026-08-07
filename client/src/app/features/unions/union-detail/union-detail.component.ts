import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { MeetingsService } from '../meetings.service';
import {
  MEETING_STATUS_LABELS,
  OFFICER_ROLES,
  POLL_STATUS_LABELS,
  UNION_EVENT_STATUS_LABELS,
  UNION_MEMBER_ROLE_LABELS,
  UNION_MEMBERSHIP_STATUS_LABELS,
  UnionEventStatus,
  UnionMembershipStatus,
} from '../models';
import { PollsService } from '../polls.service';
import { UnionsService } from '../unions.service';

@Component({
  selector: 'app-union-detail',
  imports: [
    RouterLink,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    LucideAngularModule,
    DatePipe,
  ],
  templateUrl: './union-detail.component.html',
  styleUrl: './union-detail.component.scss',
})
export class UnionDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly unionsService = inject(UnionsService);
  private readonly meetingsService = inject(MeetingsService);
  private readonly pollsService = inject(PollsService);
  protected readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly roleLabels = UNION_MEMBER_ROLE_LABELS;
  protected readonly membershipStatusLabels = UNION_MEMBERSHIP_STATUS_LABELS;
  protected readonly eventStatusLabels = UNION_EVENT_STATUS_LABELS;
  protected readonly meetingStatusLabels = MEETING_STATUS_LABELS;
  protected readonly pollStatusLabels = POLL_STATUS_LABELS;
  protected readonly UnionEventStatus = UnionEventStatus;

  protected readonly unionId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id')!)), { requireSync: true });

  protected readonly joining = signal(false);
  protected readonly showAnnouncementForm = signal(false);
  protected readonly showDocumentForm = signal(false);
  protected readonly showEventForm = signal(false);

  protected readonly announcementTitle = signal('');
  protected readonly announcementBody = signal('');
  protected readonly documentTitle = signal('');
  protected readonly documentUrl = signal('');
  protected readonly documentCategory = signal('');
  protected readonly eventTitle = signal('');
  protected readonly eventDescription = signal('');
  protected readonly eventLocation = signal('');
  protected readonly eventStartsAt = signal('');

  protected readonly unionResource = rxResource({
    params: this.unionId,
    stream: ({ params }) => this.unionsService.getById(params),
  });

  // GetMyMembershipsQuery is Seller-only — skip the call entirely for
  // everyone else rather than let it 403.
  private readonly membershipsParam = computed(() => (this.auth.hasRole('Seller') ? true : undefined));

  protected readonly membershipsResource = rxResource({
    params: this.membershipsParam,
    stream: () => this.unionsService.getMyMemberships(),
  });

  protected readonly myMembership = computed(() => this.membershipsResource.value()?.find((m) => m.unionId === this.unionId()));

  protected readonly isActiveMember = computed(() => this.myMembership()?.status === UnionMembershipStatus.Active);
  protected readonly isPendingMember = computed(() => this.myMembership()?.status === UnionMembershipStatus.PendingApproval);
  protected readonly isOfficer = computed(() => {
    const m = this.myMembership();
    return !!m && m.status === UnionMembershipStatus.Active && OFFICER_ROLES.has(m.role);
  });
  protected readonly canJoin = computed(() => this.auth.hasRole('Seller') && !this.myMembership());

  // Members/Announcements/Documents are [Authorize] on the backend — any
  // logged-in user, not anonymous. Only Events is [AllowAnonymous]. Gating
  // these on isAuthenticated() avoids firing requests that are guaranteed
  // to 401 for a signed-out visitor (which otherwise also triggers a
  // spurious refresh-token attempt for a session that never existed).
  private readonly authedUnionId = computed(() => (this.auth.isAuthenticated() ? this.unionId() : undefined));

  protected readonly membersResource = rxResource({
    params: this.authedUnionId,
    stream: ({ params }) => this.unionsService.getMembers(params, 1, 50),
  });

  // Officer-only endpoint (self-enforced server-side) — only fetched once
  // we actually know the caller is an officer, to avoid a noisy 403.
  private readonly pendingMembersParam = computed(() => (this.isOfficer() ? this.unionId() : undefined));

  protected readonly pendingMembersResource = rxResource({
    params: this.pendingMembersParam,
    stream: ({ params }) => this.unionsService.getPendingMembers(params, 1, 50),
  });

  protected readonly announcementsResource = rxResource({
    params: this.authedUnionId,
    stream: ({ params }) => this.unionsService.getAnnouncements(params, 1, 20),
  });

  protected readonly documentsResource = rxResource({
    params: this.authedUnionId,
    stream: ({ params }) => this.unionsService.getDocuments(params, 1, 20),
  });

  protected readonly eventsResource = rxResource({
    params: this.unionId,
    stream: ({ params }) => this.unionsService.getEvents(params, 1, 20),
  });

  // GetMeetings/GetPolls require an active membership (or Admin) server-side
  // — gating on that here avoids a guaranteed BusinessRuleException for a
  // logged-in visitor who isn't a member of this particular union.
  private readonly activeMemberUnionId = computed(() =>
    this.isActiveMember() || this.auth.hasRole('Admin') ? this.unionId() : undefined,
  );

  protected readonly meetingsResource = rxResource({
    params: this.activeMemberUnionId,
    stream: ({ params }) => this.meetingsService.getMeetings(params, 1, 20),
  });

  protected readonly pollsResource = rxResource({
    params: this.activeMemberUnionId,
    stream: ({ params }) => this.pollsService.getPolls(params, 1, 20),
  });

  join(): void {
    this.joining.set(true);
    this.unionsService.join(this.unionId()).subscribe({
      next: () => {
        this.joining.set(false);
        this.snackBar.open('Membership requested — an officer will review it.', 'Dismiss', { duration: 5000 });
        this.membershipsResource.reload();
      },
      error: () => this.joining.set(false),
    });
  }

  reviewMembership(membershipId: string, approve: boolean): void {
    this.unionsService.reviewMembership(membershipId, approve).subscribe({
      next: () => {
        this.snackBar.open(approve ? 'Member approved.' : 'Request rejected.', 'Dismiss', { duration: 4000 });
        this.pendingMembersResource.reload();
        this.membersResource.reload();
      },
    });
  }

  submitAnnouncement(): void {
    const title = this.announcementTitle().trim();
    const body = this.announcementBody().trim();
    if (!title || !body) return;

    this.unionsService.createAnnouncement(this.unionId(), { title, body, isPinned: false }).subscribe({
      next: () => {
        this.announcementTitle.set('');
        this.announcementBody.set('');
        this.showAnnouncementForm.set(false);
        this.announcementsResource.reload();
      },
    });
  }

  deleteAnnouncement(announcementId: string): void {
    this.unionsService.deleteAnnouncement(announcementId).subscribe({
      next: () => this.announcementsResource.reload(),
    });
  }

  submitDocument(): void {
    const title = this.documentTitle().trim();
    const fileUrl = this.documentUrl().trim();
    if (!title || !fileUrl) return;

    this.unionsService.uploadDocument(this.unionId(), { title, fileUrl, category: this.documentCategory().trim() || null }).subscribe({
      next: () => {
        this.documentTitle.set('');
        this.documentUrl.set('');
        this.documentCategory.set('');
        this.showDocumentForm.set(false);
        this.documentsResource.reload();
      },
    });
  }

  submitEvent(): void {
    const title = this.eventTitle().trim();
    const startsAt = this.eventStartsAt();
    if (!title || !startsAt) return;

    this.unionsService
      .createEvent(this.unionId(), {
        title,
        description: this.eventDescription().trim() || null,
        location: this.eventLocation().trim() || null,
        startsAtUtc: new Date(startsAt).toISOString(),
      })
      .subscribe({
        next: () => {
          this.eventTitle.set('');
          this.eventDescription.set('');
          this.eventLocation.set('');
          this.eventStartsAt.set('');
          this.showEventForm.set(false);
          this.eventsResource.reload();
        },
      });
  }

  cancelEvent(eventId: string): void {
    this.unionsService.updateEventStatus(eventId, UnionEventStatus.Cancelled).subscribe({
      next: () => this.eventsResource.reload(),
    });
  }
}
