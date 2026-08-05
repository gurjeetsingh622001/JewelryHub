import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then((m) => m.RegisterComponent),
  },
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
  { path: '**', redirectTo: '' },
];
