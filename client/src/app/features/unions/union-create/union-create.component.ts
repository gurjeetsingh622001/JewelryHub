import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { UnionsService } from '../unions.service';

@Component({
  selector: 'app-union-create',
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule],
  templateUrl: './union-create.component.html',
  styleUrl: './union-create.component.scss',
})
export class UnionCreateComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly unionsService = inject(UnionsService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly submitting = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    logoUrl: [''],
    city: ['', [Validators.required, Validators.maxLength(100)]],
    state: ['', [Validators.required, Validators.maxLength(100)]],
    annualMembershipFee: [null as number | null],
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    this.submitting.set(true);
    this.unionsService
      .create({
        name: v.name,
        description: v.description || null,
        logoUrl: v.logoUrl || null,
        city: v.city,
        state: v.state,
        annualMembershipFee: v.annualMembershipFee,
      })
      .subscribe({
        next: (union) => {
          this.submitting.set(false);
          this.snackBar.open('Union created — awaiting admin approval before it appears in the public directory.', 'Dismiss', {
            duration: 6000,
          });
          this.router.navigate(['/unions', union.id]);
        },
        error: () => this.submitting.set(false),
      });
  }
}
