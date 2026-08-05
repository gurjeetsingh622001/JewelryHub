import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { LucideAngularModule } from 'lucide-angular';

@Component({
  selector: 'app-footer',
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
})
export class FooterComponent {
  private readonly snackBar = inject(MatSnackBar);
  protected readonly year = new Date().getFullYear();

  /** No newsletter endpoint exists on the backend yet — honest placeholder rather than a dead form submit. */
  notifyComingSoon(): void {
    this.snackBar.open('Newsletter sign-up is on its way.', 'Dismiss', { duration: 4000 });
  }
}
