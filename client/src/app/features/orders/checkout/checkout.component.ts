import { CurrencyPipe } from '@angular/common';
import { Component, effect, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { AddressesService } from '../../addresses/addresses.service';
import { CartService } from '../../cart/cart.service';
import { PAYMENT_METHOD_LABELS, PaymentMethod } from '../models';
import { OrdersService } from '../orders.service';

@Component({
  selector: 'app-checkout',
  imports: [
    RouterLink,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    LucideAngularModule,
    CurrencyPipe,
  ],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss',
})
export class CheckoutComponent {
  private readonly router = inject(Router);
  private readonly cartService = inject(CartService);
  private readonly addressesService = inject(AddressesService);
  private readonly ordersService = inject(OrdersService);
  private readonly fb = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly cart = this.cartService.cart;

  protected readonly paymentMethodOptions = Object.entries(PAYMENT_METHOD_LABELS).map(([value, label]) => ({
    value: Number(value) as PaymentMethod,
    label,
  }));
  protected readonly paymentMethod = signal<PaymentMethod>(PaymentMethod.Upi);

  protected readonly addressesResource = rxResource({ stream: () => this.addressesService.getMyAddresses() });
  protected readonly selectedAddressId = signal<string | null>(null);
  protected readonly showNewAddressForm = signal(false);
  protected readonly placingOrder = signal(false);
  protected readonly savingAddress = signal(false);

  protected readonly newAddressForm = this.fb.nonNullable.group({
    label: ['Home', Validators.required],
    addressLine1: ['', Validators.required],
    addressLine2: [''],
    city: ['', Validators.required],
    state: ['', Validators.required],
    postalCode: ['', Validators.required],
    contactPhone: [''],
    isDefault: [false],
  });

  constructor() {
    // Auto-select the default (or first) address the moment the list
    // loads, and open the "add address" form automatically if there are
    // none yet — a first-time customer shouldn't have to hunt for it.
    effect(() => {
      const addresses = this.addressesResource.value();
      if (!addresses) return;

      if (addresses.length === 0) {
        this.showNewAddressForm.set(true);
      } else if (!this.selectedAddressId()) {
        const preferred = addresses.find((a) => a.isDefault) ?? addresses[0];
        this.selectedAddressId.set(preferred.id);
      }
    });
  }

  selectAddress(id: string): void {
    this.selectedAddressId.set(id);
    this.showNewAddressForm.set(false);
  }

  saveNewAddress(): void {
    if (this.newAddressForm.invalid) {
      this.newAddressForm.markAllAsTouched();
      return;
    }

    this.savingAddress.set(true);
    this.addressesService.create(this.newAddressForm.getRawValue()).subscribe({
      next: (address) => {
        this.savingAddress.set(false);
        this.addressesResource.reload();
        this.selectedAddressId.set(address.id);
        this.showNewAddressForm.set(false);
        this.newAddressForm.reset({ label: 'Home', isDefault: false });
      },
      error: () => this.savingAddress.set(false),
    });
  }

  placeOrder(): void {
    const addressId = this.selectedAddressId();
    if (!addressId) {
      this.snackBar.open('Please select or add a shipping address.', 'Dismiss', { duration: 4000 });
      return;
    }

    this.placingOrder.set(true);
    this.ordersService
      .checkout({ shippingAddressId: addressId, billingAddressId: addressId, paymentMethod: this.paymentMethod() })
      .subscribe({
        next: (order) => {
          const payment = order.payments[0];
          // Cash on Delivery is collected on delivery, not confirmed up
          // front — every other method is "confirmed" immediately since
          // there's no real payment gateway to redirect to yet (see
          // OrdersService.confirmPayment).
          if (payment && this.paymentMethod() !== PaymentMethod.CashOnDelivery) {
            this.ordersService.confirmPayment(order.id, payment.id, `DEMO-${Date.now()}`).subscribe({
              complete: () => this.finishCheckout(order.id),
            });
          } else {
            this.finishCheckout(order.id);
          }
        },
        error: () => this.placingOrder.set(false),
      });
  }

  private finishCheckout(orderId: string): void {
    this.placingOrder.set(false);
    this.cartService.refresh(); // the backend clears the cart server-side on checkout
    this.router.navigate(['/orders', orderId], { queryParams: { placed: 'true' } });
  }
}
