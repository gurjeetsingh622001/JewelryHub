// Shape of the RFC 7807 response JewelryHub.API's ExceptionHandlingMiddleware
// produces for every error — NotFoundException (404), BusinessRuleException /
// FluentValidation.ValidationException (400), ForbiddenAccessException (403),
// AuthenticationFailedException (401), and the generic 500 fallback all land
// here. `errors` is only present for the FluentValidation 400 shape
// (grouped-by-property).
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}

export function extractErrorMessage(problem: ProblemDetails | null | undefined, fallback: string): string {
  if (!problem) {
    return fallback;
  }
  if (problem.errors) {
    const firstField = Object.values(problem.errors)[0];
    if (firstField?.length) {
      return firstField[0];
    }
  }
  return problem.detail ?? problem.title ?? fallback;
}
