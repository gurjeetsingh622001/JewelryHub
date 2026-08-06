import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { ProductsService } from '../../products/products.service';
import { SellerService } from '../seller.service';

@Component({
  selector: 'app-seller-products',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, LucideAngularModule, CurrencyPipe],
  templateUrl: './seller-products.component.html',
  styleUrl: './seller-products.component.scss',
})
export class SellerProductsComponent {
  private readonly sellerService = inject(SellerService);
  private readonly productsService = inject(ProductsService);

  protected readonly profileResource = rxResource({
    stream: () => this.sellerService.getMyProfile(),
  });

  private readonly sellerId = computed(() => this.profileResource.value()?.id);

  protected readonly productsResource = rxResource({
    params: this.sellerId,
    stream: ({ params }) => this.productsService.getProducts({ sellerId: params, pageSize: 100 }),
  });
}
