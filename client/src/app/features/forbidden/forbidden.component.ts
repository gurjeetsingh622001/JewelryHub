import { Component } from '@angular/core';

@Component({
  selector: 'app-forbidden',
  template: `
    <div class="flex flex-col gap-2">
      <h1 class="text-2xl font-medium">403 — Access denied</h1>
      <p class="text-sm opacity-80">Your account doesn't have permission to view this page.</p>
    </div>
  `,
})
export class ForbiddenComponent {}
