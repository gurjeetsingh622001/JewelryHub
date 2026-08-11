import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../../core/models/paged-result';
import {
  CreateAnnouncementRequest,
  CreateEventRequest,
  CreateUnionRequest,
  Union,
  UnionAnnouncement,
  UnionDocument,
  UnionEvent,
  UnionEventStatus,
  UnionFilters,
  UnionMember,
  UploadDocumentRequest,
} from './models';

@Injectable({ providedIn: 'root' })
export class UnionsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/unions`;

  getUnions(filters: UnionFilters): Observable<PagedResult<Union>> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return this.http.get<PagedResult<Union>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Union> {
    return this.http.get<Union>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateUnionRequest): Observable<Union> {
    return this.http.post<Union>(this.baseUrl, request);
  }

  join(unionId: string): Observable<UnionMember> {
    return this.http.post<UnionMember>(`${this.baseUrl}/${unionId}/join`, {});
  }

  getMyMemberships(): Observable<UnionMember[]> {
    return this.http.get<UnionMember[]>(`${this.baseUrl}/me/memberships`);
  }

  getMembers(unionId: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<UnionMember>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<UnionMember>>(`${this.baseUrl}/${unionId}/members`, { params });
  }

  getPendingMembers(unionId: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<UnionMember>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<UnionMember>>(`${this.baseUrl}/${unionId}/members/pending`, { params });
  }

  reviewMembership(membershipId: string, approve: boolean): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/memberships/${membershipId}/review`, { approve });
  }

  removeMember(membershipId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/memberships/${membershipId}`);
  }

  getAnnouncements(unionId: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<UnionAnnouncement>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<UnionAnnouncement>>(`${this.baseUrl}/${unionId}/announcements`, { params });
  }

  createAnnouncement(unionId: string, request: CreateAnnouncementRequest): Observable<UnionAnnouncement> {
    return this.http.post<UnionAnnouncement>(`${this.baseUrl}/${unionId}/announcements`, request);
  }

  deleteAnnouncement(announcementId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/announcements/${announcementId}`);
  }

  getDocuments(unionId: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<UnionDocument>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<UnionDocument>>(`${this.baseUrl}/${unionId}/documents`, { params });
  }

  uploadDocument(unionId: string, request: UploadDocumentRequest): Observable<UnionDocument> {
    return this.http.post<UnionDocument>(`${this.baseUrl}/${unionId}/documents`, request);
  }

  getEvents(unionId: string, pageNumber = 1, pageSize = 20): Observable<PagedResult<UnionEvent>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<UnionEvent>>(`${this.baseUrl}/${unionId}/events`, { params });
  }

  createEvent(unionId: string, request: CreateEventRequest): Observable<UnionEvent> {
    return this.http.post<UnionEvent>(`${this.baseUrl}/${unionId}/events`, request);
  }

  updateEventStatus(eventId: string, status: UnionEventStatus): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/events/${eventId}/status`, { status });
  }

  // --- Admin-only actions (same /unions resource, different role) ---

  getPendingUnions(pageNumber = 1, pageSize = 20): Observable<PagedResult<Union>> {
    const params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    return this.http.get<PagedResult<Union>>(`${this.baseUrl}/pending`, { params });
  }

  approveUnion(unionId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${unionId}/approve`, {});
  }
}
