import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { CartService } from '../../cart/cart.service';
import { ReviewsService } from '../../reviews/reviews.service';
import { WishlistService } from '../../wishlist/wishlist.service';
import { resolveMediaUrl } from '../../../shared/resolve-media-url';
import { ProductsService } from '../products.service';
import { METAL_TYPE_LABELS, PURITY_TYPE_LABELS, effectivePrice } from '../models';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, LucideAngularModule, CurrencyPipe, DatePipe],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss',
})
export class ProductDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productsService = inject(ProductsService);
  private readonly cartService = inject(CartService);
  private readonly reviewsService = inject(ReviewsService);
  protected readonly wishlistService = inject(WishlistService);
  private readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly metalTypeLabels = METAL_TYPE_LABELS;
  protected readonly purityTypeLabels = PURITY_TYPE_LABELS;
  protected readonly effectivePrice = effectivePrice;
  protected readonly resolveMediaUrl = resolveMediaUrl;
  protected readonly stars = [1, 2, 3, 4, 5];

  protected readonly productId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id')!)), {
    requireSync: true,
  });

  protected readonly selectedImageIndex = signal(0);
  protected readonly quantity = signal(1);
  protected readonly addingToCart = signal(false);

  protected readonly productResource = rxResource({
    params: this.productId,
    stream: ({ params }) => this.productsService.getProductById(params),
  });

  protected readonly reviewsResource = rxResource({
    params: this.productId,
    stream: ({ params }) => this.reviewsService.getForProduct(params, 1, 10),
  });

  selectImage(index: number): void {
    this.selectedImageIndex.set(index);
  }

  decrementQuantity(): void {
    this.quantity.update((q) => Math.max(1, q - 1));
  }

  incrementQuantity(maxAvailable: number): void {
    this.quantity.update((q) => Math.min(maxAvailable, q + 1));
  }

  addToCart(): void {
    if (!this.auth.isAuthenticated()) {
      this.router.navigateByUrl('/login');
      return;
    }
    if (!this.auth.hasRole('Customer')) {
      this.snackBar.open('Only customer accounts can add items to a cart.', 'Dismiss', { duration: 4000 });
      return;
    }

    this.addingToCart.set(true);
    this.cartService.addItem(this.productId(), this.quantity()).subscribe({
      next: () => {
        this.addingToCart.set(false);
        this.snackBar.open('Added to cart.', 'View Cart', { duration: 4000 }).onAction().subscribe(() => {
          this.router.navigateByUrl('/cart');
        });
      },
      error: () => this.addingToCart.set(false),
    });
  }

  toggleWishlist(): void {
    if (!this.auth.isAuthenticated()) {
      this.router.navigateByUrl('/login');
      return;
    }
    if (!this.auth.hasRole('Customer')) {
      this.snackBar.open('Only customer accounts have a wishlist.', 'Dismiss', { duration: 4000 });
      return;
    }

    const productId = this.productId();
    if (this.wishlistService.isInWishlist(productId)) {
      this.wishlistService.removeItem(productId).subscribe();
    } else {
      this.wishlistService.addItem(productId).subscribe({
        next: () => this.snackBar.open('Added to wishlist.', 'Dismiss', { duration: 3000 }),
      });
    }
  }
}
