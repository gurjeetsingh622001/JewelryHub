import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { resolveMediaUrl } from '../../../shared/resolve-media-url';
import { CartService } from '../../cart/cart.service';
import { WishlistService } from '../wishlist.service';

@Component({
  selector: 'app-wishlist-page',
  imports: [RouterLink, MatButtonModule, LucideAngularModule, CurrencyPipe],
  templateUrl: './wishlist-page.component.html',
  styleUrl: './wishlist-page.component.scss',
})
export class WishlistPageComponent {
  protected readonly wishlistService = inject(WishlistService);
  protected readonly resolveMediaUrl = resolveMediaUrl;
  private readonly cartService = inject(CartService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  // Tracks which single line is mid-request, so only that row shows a busy
  // state instead of freezing the whole list on every click.
  protected readonly busyProductId = signal<string | null>(null);

  removeItem(productId: string): void {
    this.busyProductId.set(productId);
    this.wishlistService.removeItem(productId).subscribe({
      complete: () => this.busyProductId.set(null),
    });
  }

  moveToCart(productId: string): void {
    this.busyProductId.set(productId);
    this.cartService.addItem(productId, 1).subscribe({
      next: () => {
        this.wishlistService.removeItem(productId).subscribe({
          complete: () => this.busyProductId.set(null),
        });
        this.snackBar.open('Added to cart.', 'View Cart', { duration: 4000 }).onAction().subscribe(() => {
          this.router.navigateByUrl('/cart');
        });
      },
      error: () => this.busyProductId.set(null),
    });
  }
}
