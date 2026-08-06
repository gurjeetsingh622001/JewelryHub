import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { ORDER_STATUS_LABELS, OrderStatus, PAYMENT_METHOD_LABELS } from '../models';
import { OrdersService } from '../orders.service';

const CANCELLABLE_STATUSES = [OrderStatus.PendingPayment, OrderStatus.Confirmed, OrderStatus.Processing];

@Component({
  selector: 'app-order-detail',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, LucideAngularModule, CurrencyPipe, DatePipe],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.scss',
})
export class OrderDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly ordersService = inject(OrdersService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly orderStatusLabels = ORDER_STATUS_LABELS;
  protected readonly paymentMethodLabels = PAYMENT_METHOD_LABELS;

  protected readonly justPlaced = toSignal(this.route.queryParamMap.pipe(map((params) => params.get('placed') === 'true')), {
    requireSync: true,
  });

  private readonly orderId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id')!)), { requireSync: true });

  protected readonly orderResource = rxResource({
    params: this.orderId,
    stream: ({ params }) => this.ordersService.getById(params),
  });

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
}
