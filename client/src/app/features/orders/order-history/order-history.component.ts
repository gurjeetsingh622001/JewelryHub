import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { ORDER_STATUS_LABELS } from '../models';
import { OrdersService } from '../orders.service';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-order-history',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, LucideAngularModule, CurrencyPipe, DatePipe],
  templateUrl: './order-history.component.html',
  styleUrl: './order-history.component.scss',
})
export class OrderHistoryComponent {
  private readonly ordersService = inject(OrdersService);

  protected readonly orderStatusLabels = ORDER_STATUS_LABELS;
  protected readonly pageNumber = signal(1);

  protected readonly ordersResource = rxResource({
    params: this.pageNumber,
    stream: ({ params }) => this.ordersService.getMyOrders(params, PAGE_SIZE),
  });

  goToPage(page: number): void {
    this.pageNumber.set(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
