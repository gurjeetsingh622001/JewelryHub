import { Injectable } from '@angular/core';
import { AuthenticatedUser, AuthResponse } from './models';

const ACCESS_TOKEN_KEY = 'jewelryhub.accessToken';
const REFRESH_TOKEN_KEY = 'jewelryhub.refreshToken';
const USER_KEY = 'jewelryhub.user';

/**
 * The only thing in the app allowed to touch localStorage for auth state —
 * everything else goes through AuthService. Access + refresh tokens live in
 * localStorage (not an httpOnly cookie) because the backend's /auth/refresh
 * endpoint expects the raw refresh token in the request body, not a cookie;
 * this is a plain SPA-calls-API setup, not a BFF.
 *
 * Guarded against localStorage being unavailable (unit-test runners without
 * a real browser, private-browsing edge cases) rather than assuming a
 * browser environment — falls back to acting like an always-empty store.
 */
@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  private readonly store = this.resolveStorage();

  save(response: AuthResponse): void {
    this.store?.setItem(ACCESS_TOKEN_KEY, response.accessToken);
    this.store?.setItem(REFRESH_TOKEN_KEY, response.refreshToken);
    const user: AuthenticatedUser = {
      userId: response.userId,
      email: response.email,
      firstName: response.firstName,
      lastName: response.lastName,
      roles: response.roles,
    };
    this.store?.setItem(USER_KEY, JSON.stringify(user));
  }

  /** Merges a partial update (e.g. a name change from the Profile page) onto the cached user, without touching tokens. */
  updateUser(partial: Partial<AuthenticatedUser>): AuthenticatedUser | null {
    const current = this.loadUser();
    if (!current) return null;
    const updated = { ...current, ...partial };
    this.store?.setItem(USER_KEY, JSON.stringify(updated));
    return updated;
  }

  clear(): void {
    this.store?.removeItem(ACCESS_TOKEN_KEY);
    this.store?.removeItem(REFRESH_TOKEN_KEY);
    this.store?.removeItem(USER_KEY);
  }

  getAccessToken(): string | null {
    return this.store?.getItem(ACCESS_TOKEN_KEY) ?? null;
  }

  getRefreshToken(): string | null {
    return this.store?.getItem(REFRESH_TOKEN_KEY) ?? null;
  }

  loadUser(): AuthenticatedUser | null {
    const raw = this.store?.getItem(USER_KEY);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as AuthenticatedUser;
    } catch {
      return null;
    }
  }

  private resolveStorage(): Storage | null {
    try {
      return typeof localStorage !== 'undefined' ? localStorage : null;
    } catch {
      return null;
    }
  }
}
