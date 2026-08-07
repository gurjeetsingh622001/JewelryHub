import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { OFFICER_ROLES, POLL_STATUS_LABELS, PollStatus, UnionMembershipStatus } from '../models';
import { PollsService } from '../polls.service';
import { UnionsService } from '../unions.service';

@Component({
  selector: 'app-poll-detail',
  imports: [RouterLink, FormsModule, MatButtonModule, MatCheckboxModule, MatRadioModule, MatProgressSpinnerModule, LucideAngularModule, DatePipe],
  templateUrl: './poll-detail.component.html',
  styleUrl: './poll-detail.component.scss',
})
export class PollDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly pollsService = inject(PollsService);
  private readonly unionsService = inject(UnionsService);
  protected readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly pollStatusLabels = POLL_STATUS_LABELS;
  protected readonly PollStatus = PollStatus;

  protected readonly pollId = toSignal(this.route.paramMap.pipe(map((params) => params.get('pollId')!)), { requireSync: true });

  protected readonly pollResource = rxResource({
    params: this.pollId,
    stream: ({ params }) => this.pollsService.getById(params),
  });

  private readonly membershipsParam = computed(() => (this.auth.hasRole('Seller') ? true : undefined));

  protected readonly membershipsResource = rxResource({
    params: this.membershipsParam,
    stream: () => this.unionsService.getMyMemberships(),
  });

  protected readonly myMembership = computed(() =>
    this.membershipsResource.value()?.find((m) => m.unionId === this.pollResource.value()?.unionId),
  );

  protected readonly isOfficer = computed(() => {
    const m = this.myMembership();
    return !!m && m.status === UnionMembershipStatus.Active && OFFICER_ROLES.has(m.role);
  });

  protected readonly isActiveMember = computed(() => this.myMembership()?.status === UnionMembershipStatus.Active);

  // No per-member "hasVoted" flag exists on PollDto — this just hides the
  // ballot locally after a successful submit for this page session; a
  // reload will show it again, and revoting correctly surfaces the
  // backend's real "already voted" error rather than failing silently.
  protected readonly hasVotedThisSession = signal(false);
  protected readonly selectedOptionIds = signal<string[]>([]);
  protected readonly voting = signal(false);

  toggleOption(optionId: string, allowMultiple: boolean): void {
    this.selectedOptionIds.update((ids) => {
      if (allowMultiple) {
        return ids.includes(optionId) ? ids.filter((id) => id !== optionId) : [...ids, optionId];
      }
      return [optionId];
    });
  }

  submitVote(): void {
    const optionIds = this.selectedOptionIds();
    if (optionIds.length === 0) return;

    this.voting.set(true);
    this.pollsService.vote(this.pollId(), optionIds).subscribe({
      next: () => {
        this.voting.set(false);
        this.hasVotedThisSession.set(true);
        this.snackBar.open('Vote submitted.', 'Dismiss', { duration: 4000 });
        this.pollResource.reload();
      },
      error: () => this.voting.set(false),
    });
  }

  openPoll(): void {
    this.pollsService.open(this.pollId()).subscribe({ next: () => this.pollResource.reload() });
  }

  closePoll(): void {
    this.pollsService.close(this.pollId()).subscribe({ next: () => this.pollResource.reload() });
  }
}
