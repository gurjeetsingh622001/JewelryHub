import { Component, inject } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';
import { UnionsService } from '../../unions/unions.service';

@Component({
  selector: 'app-admin-pending-unions',
  imports: [RouterLink, MatButtonModule, MatProgressSpinnerModule, LucideAngularModule],
  templateUrl: './admin-pending-unions.component.html',
  styleUrl: './admin-pending-unions.component.scss',
})
export class AdminPendingUnionsComponent {
  private readonly unionsService = inject(UnionsService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly unionsResource = rxResource({
    stream: () => this.unionsService.getPendingUnions(1, 50),
  });

  approve(unionId: string): void {
    this.unionsService.approveUnion(unionId).subscribe({
      next: () => {
        this.snackBar.open('Union approved.', 'Dismiss', { duration: 4000 });
        this.unionsResource.reload();
      },
    });
  }
}
