import { HttpClient } from '@angular/common/http';
import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';
import { Cart } from './models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly baseUrl = `${environment.apiUrl}/cart`;

  private readonly _cart = signal<Cart | null>(null);
  readonly cart = this._cart.asReadonly();
  readonly itemCount = computed(() => this._cart()?.totalItemCount ?? 0);

  constructor() {
    // The cart only exists for a logged-in Customer (the backend's
    // CartController is [Authorize(Roles = "Customer")]) — refresh it the
    // moment that becomes true (login, or app boot already logged in), and
    // drop it the moment it stops being true (logout, or a Seller/Admin
    // session with no cart of their own).
    effect(() => {
      if (this.auth.isAuthenticated() && this.auth.hasRole('Customer')) {
        this.refresh();
      } else {
        this._cart.set(null);
      }
    });
  }

  refresh(): void {
    this.http.get<Cart>(this.baseUrl).subscribe({
      next: (cart) => this._cart.set(cart),
      // Not every Customer has a cart row yet (none created until the first
      // add) — a 404 here just means "empty", not a real failure.
      error: () => this._cart.set(null),
    });
  }

  addItem(productId: string, quantity = 1): Observable<Cart> {
    return this.http
      .post<Cart>(`${this.baseUrl}/items`, { productId, quantity })
      .pipe(tap((cart) => this._cart.set(cart)));
  }

  updateQuantity(productId: string, quantity: number): Observable<Cart> {
    return this.http
      .put<Cart>(`${this.baseUrl}/items/${productId}`, { quantity })
      .pipe(tap((cart) => this._cart.set(cart)));
  }

  removeItem(productId: string): Observable<Cart> {
    return this.http.delete<Cart>(`${this.baseUrl}/items/${productId}`).pipe(tap((cart) => this._cart.set(cart)));
  }

  clear(): Observable<void> {
    return this.http.delete<void>(this.baseUrl).pipe(tap(() => this._cart.set(null)));
  }
}
