import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../../core/auth/auth.service';
import { UnionFilters } from '../models';
import { UnionsService } from '../unions.service';

const PAGE_SIZE = 12;

@Component({
  selector: 'app-union-list',
  imports: [RouterLink, FormsModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, LucideAngularModule],
  templateUrl: './union-list.component.html',
  styleUrl: './union-list.component.scss',
})
export class UnionListComponent {
  protected readonly auth = inject(AuthService);
  private readonly unionsService = inject(UnionsService);

  // searchInput is the editable box; committedSearch only changes on submit,
  // so results don't refetch on every keystroke.
  protected readonly searchInput = signal('');
  protected readonly committedSearch = signal('');
  protected readonly pageNumber = signal(1);

  protected readonly filters = computed<UnionFilters>(() => ({
    search: this.committedSearch() || undefined,
    pageNumber: this.pageNumber(),
    pageSize: PAGE_SIZE,
  }));

  protected readonly unionsResource = rxResource({
    params: this.filters,
    stream: ({ params }) => this.unionsService.getUnions(params),
  });

  submitSearch(): void {
    this.committedSearch.set(this.searchInput());
    this.pageNumber.set(1);
  }

  goToPage(page: number): void {
    this.pageNumber.set(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
}
