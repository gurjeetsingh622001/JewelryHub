import { Component, computed, effect, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { map } from 'rxjs';
import { METAL_TYPE_LABELS, PRODUCT_TYPE_LABELS, PURITY_TYPE_LABELS } from '../../../products/models';
import { ProductsService } from '../../../products/products.service';
import { SellerService } from '../../seller.service';

@Component({
  selector: 'app-seller-product-form',
  imports: [
    RouterLink,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    LucideAngularModule,
  ],
  templateUrl: './seller-product-form.component.html',
  styleUrl: './seller-product-form.component.scss',
})
export class SellerProductFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly productsService = inject(ProductsService);
  private readonly sellerService = inject(SellerService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly metalTypeOptions = Object.entries(METAL_TYPE_LABELS).map(([value, label]) => ({ value: Number(value), label }));
  protected readonly purityTypeOptions = Object.entries(PURITY_TYPE_LABELS).map(([value, label]) => ({ value: Number(value), label }));
  protected readonly productTypeOptions = Object.entries(PRODUCT_TYPE_LABELS).map(([value, label]) => ({ value: Number(value), label }));

  protected readonly productId = toSignal(this.route.paramMap.pipe(map((params) => params.get('id'))), { requireSync: true });
  protected readonly isEditMode = computed(() => this.productId() !== null);

  // rxResource only skips its loader when params returns undefined, not
  // null — on the "new product" route productId() is null, and passing
  // that straight through would fire GET /products/null.
  private readonly editProductId = computed(() => this.productId() ?? undefined);

  protected readonly saving = signal(false);
  protected readonly adjustingInventory = signal(false);
  protected readonly inventoryDelta = signal(0);

  protected readonly categoriesResource = rxResource({
    stream: () => this.productsService.getCategories(),
  });

  protected readonly productResource = rxResource({
    params: this.editProductId,
    stream: ({ params }) => this.productsService.getProductById(params),
  });

  protected readonly createForm = this.fb.nonNullable.group({
    categoryId: ['', Validators.required],
    name: ['', [Validators.required, Validators.maxLength(250)]],
    sku: ['', [Validators.required, Validators.maxLength(80)]],
    description: [''],
    productType: [0, Validators.required],
    metalType: [0, Validators.required],
    purity: [0, Validators.required],
    grossWeightGrams: [0, [Validators.required, Validators.min(0.01)]],
    netWeightGrams: [0, [Validators.required, Validators.min(0.01)]],
    metalRatePerGramAtListing: [0, [Validators.required, Validators.min(0.01)]],
    size: [''],
    sizeUnit: [''],
    makingCharges: [0, [Validators.required, Validators.min(0)]],
    makingChargesArePercentage: [true],
    wastageCharges: [0, [Validators.required, Validators.min(0)]],
    discountPercentage: [null as number | null],
    isHallmarked: [false],
    hallmarkUniqueId: [''],
    certificationAuthority: [''],
    initialQuantity: [1, [Validators.required, Validators.min(0)]],
    imageUrl: [''],
  });

  protected readonly editForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(250)]],
    description: [''],
    grossWeightGrams: [0, [Validators.required, Validators.min(0.01)]],
    netWeightGrams: [0, [Validators.required, Validators.min(0.01)]],
    metalRatePerGramAtListing: [0, [Validators.required, Validators.min(0.01)]],
    size: [''],
    sizeUnit: [''],
    makingCharges: [0, [Validators.required, Validators.min(0)]],
    makingChargesArePercentage: [true],
    wastageCharges: [0, [Validators.required, Validators.min(0)]],
    discountPercentage: [null as number | null],
  });

  constructor() {
    // Patches the edit form the moment the product resource resolves.
    effect(() => {
      const product = this.productResource.value();
      if (product && this.isEditMode()) {
        this.editForm.patchValue({
          name: product.name,
          description: product.description ?? '',
          grossWeightGrams: product.grossWeightGrams,
          netWeightGrams: product.netWeightGrams,
          metalRatePerGramAtListing: product.metalRatePerGramAtListing,
          size: product.size ?? '',
          sizeUnit: product.sizeUnit ?? '',
          makingCharges: product.makingCharges,
          makingChargesArePercentage: product.makingChargesArePercentage,
          wastageCharges: product.wastageCharges,
          discountPercentage: product.discountPercentage,
        });
      }
    });
  }

  submitCreate(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const v = this.createForm.getRawValue();
    this.saving.set(true);
    this.sellerService
      .createProduct({
        categoryId: v.categoryId,
        name: v.name,
        sku: v.sku,
        description: v.description || null,
        productType: v.productType,
        metalType: v.metalType,
        purity: v.purity,
        grossWeightGrams: v.grossWeightGrams,
        netWeightGrams: v.netWeightGrams,
        metalRatePerGramAtListing: v.metalRatePerGramAtListing,
        size: v.size || null,
        sizeUnit: v.sizeUnit || null,
        makingCharges: v.makingCharges,
        makingChargesArePercentage: v.makingChargesArePercentage,
        wastageCharges: v.wastageCharges,
        discountPercentage: v.discountPercentage,
        isHallmarked: v.isHallmarked,
        hallmarkUniqueId: v.hallmarkUniqueId || null,
        certificationAuthority: v.certificationAuthority || null,
        initialQuantity: v.initialQuantity,
        gemstones: [],
        images: v.imageUrl ? [{ url: v.imageUrl, altText: v.name, displayOrder: 0, isPrimary: true }] : [],
      })
      .subscribe({
        next: (product) => {
          this.saving.set(false);
          this.snackBar.open('Product listed successfully.', 'Dismiss', { duration: 4000 });
          this.router.navigate(['/seller/products', product.id, 'edit']);
        },
        error: () => this.saving.set(false),
      });
  }

  submitEdit(): void {
    const id = this.productId();
    if (!id || this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const v = this.editForm.getRawValue();
    this.saving.set(true);
    this.sellerService
      .updateProduct(id, {
        name: v.name,
        description: v.description || null,
        grossWeightGrams: v.grossWeightGrams,
        netWeightGrams: v.netWeightGrams,
        metalRatePerGramAtListing: v.metalRatePerGramAtListing,
        size: v.size || null,
        sizeUnit: v.sizeUnit || null,
        makingCharges: v.makingCharges,
        makingChargesArePercentage: v.makingChargesArePercentage,
        wastageCharges: v.wastageCharges,
        discountPercentage: v.discountPercentage,
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.snackBar.open('Product updated.', 'Dismiss', { duration: 4000 });
          this.productResource.reload();
        },
        error: () => this.saving.set(false),
      });
  }

  adjustInventory(delta: number): void {
    const id = this.productId();
    if (!id || delta === 0) return;

    this.adjustingInventory.set(true);
    this.sellerService.adjustInventory(id, delta).subscribe({
      next: () => {
        this.adjustingInventory.set(false);
        this.inventoryDelta.set(0);
        this.snackBar.open('Inventory updated.', 'Dismiss', { duration: 4000 });
        this.productResource.reload();
      },
      error: () => this.adjustingInventory.set(false),
    });
  }
}
