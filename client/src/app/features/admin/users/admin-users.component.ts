import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { AdminService } from '../admin.service';
import { UserFilters } from '../models';

@Component({
  selector: 'app-admin-users',
  imports: [FormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatProgressSpinnerModule, LucideAngularModule, DatePipe],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss',
})
export class AdminUsersComponent {
  private readonly adminService = inject(AdminService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly searchInput = signal('');
  protected readonly committedSearch = signal('');
  protected readonly roleFilter = signal<string | undefined>(undefined);
  protected readonly updatingUserId = signal<string | null>(null);

  protected readonly roleOptions = [
    { value: undefined, label: 'All Roles' },
    { value: 'Customer', label: 'Customer' },
    { value: 'Seller', label: 'Seller' },
    { value: 'Admin', label: 'Admin' },
  ];

  protected readonly filters = computed<UserFilters>(() => ({
    search: this.committedSearch() || undefined,
    role: this.roleFilter(),
    pageNumber: 1,
    pageSize: 50,
  }));

  protected readonly usersResource = rxResource({
    params: this.filters,
    stream: ({ params }) => this.adminService.getUsers(params),
  });

  submitSearch(): void {
    this.committedSearch.set(this.searchInput());
  }

  toggleActive(userId: string, currentlyActive: boolean): void {
    this.updatingUserId.set(userId);
    this.adminService.setUserStatus(userId, !currentlyActive).subscribe({
      next: () => {
        this.updatingUserId.set(null);
        this.snackBar.open(currentlyActive ? 'Account deactivated.' : 'Account reactivated.', 'Dismiss', { duration: 4000 });
        this.usersResource.reload();
      },
      error: () => this.updatingUserId.set(null),
    });
  }
}
