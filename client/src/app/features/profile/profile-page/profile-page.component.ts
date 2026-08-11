import { Component, effect, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../../core/auth/auth.service';
import { UploadsService } from '../../../core/uploads/uploads.service';
import { resolveMediaUrl } from '../../../shared/resolve-media-url';
import { ProfileService } from '../profile.service';

@Component({
  selector: 'app-profile-page',
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, LucideAngularModule],
  templateUrl: './profile-page.component.html',
  styleUrl: './profile-page.component.scss',
})
export class ProfilePageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly profileService = inject(ProfileService);
  private readonly uploadsService = inject(UploadsService);
  protected readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly resolveMediaUrl = resolveMediaUrl;
  protected readonly saving = signal(false);
  protected readonly uploadingPhoto = signal(false);
  protected readonly photoUrl = signal<string | null>(null);

  // Only Customer/Seller accounts have a role-specific entity to hold a
  // photo (Customer.ProfileImageUrl / Seller.LogoUrl) — see MyProfileDto's
  // backend remarks. Admin has neither, so the photo section is hidden
  // rather than offering an upload that would silently not persist.
  protected readonly hasPhotoSlot = () => this.auth.hasRole('Customer') || this.auth.hasRole('Seller');

  protected readonly profileResource = rxResource({
    stream: () => this.profileService.getMyProfile(),
  });

  protected readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    phoneNumber: [''],
  });

  constructor() {
    effect(() => {
      const profile = this.profileResource.value();
      if (profile) {
        this.form.patchValue({
          firstName: profile.firstName,
          lastName: profile.lastName,
          phoneNumber: profile.phoneNumber ?? '',
        });
        this.photoUrl.set(profile.photoUrl);
      }
    });
  }

  onPhotoSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.uploadingPhoto.set(true);
    this.uploadsService.uploadAvatar(file).subscribe({
      next: (uploaded) => {
        this.uploadingPhoto.set(false);
        this.photoUrl.set(uploaded.url);
      },
      error: () => this.uploadingPhoto.set(false),
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    this.saving.set(true);
    this.profileService
      .updateMyProfile({
        firstName: v.firstName,
        lastName: v.lastName,
        phoneNumber: v.phoneNumber || null,
        photoUrl: this.photoUrl(),
      })
      .subscribe({
        next: (profile) => {
          this.saving.set(false);
          this.auth.updateCachedName(profile.firstName, profile.lastName);
          this.snackBar.open('Profile updated.', 'Dismiss', { duration: 3000 });
        },
        error: () => this.saving.set(false),
      });
  }
}
