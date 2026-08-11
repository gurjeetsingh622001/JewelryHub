import { CurrencyPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { AdminService } from '../admin.service';

@Component({
  selector: 'app-admin-dashboard',
  imports: [RouterLink, MatProgressSpinnerModule, LucideAngularModule, CurrencyPipe],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
})
export class AdminDashboardComponent {
  private readonly adminService = inject(AdminService);

  protected readonly dashboardResource = rxResource({
    stream: () => this.adminService.getDashboard(),
  });
}
