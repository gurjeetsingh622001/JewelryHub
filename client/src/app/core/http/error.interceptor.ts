import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { catchError, throwError } from 'rxjs';
import { extractErrorMessage, ProblemDetails } from './problem-details';

/**
 * Last-resort global error surface (a PrimeNG toast) for any request a
 * component didn't handle itself. 401s are deliberately left alone here —
 * authInterceptor already owns the refresh-and-retry flow for those, and
 * showing a toast for a 401 that's about to be silently retried would just
 * flash a false error at the user.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const messageService = inject(MessageService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status !== 401) {
        const problem = error.error as ProblemDetails | undefined;
        messageService.add({
          severity: 'error',
          summary: problem?.title ?? 'Something went wrong',
          detail: extractErrorMessage(problem, error.message),
          life: 6000,
        });
      }
      return throwError(() => error);
    }),
  );
};
