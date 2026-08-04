import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-home',
  template: `
    <div class="flex flex-col gap-2">
      <h1 class="text-2xl font-medium">Welcome{{ auth.currentUser()?.firstName ? ', ' + auth.currentUser()?.firstName : '' }}</h1>
      <p class="text-sm opacity-80">
        This is a placeholder landing page — the storefront/dashboard screens for each role
        (Customer, Seller, Admin, Union) are built on top of this foundation next.
      </p>
    </div>
  `,
})
export class HomeComponent {
  protected readonly auth = inject(AuthService);
}
