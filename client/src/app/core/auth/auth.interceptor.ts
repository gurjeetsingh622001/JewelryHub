import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

const PUBLIC_AUTH_PATHS = ['/auth/login', '/auth/register/customer', '/auth/register/seller', '/auth/refresh'];

/**
 * Attaches the access token to every outgoing API request, and on a 401
 * transparently refreshes once and retries — the caller never sees the
 * first failure. If the refresh itself fails (expired/revoked refresh
 * token), the session is cleared and the user is sent to /login.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isPublicAuthCall = PUBLIC_AUTH_PATHS.some((path) => req.url.includes(path));
  const token = authService.getAccessToken();
  const authedReq = token && !isPublicAuthCall ? withBearerToken(req, token) : req;

  return next(authedReq).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401 && !isPublicAuthCall) {
        return authService.refresh().pipe(
          switchMap((response) => next(withBearerToken(req, response.accessToken))),
          catchError((refreshError) => {
            authService.logout().subscribe();
            router.navigate(['/login']);
            return throwError(() => refreshError);
          }),
        );
      }
      return throwError(() => error);
    }),
  );
};

function withBearerToken(req: Parameters<HttpInterceptorFn>[0], token: string) {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}
