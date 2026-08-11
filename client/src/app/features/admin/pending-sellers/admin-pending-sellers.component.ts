import { Component, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { DOCUMENT_STATUS_LABELS, DocumentVerificationStatus } from '../../seller/models';
import { SellerService } from '../../seller/seller.service';

@Component({
  selector: 'app-admin-pending-sellers',
  imports: [FormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, LucideAngularModule],
  templateUrl: './admin-pending-sellers.component.html',
  styleUrl: './admin-pending-sellers.component.scss',
})
export class AdminPendingSellersComponent {
  private readonly sellerService = inject(SellerService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly documentStatusLabels = DOCUMENT_STATUS_LABELS;
  protected readonly DocumentVerificationStatus = DocumentVerificationStatus;

  protected readonly rejectingSellerId = signal<string | null>(null);
  protected readonly rejectReason = signal('');

  protected readonly sellersResource = rxResource({
    stream: () => this.sellerService.getPendingSellers(1, 50),
  });

  reviewDocument(documentId: string, decision: DocumentVerificationStatus): void {
    this.sellerService.reviewDocument(documentId, decision).subscribe({
      next: () => {
        this.snackBar.open('Document reviewed.', 'Dismiss', { duration: 3000 });
        this.sellersResource.reload();
      },
    });
  }

  approveSeller(sellerId: string): void {
    this.sellerService.approveSeller(sellerId).subscribe({
      next: () => {
        this.snackBar.open('Seller approved.', 'Dismiss', { duration: 4000 });
        this.sellersResource.reload();
      },
    });
  }

  startReject(sellerId: string): void {
    this.rejectingSellerId.set(sellerId);
    this.rejectReason.set('');
  }

  confirmReject(sellerId: string): void {
    const reason = this.rejectReason().trim();
    if (!reason) return;

    this.sellerService.rejectSeller(sellerId, reason).subscribe({
      next: () => {
        this.rejectingSellerId.set(null);
        this.snackBar.open('Seller rejected.', 'Dismiss', { duration: 4000 });
        this.sellersResource.reload();
      },
    });
  }
}
