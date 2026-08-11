import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../../core/models/paged-result';
import { AdminReview, AdminReviewFilters, DashboardOverview, UserFilters, UserSummary } from './models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/admin`;

  getDashboard(): Observable<DashboardOverview> {
    return this.http.get<DashboardOverview>(`${this.baseUrl}/dashboard`);
  }

  getUsers(filters: UserFilters): Observable<PagedResult<UserSummary>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<UserSummary>>(`${this.baseUrl}/users`, { params });
  }

  setUserStatus(userId: string, isActive: boolean): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${userId}/status`, { isActive });
  }

  getReviews(filters: AdminReviewFilters): Observable<PagedResult<AdminReview>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<AdminReview>>(`${this.baseUrl}/reviews`, { params });
  }
}
