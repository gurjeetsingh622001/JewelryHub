import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MyProfile, UpdateMyProfileRequest } from './models';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  getMyProfile(): Observable<MyProfile> {
    return this.http.get<MyProfile>(`${this.baseUrl}/me`);
  }

  updateMyProfile(request: UpdateMyProfileRequest): Observable<MyProfile> {
    return this.http.put<MyProfile>(`${this.baseUrl}/me`, request);
  }
}
