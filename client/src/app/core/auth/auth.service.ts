import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, of, shareReplay, tap } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { AuthResponse, AuthenticatedUser, LoginRequest, RegisterCustomerRequest, RegisterSellerRequest } from './models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(TokenStorageService);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly _currentUser = signal<AuthenticatedUser | null>(this.storage.loadUser());
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this._currentUser() !== null);
  readonly roles = computed(() => this._currentUser()?.roles ?? []);

  // Two requests hitting a 401 at the same moment must share one refresh
  // call, not fire two — a second /auth/refresh with an already-rotated
  // token would be treated as reuse and revoke every session. See
  // AuthInterceptor.
  private refreshInFlight: Observable<AuthResponse> | null = null;

  hasRole(role: string): boolean {
    return this.roles().includes(role);
  }

  /** Keeps the cached name (Navbar greeting, etc.) in sync after a Profile page edit, without a full re-login. */
  updateCachedName(firstName: string, lastName: string): void {
    const updated = this.storage.updateUser({ firstName, lastName });
    if (updated) {
      this._currentUser.set(updated);
    }
  }

  getAccessToken(): string | null {
    return this.storage.getAccessToken();
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request).pipe(tap((r) => this.setSession(r)));
  }

  registerCustomer(request: RegisterCustomerRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register/customer`, request).pipe(tap((r) => this.setSession(r)));
  }

  registerSeller(request: RegisterSellerRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register/seller`, request).pipe(tap((r) => this.setSession(r)));
  }

  /** Shared across concurrent 401s so only one /auth/refresh call is ever in flight at a time. */
  refresh(): Observable<AuthResponse> {
    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    const refreshToken = this.storage.getRefreshToken();
    if (!refreshToken) {
      this.clearSession();
      throw new Error('No refresh token available.');
    }

    this.refreshInFlight = this.http.post<AuthResponse>(`${this.baseUrl}/refresh`, { refreshToken }).pipe(
      tap((r) => this.setSession(r)),
      finalize(() => (this.refreshInFlight = null)),
      shareReplay(1),
    );
    return this.refreshInFlight;
  }

  logout(): Observable<void> {
    const refreshToken = this.storage.getRefreshToken();
    this.clearSession();
    if (!refreshToken) {
      return of(undefined);
    }
    // Best-effort — the client-side session is already cleared either way.
    return this.http.post<void>(`${this.baseUrl}/logout`, { refreshToken }).pipe(catchError(() => of(undefined)));
  }

  private setSession(response: AuthResponse): void {
    this.storage.save(response);
    this._currentUser.set({
      userId: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
      roles: response.roles,
    });
  }

  private clearSession(): void {
    this.storage.clear();
    this._currentUser.set(null);
  }
}
