import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { MeetingsService } from '../meetings.service';
import {
  ACTION_ITEM_STATUS_LABELS,
  AGENDA_ITEM_STATUS_LABELS,
  MEETING_ATTENDANCE_STATUS_LABELS,
  MEETING_STATUS_LABELS,
  MeetingAttendanceStatus,
  MeetingStatus,
  OFFICER_ROLES,
  UnionMembershipStatus,
} from '../models';
import { UnionsService } from '../unions.service';

interface ActionItemDraft {
  description: string;
  responsibleMemberId: string;
  dueDate: string;
}

@Component({
  selector: 'app-meeting-detail',
  imports: [
    RouterLink,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    LucideAngularModule,
    DatePipe,
  ],
  templateUrl: './meeting-detail.component.html',
  styleUrl: './meeting-detail.component.scss',
})
export class MeetingDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly meetingsService = inject(MeetingsService);
  private readonly unionsService = inject(UnionsService);
  protected readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly meetingStatusLabels = MEETING_STATUS_LABELS;
  protected readonly agendaItemStatusLabels = AGENDA_ITEM_STATUS_LABELS;
  protected readonly attendanceStatusLabels = MEETING_ATTENDANCE_STATUS_LABELS;
  protected readonly actionItemStatusLabels = ACTION_ITEM_STATUS_LABELS;
  protected readonly MeetingStatus = MeetingStatus;
  protected readonly MeetingAttendanceStatus = MeetingAttendanceStatus;

  protected readonly meetingId = toSignal(this.route.paramMap.pipe(map((params) => params.get('meetingId')!)), { requireSync: true });

  protected readonly meetingResource = rxResource({
    params: this.meetingId,
    stream: ({ params }) => this.meetingsService.getById(params),
  });

  private readonly membershipsParam = computed(() => (this.auth.hasRole('Seller') ? true : undefined));

  protected readonly membershipsResource = rxResource({
    params: this.membershipsParam,
    stream: () => this.unionsService.getMyMemberships(),
  });

  protected readonly myMembership = computed(() =>
    this.membershipsResource.value()?.find((m) => m.unionId === this.meetingResource.value()?.unionId),
  );

  protected readonly isOfficer = computed(() => {
    const m = this.myMembership();
    return !!m && m.status === UnionMembershipStatus.Active && OFFICER_ROLES.has(m.role);
  });

  protected readonly myAttendee = computed(() => {
    const membership = this.myMembership();
    return this.meetingResource.value()?.attendees.find((a) => a.unionMemberId === membership?.id);
  });

  protected readonly rsvping = signal(false);
  protected readonly showMinuteForm = signal(false);
  protected readonly minuteAgendaItemId = signal<string | null>(null);
  protected readonly minuteDecisionSummary = signal('');
  protected readonly minuteDiscussionNotes = signal('');
  protected readonly minuteActionItems = signal<ActionItemDraft[]>([]);
  protected readonly savingMinute = signal(false);

  rsvp(status: MeetingAttendanceStatus): void {
    this.rsvping.set(true);
    this.meetingsService.rsvp(this.meetingId(), status).subscribe({
      next: () => {
        this.rsvping.set(false);
        this.snackBar.open('RSVP recorded.', 'Dismiss', { duration: 3000 });
        this.meetingResource.reload();
      },
      error: () => this.rsvping.set(false),
    });
  }

  updateStatus(status: MeetingStatus): void {
    this.meetingsService.updateStatus(this.meetingId(), status).subscribe({
      next: () => this.meetingResource.reload(),
    });
  }

  addActionItem(): void {
    this.minuteActionItems.update((items) => [...items, { description: '', responsibleMemberId: '', dueDate: '' }]);
  }

  removeActionItem(index: number): void {
    this.minuteActionItems.update((items) => items.filter((_, i) => i !== index));
  }

  updateActionItem(index: number, patch: Partial<ActionItemDraft>): void {
    this.minuteActionItems.update((items) => items.map((item, i) => (i === index ? { ...item, ...patch } : item)));
  }

  submitMinute(): void {
    const decisionSummary = this.minuteDecisionSummary().trim();
    if (!decisionSummary) return;

    const actionItems = this.minuteActionItems()
      .filter((item) => item.description.trim() && item.responsibleMemberId)
      .map((item) => ({
        description: item.description.trim(),
        responsibleMemberId: item.responsibleMemberId,
        dueDateUtc: item.dueDate ? new Date(item.dueDate).toISOString() : null,
      }));

    this.savingMinute.set(true);
    this.meetingsService
      .recordMinute(this.meetingId(), {
        agendaItemId: this.minuteAgendaItemId(),
        decisionSummary,
        discussionNotes: this.minuteDiscussionNotes().trim() || null,
        actionItems,
      })
      .subscribe({
        next: () => {
          this.savingMinute.set(false);
          this.minuteAgendaItemId.set(null);
          this.minuteDecisionSummary.set('');
          this.minuteDiscussionNotes.set('');
          this.minuteActionItems.set([]);
          this.showMinuteForm.set(false);
          this.meetingResource.reload();
        },
        error: () => this.savingMinute.set(false),
      });
  }
}
