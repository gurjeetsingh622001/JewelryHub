import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

/** Blocks a route unless the user is logged in, redirecting to /login otherwise. */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }
  return router.createUrlTree(['/login']);
};

/**
 * Blocks a route unless the user holds one of the given roles (matches the
 * backend's [Authorize(Roles = "...")] convention — Customer/Seller/Admin).
 * Usage: canActivate: [roleGuard(['Seller', 'Admin'])].
 */
export function roleGuard(allowedRoles: string[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      return router.createUrlTree(['/login']);
    }
    if (allowedRoles.some((role) => authService.hasRole(role))) {
      return true;
    }
    return router.createUrlTree(['/forbidden']);
  };
}
