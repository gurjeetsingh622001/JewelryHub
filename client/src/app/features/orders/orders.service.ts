import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../../core/models/paged-result';
import { CheckoutRequest, Order, OrderSummary } from './models';

@Injectable({ providedIn: 'root' })
export class OrdersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/orders`;

  checkout(request: CheckoutRequest): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, request);
  }

  getById(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }

  getMyOrders(pageNumber = 1, pageSize = 20): Observable<PagedResult<OrderSummary>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<OrderSummary>>(`${this.baseUrl}/me`, { params });
  }

  /**
   * Stands in for a real payment gateway webhook (see ConfirmPaymentCommand's
   * remarks on the backend) — Checkout calls this immediately after placing
   * an order for any "online" payment method, since there's no real gateway
   * to redirect to yet. Cash on Delivery skips this entirely; that payment
   * is collected on delivery, not confirmed up front.
   */
  confirmPayment(orderId: string, paymentId: string, gatewayTransactionId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${orderId}/payments/${paymentId}/confirm`, { gatewayTransactionId });
  }

  cancel(orderId: string, reason?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${orderId}/cancel`, { reason });
  }
}
