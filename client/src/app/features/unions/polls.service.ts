import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../../core/models/paged-result';
import { CreatePollRequest, Poll, PollSummary } from './models';

@Injectable({ providedIn: 'root' })
export class PollsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/union-polls`;

  getPolls(unionId: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<PollSummary>> {
    const params = new HttpParams().set('unionId', unionId).set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<PollSummary>>(this.baseUrl, { params });
  }

  create(request: CreatePollRequest): Observable<Poll> {
    return this.http.post<Poll>(this.baseUrl, request);
  }

  getById(id: string): Observable<Poll> {
    return this.http.get<Poll>(`${this.baseUrl}/${id}`);
  }

  open(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/open`, {});
  }

  close(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/close`, {});
  }

  vote(id: string, optionIds: string[]): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/vote`, { optionIds });
  }
}
