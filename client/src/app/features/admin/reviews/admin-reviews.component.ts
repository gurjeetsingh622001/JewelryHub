import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { ReviewsService } from '../../reviews/reviews.service';
import { AdminService } from '../admin.service';
import { AdminReviewFilters } from '../models';

type ModerationFilter = 'all' | 'flagged' | 'hidden';

@Component({
  selector: 'app-admin-reviews',
  imports: [MatButtonModule, MatButtonToggleModule, MatProgressSpinnerModule, LucideAngularModule, DatePipe],
  templateUrl: './admin-reviews.component.html',
  styleUrl: './admin-reviews.component.scss',
})
export class AdminReviewsComponent {
  private readonly adminService = inject(AdminService);
  private readonly reviewsService = inject(ReviewsService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly activeFilter = signal<ModerationFilter>('all');

  private readonly filters = computed<AdminReviewFilters>(() => {
    const filter = this.activeFilter();
    return {
      isFlagged: filter === 'flagged' ? true : undefined,
      isApproved: filter === 'hidden' ? false : undefined,
      pageNumber: 1,
      pageSize: 50,
    };
  });

  protected readonly reviewsResource = rxResource({
    params: this.filters,
    stream: ({ params }) => this.adminService.getReviews(params),
  });

  moderate(reviewId: string, isApproved: boolean): void {
    this.reviewsService.moderate(reviewId, isApproved).subscribe({
      next: () => {
        this.snackBar.open(isApproved ? 'Review approved.' : 'Review hidden.', 'Dismiss', { duration: 4000 });
        this.reviewsResource.reload();
      },
    });
  }
}
