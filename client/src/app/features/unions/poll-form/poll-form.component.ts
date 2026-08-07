import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { PollsService } from '../polls.service';

@Component({
  selector: 'app-poll-form',
  imports: [RouterLink, FormsModule, MatButtonModule, MatCheckboxModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, LucideAngularModule],
  templateUrl: './poll-form.component.html',
  styleUrl: './poll-form.component.scss',
})
export class PollFormComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly pollsService = inject(PollsService);

  protected readonly unionId = toSignal(this.route.paramMap.pipe(map((params) => params.get('unionId')!)), { requireSync: true });

  protected readonly submitting = signal(false);
  protected readonly question = signal('');
  protected readonly allowMultipleSelections = signal(false);
  protected readonly closesAt = signal('');
  protected readonly options = signal<string[]>(['', '']);

  addOption(): void {
    this.options.update((opts) => [...opts, '']);
  }

  removeOption(index: number): void {
    this.options.update((opts) => opts.filter((_, i) => i !== index));
  }

  updateOption(index: number, value: string): void {
    this.options.update((opts) => opts.map((o, i) => (i === index ? value : o)));
  }

  submit(): void {
    const question = this.question().trim();
    const options = this.options().map((o) => o.trim()).filter((o) => o.length > 0);
    if (!question || options.length < 2) return;

    this.submitting.set(true);
    this.pollsService
      .create({
        unionId: this.unionId(),
        question,
        allowMultipleSelections: this.allowMultipleSelections(),
        closesAtUtc: this.closesAt() ? new Date(this.closesAt()).toISOString() : null,
        options,
      })
      .subscribe({
        next: (poll) => {
          this.submitting.set(false);
          this.router.navigate(['/unions', this.unionId(), 'polls', poll.id]);
        },
        error: () => this.submitting.set(false),
      });
  }
}
