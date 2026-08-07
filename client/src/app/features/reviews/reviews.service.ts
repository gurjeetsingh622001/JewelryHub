import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../../core/models/paged-result';
import { CreateReviewRequest, Review } from './models';

@Injectable({ providedIn: 'root' })
export class ReviewsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/reviews`;

  getForProduct(productId: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<Review>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<Review>>(`${this.baseUrl}/product/${productId}`, { params });
  }

  create(request: CreateReviewRequest): Observable<Review> {
    return this.http.post<Review>(this.baseUrl, request);
  }
}
