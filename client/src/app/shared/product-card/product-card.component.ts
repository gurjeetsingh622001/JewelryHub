import { CurrencyPipe } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../core/auth/auth.service';
import { ProductListItem, effectivePrice } from '../../features/products/models';
import { WishlistService } from '../../features/wishlist/wishlist.service';
import { resolveMediaUrl } from '../resolve-media-url';

/**
 * Reusable listing card — used by the Product List grid, and intended for
 * anywhere else a compact product summary is needed (search results,
 * "you may also like", etc.) so the card design stays in exactly one place.
 */
@Component({
  selector: 'app-product-card',
  imports: [RouterLink, LucideAngularModule, CurrencyPipe],
  templateUrl: './product-card.component.html',
  styleUrl: './product-card.component.scss',
})
export class ProductCardComponent {
  @Input({ required: true }) product!: ProductListItem;

  protected readonly wishlistService = inject(WishlistService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly effectivePrice = effectivePrice;
  protected readonly resolveMediaUrl = resolveMediaUrl;

  toggleWishlist(event: Event): void {
    event.preventDefault();
    event.stopPropagation();

    if (!this.auth.isAuthenticated()) {
      this.router.navigateByUrl('/login');
      return;
    }
    if (!this.auth.hasRole('Customer')) {
      this.snackBar.open('Only customer accounts have a wishlist.', 'Dismiss', { duration: 4000 });
      return;
    }

    if (this.wishlistService.isInWishlist(this.product.id)) {
      this.wishlistService.removeItem(this.product.id).subscribe();
    } else {
      this.wishlistService.addItem(this.product.id).subscribe({
        next: () => this.snackBar.open('Added to wishlist.', 'Dismiss', { duration: 3000 }),
      });
    }
  }
}
