import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { ReviewsService } from '../../reviews/reviews.service';
import { ORDER_STATUS_LABELS, OrderStatus, PAYMENT_METHOD_LABELS } from '../models';
import { OrdersService } from '../orders.service';

const CANCELLABLE_STATUSES = [OrderStatus.PendingPayment, OrderStatus.Confirmed, OrderStatus.Processing];
const STARS = [1, 2, 3, 4, 5];

@Component({
  selector: 'app-order-detail',
  imports: [
    RouterLink,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    LucideAngularModule,
    CurrencyPipe,
    DatePipe,
  ],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.scss',
})
export class OrderDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly ordersService = inject(OrdersService);
  private readonly reviewsService = inject(ReviewsService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly orderStatusLabels = ORDER_STATUS_LABELS;
  protected readonly paymentMethodLabels = PAYMENT_METHOD_LABELS;
  protected readonly OrderStatus = OrderStatus;
  protected readonly stars = STARS;

  protected readonly justPlaced = toSignal(this.route.queryParamMap.pipe(map((params) => params.get('placed') === 'true')), {
    requireSync: true,
  });

  private readonly orderId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id')!)), { requireSync: true });

  protected readonly orderResource = rxResource({
    params: this.orderId,
    stream: ({ params }) => this.ordersService.getById(params),
  });

  // A delivered OrderItem carries no "already reviewed" flag from the API,
  // so once a review is submitted successfully this session we just hide
  // that item's form/button locally rather than re-querying for it.
  protected readonly reviewedItemIds = signal<ReadonlySet<string>>(new Set());
  protected readonly reviewingItemId = signal<string | null>(null);
  protected readonly submittingReview = signal(false);
  protected readonly productRating = signal(5);
  protected readonly sellerRating = signal(5);
  protected readonly reviewTitle = signal('');
  protected readonly reviewComment = signal('');

  isCancellable(status: OrderStatus): boolean {
    return CANCELLABLE_STATUSES.includes(status);
  }

  cancelOrder(): void {
    const order = this.orderResource.value();
    if (!order) return;

    this.ordersService.cancel(order.id).subscribe({
      next: () => {
        this.snackBar.open('Order cancelled.', 'Dismiss', { duration: 4000 });
        this.orderResource.reload();
      },
    });
  }

  startReview(orderItemId: string): void {
    this.reviewingItemId.set(orderItemId);
    this.productRating.set(5);
    this.sellerRating.set(5);
    this.reviewTitle.set('');
    this.reviewComment.set('');
  }

  cancelReview(): void {
    this.reviewingItemId.set(null);
  }

  submitReview(orderItemId: string): void {
    this.submittingReview.set(true);
    this.reviewsService
      .create({
        orderItemId,
        productRating: this.productRating(),
        sellerRating: this.sellerRating(),
        title: this.reviewTitle().trim() || null,
        comment: this.reviewComment().trim() || null,
      })
      .subscribe({
        next: () => {
          this.submittingReview.set(false);
          this.reviewingItemId.set(null);
          this.reviewedItemIds.update((ids) => new Set(ids).add(orderItemId));
          this.snackBar.open('Thank you for your review!', 'Dismiss', { duration: 4000 });
        },
        error: () => this.submittingReview.set(false),
      });
  }
}
