import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../../core/models/paged-result';
import { OrderStatus } from '../orders/models';
import { Product } from '../products/models';
import {
  CreateProductRequest,
  DocumentVerificationStatus,
  SellerDocument,
  SellerOrderItem,
  SellerProfile,
  SubmitDocumentRequest,
  UpdateItemStatusRequest,
  UpdateProductRequest,
} from './models';

@Injectable({ providedIn: 'root' })
export class SellerService {
  private readonly http = inject(HttpClient);
  private readonly sellersUrl = `${environment.apiUrl}/sellers`;
  private readonly productsUrl = `${environment.apiUrl}/products`;
  private readonly ordersUrl = `${environment.apiUrl}/orders`;

  getMyProfile(): Observable<SellerProfile> {
    return this.http.get<SellerProfile>(`${this.sellersUrl}/me`);
  }

  submitDocument(request: SubmitDocumentRequest): Observable<SellerDocument> {
    return this.http.post<SellerDocument>(`${this.sellersUrl}/me/documents`, request);
  }

  createProduct(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.productsUrl, request);
  }

  updateProduct(id: string, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.productsUrl}/${id}`, request);
  }

  adjustInventory(id: string, quantityDelta: number, reason?: string): Observable<void> {
    return this.http.post<void>(`${this.productsUrl}/${id}/inventory/adjust`, { quantityDelta, reason });
  }

  getOrderQueue(status?: OrderStatus, pageNumber = 1, pageSize = 20): Observable<PagedResult<SellerOrderItem>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (status !== undefined) {
      params = params.set('status', status);
    }
    return this.http.get<PagedResult<SellerOrderItem>>(`${this.ordersUrl}/seller/queue`, { params });
  }

  updateItemStatus(orderItemId: string, request: UpdateItemStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.ordersUrl}/items/${orderItemId}/status`, request);
  }

  // --- Admin-only actions (same /sellers resource, different role) ---

  getPendingSellers(pageNumber = 1, pageSize = 20): Observable<PagedResult<SellerProfile>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<SellerProfile>>(`${this.sellersUrl}/pending`, { params });
  }

  reviewDocument(documentId: string, decision: DocumentVerificationStatus, reviewerNote?: string | null): Observable<void> {
    return this.http.post<void>(`${this.sellersUrl}/documents/${documentId}/review`, { decision, reviewerNote });
  }

  approveSeller(sellerId: string): Observable<void> {
    return this.http.post<void>(`${this.sellersUrl}/${sellerId}/approve`, {});
  }

  rejectSeller(sellerId: string, reason: string): Observable<void> {
    return this.http.post<void>(`${this.sellersUrl}/${sellerId}/reject`, { reason });
  }
}
