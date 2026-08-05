import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { extractErrorMessage, ProblemDetails } from './problem-details';

/**
 * Last-resort global error surface (a Material snack bar) for any request a
 * component didn't handle itself. 401s are deliberately left alone here —
 * authInterceptor already owns the refresh-and-retry flow for those, and
 * showing a toast for a 401 that's about to be silently retried would just
 * flash a false error at the user.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status !== 401) {
        const problem = error.error as ProblemDetails | undefined;
        snackBar.open(extractErrorMessage(problem, error.message), 'Dismiss', {
          duration: 6000,
          panelClass: 'app-snackbar--error',
        });
      }
      return throwError(() => error);
    }),
  );
};
