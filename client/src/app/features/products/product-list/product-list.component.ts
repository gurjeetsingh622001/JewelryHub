import { Component, computed, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { ProductCardComponent } from '../../../shared/product-card/product-card.component';
import { ProductsService } from '../products.service';
import { METAL_TYPE_LABELS, MetalType, ProductFilters, ProductSortOption } from '../models';

const PAGE_SIZE = 12;

@Component({
  selector: 'app-product-list',
  imports: [
    FormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    LucideAngularModule,
    ProductCardComponent,
  ],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss',
})
export class ProductListComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productsService = inject(ProductsService);

  protected readonly metalTypeOptions = Object.entries(METAL_TYPE_LABELS).map(([value, label]) => ({
    value: Number(value) as MetalType,
    label,
  }));

  protected readonly sortOptions = [
    { value: ProductSortOption.Newest, label: 'Newest' },
    { value: ProductSortOption.PriceLowToHigh, label: 'Price: Low to High' },
    { value: ProductSortOption.PriceHighToLow, label: 'Price: High to Low' },
    { value: ProductSortOption.RatingDesc, label: 'Top Rated' },
  ];

  // The URL's query params are the single source of truth for filters —
  // sharable/bookmarkable, and how Navbar/Home's category links drive this
  // page (e.g. /products?search=ring).
  private readonly queryParamMap = toSignal(this.route.queryParamMap, { requireSync: true });

  protected readonly filters = computed<ProductFilters>(() => {
    const params = this.queryParamMap();
    return {
      search: params.get('search') ?? undefined,
      categoryId: params.get('categoryId') ?? undefined,
      metalType: params.get('metalType') !== null ? Number(params.get('metalType')) : undefined,
      minPrice: params.get('minPrice') !== null ? Number(params.get('minPrice')) : undefined,
      maxPrice: params.get('maxPrice') !== null ? Number(params.get('maxPrice')) : undefined,
      sort: params.get('sort') !== null ? Number(params.get('sort')) : ProductSortOption.Newest,
      pageNumber: params.get('page') !== null ? Number(params.get('page')) : 1,
      pageSize: PAGE_SIZE,
    };
  });

  // Local, editable mirror of the search box only — typing shouldn't
  // navigate on every keystroke, so it's committed to the URL on Enter.
  protected readonly searchInput = signal(this.filters().search ?? '');

  protected readonly productsResource = rxResource({
    params: this.filters,
    stream: ({ params }) => this.productsService.getProducts(params),
  });

  protected readonly categoriesResource = rxResource({
    stream: () => this.productsService.getCategories(),
  });

  submitSearch(): void {
    this.updateFilters({ search: this.searchInput() || undefined });
  }

  updateFilters(patch: Record<string, string | number | undefined>): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { ...patch, page: undefined }, // any filter change resets to page 1
      queryParamsHandling: 'merge',
    });
  }

  goToPage(page: number): void {
    this.router.navigate([], { relativeTo: this.route, queryParams: { page }, queryParamsHandling: 'merge' });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  clearFilters(): void {
    this.searchInput.set('');
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
  }
}
