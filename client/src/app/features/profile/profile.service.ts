import { HttpClient } from '@angular/common/http';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';
import { MyProfile, UpdateMyProfileRequest } from './models';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly baseUrl = `${environment.apiUrl}/users`;

  // Same lifecycle as CartService/WishlistService — cached so the Navbar
  // (avatar in the trigger button, name/photo in the account menu) doesn't
  // need its own separate fetch, and stays in sync the moment the Profile
  // page saves a change.
  private readonly _myProfile = signal<MyProfile | null>(null);
  readonly myProfile = this._myProfile.asReadonly();
  readonly photoUrl = computed(() => this._myProfile()?.photoUrl ?? null);

  constructor() {
    effect(() => {
      if (this.auth.isAuthenticated()) {
        this.refresh();
      } else {
        this._myProfile.set(null);
      }
    });
  }

  refresh(): void {
    this.getMyProfile().subscribe({
      next: (profile) => this._myProfile.set(profile),
      error: () => this._myProfile.set(null),
    });
  }

  getMyProfile(): Observable<MyProfile> {
    return this.http.get<MyProfile>(`${this.baseUrl}/me`);
  }

  updateMyProfile(request: UpdateMyProfileRequest): Observable<MyProfile> {
    return this.http.put<MyProfile>(`${this.baseUrl}/me`, request).pipe(tap((profile) => this._myProfile.set(profile)));
  }
}
