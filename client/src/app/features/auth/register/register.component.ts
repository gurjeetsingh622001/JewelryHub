import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SelectButtonModule } from 'primeng/selectbutton';
import { AuthService } from '../../../core/auth/auth.service';

type AccountType = 'Customer' | 'Seller';

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    SelectButtonModule,
  ],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly accountType = signal<AccountType>('Customer');
  protected readonly accountTypeOptions: { label: string; value: AccountType }[] = [
    { label: 'Customer', value: 'Customer' },
    { label: 'Seller', value: 'Seller' },
  ];

  protected readonly customerForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phoneNumber: [''],
  });

  protected readonly sellerForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    phoneNumber: [''],
    businessName: ['', Validators.required],
    gstNumber: ['', [Validators.required, Validators.pattern(/^[0-9A-Z]{15}$/)]],
    businessRegistrationNumber: ['', Validators.required],
    addressLine1: ['', Validators.required],
    addressLine2: [''],
    city: ['', Validators.required],
    state: ['', Validators.required],
    postalCode: ['', Validators.required],
  });

  submit(): void {
    if (this.accountType() === 'Customer') {
      if (this.customerForm.invalid) {
        this.customerForm.markAllAsTouched();
        return;
      }
      this.submitting.set(true);
      this.authService.registerCustomer(this.customerForm.getRawValue()).subscribe({
        next: () => this.router.navigateByUrl('/'),
        error: () => this.submitting.set(false),
      });
      return;
    }

    if (this.sellerForm.invalid) {
      this.sellerForm.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.authService.registerSeller(this.sellerForm.getRawValue()).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: () => this.submitting.set(false),
    });
  }
}
