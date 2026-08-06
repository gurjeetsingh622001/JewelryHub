import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { SellerService } from '../seller.service';
import { SELLER_STATUS_LABELS, SellerVerificationStatus } from '../models';

@Component({
  selector: 'app-seller-dashboard',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, LucideAngularModule, CurrencyPipe],
  templateUrl: './seller-dashboard.component.html',
  styleUrl: './seller-dashboard.component.scss',
})
export class SellerDashboardComponent {
  private readonly sellerService = inject(SellerService);

  protected readonly statusLabels = SELLER_STATUS_LABELS;
  protected readonly VerificationStatus = SellerVerificationStatus;

  protected readonly profileResource = rxResource({
    stream: () => this.sellerService.getMyProfile(),
  });

  isApproved(status: SellerVerificationStatus): boolean {
    return status === SellerVerificationStatus.Approved;
  }
}
