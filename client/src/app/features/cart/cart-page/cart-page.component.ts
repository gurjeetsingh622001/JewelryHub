import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { LucideAngularModule } from 'lucide-angular';
import { resolveMediaUrl } from '../../../shared/resolve-media-url';
import { CartService } from '../cart.service';

@Component({
  selector: 'app-cart-page',
  imports: [RouterLink, MatButtonModule, LucideAngularModule, CurrencyPipe],
  templateUrl: './cart-page.component.html',
  styleUrl: './cart-page.component.scss',
})
export class CartPageComponent {
  protected readonly cartService = inject(CartService);
  protected readonly resolveMediaUrl = resolveMediaUrl;

  // Tracks which single line is mid-request, so only that row shows a
  // busy state instead of freezing the whole cart on every click.
  protected readonly updatingProductId = signal<string | null>(null);

  updateQuantity(productId: string, quantity: number): void {
    if (quantity < 1) return;
    this.updatingProductId.set(productId);
    this.cartService.updateQuantity(productId, quantity).subscribe({
      complete: () => this.updatingProductId.set(null),
    });
  }

  removeItem(productId: string): void {
    this.updatingProductId.set(productId);
    this.cartService.removeItem(productId).subscribe({
      complete: () => this.updatingProductId.set(null),
    });
  }

  clearCart(): void {
    this.cartService.clear().subscribe();
  }
}
