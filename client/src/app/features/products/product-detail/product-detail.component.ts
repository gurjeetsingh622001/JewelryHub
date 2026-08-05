import { CurrencyPipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { ProductsService } from '../products.service';
import { METAL_TYPE_LABELS, PURITY_TYPE_LABELS, effectivePrice } from '../models';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, LucideAngularModule, CurrencyPipe],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss',
})
export class ProductDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly productsService = inject(ProductsService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly metalTypeLabels = METAL_TYPE_LABELS;
  protected readonly purityTypeLabels = PURITY_TYPE_LABELS;
  protected readonly effectivePrice = effectivePrice;

  private readonly productId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id')!)), {
    requireSync: true,
  });

  protected readonly selectedImageIndex = signal(0);

  protected readonly productResource = rxResource({
    params: this.productId,
    stream: ({ params }) => this.productsService.getProductById(params),
  });

  selectImage(index: number): void {
    this.selectedImageIndex.set(index);
  }

  /** Cart isn't wired up yet (see docs/ROADMAP.md Phase 13b) — honest feedback instead of a silent no-op click. */
  addToCart(): void {
    this.snackBar.open('Cart is coming soon.', 'Dismiss', { duration: 4000 });
  }

  addToWishlist(): void {
    this.snackBar.open('Wishlist is coming soon.', 'Dismiss', { duration: 4000 });
  }
}
