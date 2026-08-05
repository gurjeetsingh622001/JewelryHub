import { Routes } from '@angular/router';
import { ShellComponent } from './core/layout/shell.component';

export const routes: Routes = [
  // Login/Register are deliberately full-page and outside the storefront
  // Shell — no Navbar/Footer inviting someone to wander off mid-signup,
  // same reasoning most premium retailers use for their auth pages.
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    // Everything else is the storefront proper — wrapped once in Shell
    // (Navbar + Footer) via its own nested <router-outlet>.
    path: '',
    component: ShellComponent,
    children: [
      {
        path: 'forbidden',
        loadComponent: () => import('./features/forbidden/forbidden.component').then((m) => m.ForbiddenComponent),
      },
      {
        // Public storefront home — a luxury retail site's landing page is
        // browsable without an account, same as Cartier/Tiffany/Blue Nile.
        // authGuard is reserved for account-specific pages (orders, wishlist).
        path: '',
        loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
