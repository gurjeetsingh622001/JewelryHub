import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../../core/models/paged-result';
import { ActionItem, CreateMeetingRequest, Meeting, MeetingAttendanceStatus, MeetingMinute, MeetingStatus, MeetingSummary, RecordMinuteRequest } from './models';

@Injectable({ providedIn: 'root' })
export class MeetingsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/union-meetings`;

  getMeetings(unionId: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<MeetingSummary>> {
    const params = new HttpParams().set('unionId', unionId).set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<MeetingSummary>>(this.baseUrl, { params });
  }

  create(request: CreateMeetingRequest): Observable<Meeting> {
    return this.http.post<Meeting>(this.baseUrl, request);
  }

  getById(id: string): Observable<Meeting> {
    return this.http.get<Meeting>(`${this.baseUrl}/${id}`);
  }

  updateStatus(id: string, status: MeetingStatus): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, { status });
  }

  rsvp(id: string, status: MeetingAttendanceStatus): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/rsvp`, { status });
  }

  recordMinute(id: string, request: RecordMinuteRequest): Observable<MeetingMinute> {
    return this.http.post<MeetingMinute>(`${this.baseUrl}/${id}/minutes`, request);
  }

  getMyActionItems(): Observable<ActionItem[]> {
    return this.http.get<ActionItem[]>(`${this.baseUrl}/my-action-items`);
  }
}
