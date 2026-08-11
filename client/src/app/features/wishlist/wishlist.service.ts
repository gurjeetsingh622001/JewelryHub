import { HttpClient } from '@angular/common/http';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';
import { Wishlist } from './models';

@Injectable({ providedIn: 'root' })
export class WishlistService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly baseUrl = `${environment.apiUrl}/wishlist`;

  private readonly _wishlist = signal<Wishlist | null>(null);
  readonly wishlist = this._wishlist.asReadonly();
  readonly itemCount = computed(() => this._wishlist()?.items.length ?? 0);

  constructor() {
    // Same lifecycle as CartService — the backend's WishlistController is
    // [Authorize(Roles = "Customer")], so only refresh/hold state for a
    // logged-in Customer session.
    effect(() => {
      if (this.auth.isAuthenticated() && this.auth.hasRole('Customer')) {
        this.refresh();
      } else {
        this._wishlist.set(null);
      }
    });
  }

  refresh(): void {
    this.http.get<Wishlist>(this.baseUrl).subscribe({
      next: (wishlist) => this._wishlist.set(wishlist),
      // No wishlist row yet (none created until the first add) — a 404
      // here just means "empty", not a real failure.
      error: () => this._wishlist.set(null),
    });
  }

  isInWishlist(productId: string): boolean {
    return this._wishlist()?.items.some((i) => i.productId === productId) ?? false;
  }

  addItem(productId: string): Observable<Wishlist> {
    return this.http
      .post<Wishlist>(`${this.baseUrl}/items/${productId}`, {})
      .pipe(tap((wishlist) => this._wishlist.set(wishlist)));
  }

  removeItem(productId: string): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/items/${productId}`)
      .pipe(
        tap(() => {
          const current = this._wishlist();
          if (current) {
            this._wishlist.set({ ...current, items: current.items.filter((i) => i.productId !== productId) });
          }
        }),
      );
  }
}
