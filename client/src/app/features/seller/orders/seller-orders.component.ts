import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { ORDER_STATUS_LABELS, OrderStatus } from '../../orders/models';
import { SellerService } from '../seller.service';

/** A seller only ever advances an item forward through this subset of the full OrderStatus lifecycle. */
const NEXT_STATUS: Partial<Record<OrderStatus, OrderStatus>> = {
  [OrderStatus.Confirmed]: OrderStatus.Processing,
  [OrderStatus.Processing]: OrderStatus.Shipped,
  [OrderStatus.Shipped]: OrderStatus.Delivered,
};

@Component({
  selector: 'app-seller-orders',
  imports: [
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    LucideAngularModule,
    CurrencyPipe,
    DatePipe,
  ],
  templateUrl: './seller-orders.component.html',
  styleUrl: './seller-orders.component.scss',
})
export class SellerOrdersComponent {
  private readonly sellerService = inject(SellerService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly orderStatusLabels = ORDER_STATUS_LABELS;
  protected readonly OrderStatus = OrderStatus;
  protected readonly nextStatus = NEXT_STATUS;

  protected readonly statusFilter = signal<OrderStatus | undefined>(undefined);
  protected readonly updatingItemId = signal<string | null>(null);

  // Marking an item Shipped requires Carrier + Tracking Number (enforced
  // server-side) — this tracks which row's inline shipment-details form is
  // currently open, plus its two field values.
  protected readonly shippingItemId = signal<string | null>(null);
  protected readonly carrierInput = signal('');
  protected readonly trackingNumberInput = signal('');

  protected readonly statusFilterOptions = [
    { value: undefined, label: 'All' },
    { value: OrderStatus.Confirmed, label: 'Confirmed' },
    { value: OrderStatus.Processing, label: 'Processing' },
    { value: OrderStatus.Shipped, label: 'Shipped' },
    { value: OrderStatus.Delivered, label: 'Delivered' },
  ];

  // Wrapped in an object because rxResource treats a bare `undefined` params
  // value as "no request yet" and skips the loader — but undefined *is* a
  // valid, meaningful filter value here (it means "All statuses").
  protected readonly queueResource = rxResource({
    params: () => ({ status: this.statusFilter() }),
    stream: ({ params }) => this.sellerService.getOrderQueue(params.status, 1, 100),
  });

  advanceStatus(orderItemId: string, currentStatus: OrderStatus): void {
    const newStatus = this.nextStatus[currentStatus];
    if (newStatus === undefined) return;

    if (newStatus === OrderStatus.Shipped) {
      this.shippingItemId.set(orderItemId);
      this.carrierInput.set('');
      this.trackingNumberInput.set('');
      return;
    }

    this.runUpdate(orderItemId, { newStatus });
  }

  confirmShipment(orderItemId: string): void {
    const carrier = this.carrierInput().trim();
    const trackingNumber = this.trackingNumberInput().trim();
    if (!carrier || !trackingNumber) return;

    this.runUpdate(orderItemId, { newStatus: OrderStatus.Shipped, carrier, trackingNumber }, () => this.shippingItemId.set(null));
  }

  cancelShipmentForm(): void {
    this.shippingItemId.set(null);
  }

  private runUpdate(orderItemId: string, request: Parameters<SellerService['updateItemStatus']>[1], onSuccess?: () => void): void {
    this.updatingItemId.set(orderItemId);
    this.sellerService.updateItemStatus(orderItemId, request).subscribe({
      next: () => {
        this.updatingItemId.set(null);
        onSuccess?.();
        this.snackBar.open('Order item updated.', 'Dismiss', { duration: 4000 });
        this.queueResource.reload();
      },
      error: () => this.updatingItemId.set(null),
    });
  }

  actionLabel(status: OrderStatus): string {
    const next = this.nextStatus[status];
    if (next === OrderStatus.Processing) return 'Start Processing';
    if (next === OrderStatus.Shipped) return 'Mark Shipped';
    if (next === OrderStatus.Delivered) return 'Mark Delivered';
    return '';
  }
}
