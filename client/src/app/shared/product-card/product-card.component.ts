import { CurrencyPipe } from '@angular/common';
import { Component, Input, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { ProductListItem, effectivePrice } from '../../features/products/models';

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

  private readonly snackBar = inject(MatSnackBar);

  protected readonly effectivePrice = effectivePrice;

  addToWishlist(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    // Wishlist isn't wired up yet (see docs/ROADMAP.md Phase 13b) — honest
    // feedback instead of a silent no-op click.
    this.snackBar.open('Wishlist is coming soon.', 'Dismiss', { duration: 4000 });
  }
}
