import { DatePipe } from '@angular/common';
import { Component, ViewChild, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { DOCUMENT_STATUS_LABELS } from '../models';
import { SellerService } from '../seller.service';

const DOCUMENT_TYPES = ['GST Certificate', 'PAN Card', 'Business License', 'Aadhaar Card', 'Bank Statement', 'Other'];

@Component({
  selector: 'app-seller-kyc',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    LucideAngularModule,
    DatePipe,
  ],
  templateUrl: './seller-kyc.component.html',
  styleUrl: './seller-kyc.component.scss',
})
export class SellerKycComponent {
  private readonly fb = inject(FormBuilder);
  private readonly sellerService = inject(SellerService);
  private readonly snackBar = inject(MatSnackBar);

  // Plain form.reset() clears values but not FormGroupDirective's internal
  // "submitted" flag, so Material's default ErrorStateMatcher would keep
  // showing every required field as invalid right after a successful
  // submit — resetForm() on the directive clears both.
  @ViewChild(FormGroupDirective) private formDirective!: FormGroupDirective;

  protected readonly documentTypes = DOCUMENT_TYPES;
  protected readonly statusLabels = DOCUMENT_STATUS_LABELS;
  protected readonly submitting = signal(false);

  protected readonly profileResource = rxResource({
    stream: () => this.sellerService.getMyProfile(),
  });

  protected readonly form = this.fb.nonNullable.group({
    documentType: ['', Validators.required],
    fileUrl: ['', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]],
    fileName: [''],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.sellerService
      .submitDocument({ documentType: value.documentType, fileUrl: value.fileUrl, fileName: value.fileName || null })
      .subscribe({
        next: () => {
          this.submitting.set(false);
          this.formDirective.resetForm({ documentType: '', fileUrl: '', fileName: '' });
          this.snackBar.open('Document submitted for review.', 'Dismiss', { duration: 4000 });
          this.profileResource.reload();
        },
        error: () => this.submitting.set(false),
      });
  }
}
