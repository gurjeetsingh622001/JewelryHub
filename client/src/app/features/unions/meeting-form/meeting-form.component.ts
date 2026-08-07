import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { MeetingsService } from '../meetings.service';

@Component({
  selector: 'app-meeting-form',
  imports: [RouterLink, FormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, LucideAngularModule],
  templateUrl: './meeting-form.component.html',
  styleUrl: './meeting-form.component.scss',
})
export class MeetingFormComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly meetingsService = inject(MeetingsService);

  protected readonly unionId = toSignal(this.route.paramMap.pipe(map((params) => params.get('unionId')!)), { requireSync: true });

  protected readonly submitting = signal(false);
  protected readonly title = signal('');
  protected readonly location = signal('');
  protected readonly virtualMeetingLink = signal('');
  protected readonly scheduledAt = signal('');
  protected readonly durationMinutes = signal(60);
  protected readonly agendaTopics = signal<string[]>(['']);

  addAgendaTopic(): void {
    this.agendaTopics.update((topics) => [...topics, '']);
  }

  removeAgendaTopic(index: number): void {
    this.agendaTopics.update((topics) => topics.filter((_, i) => i !== index));
  }

  updateAgendaTopic(index: number, value: string): void {
    this.agendaTopics.update((topics) => topics.map((t, i) => (i === index ? value : t)));
  }

  submit(): void {
    const title = this.title().trim();
    const scheduledAt = this.scheduledAt();
    if (!title || !scheduledAt) return;

    this.submitting.set(true);
    this.meetingsService
      .create({
        unionId: this.unionId(),
        title,
        location: this.location().trim() || null,
        virtualMeetingLink: this.virtualMeetingLink().trim() || null,
        scheduledAtUtc: new Date(scheduledAt).toISOString(),
        durationMinutes: this.durationMinutes(),
        agendaTopics: this.agendaTopics().map((t) => t.trim()).filter((t) => t.length > 0),
      })
      .subscribe({
        next: (meeting) => {
          this.submitting.set(false);
          this.router.navigate(['/unions', this.unionId(), 'meetings', meeting.id]);
        },
        error: () => this.submitting.set(false),
      });
  }
}
