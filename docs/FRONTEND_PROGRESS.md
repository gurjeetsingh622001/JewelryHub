# Frontend Progress

## Status: Foundation Complete, No Feature Modules Yet

Last updated 2026-08-04. An Angular workspace now exists at `client/` (sibling to `src/`, not
part of `JewelryHub.sln` since it isn't a .NET project). The auth flow, HTTP layer, and routing
shell are built and verified (production build clean, unit-test smoke passes, dev server boots).
No feature screens (product browsing, cart, seller dashboard, etc.) exist yet — see
[ROADMAP.md](ROADMAP.md) Phase 13 onward.

## Stack

- **Angular 22**, standalone components (no NgModules), signals for local/service state.
- **UI**: Angular Material (forms, buttons, toolbar, cards, menu) **+** PrimeNG (used where
  Material has no equivalent — `SelectButton` on the register page, `Toast`/`MessageService` for
  global error notifications) **+** Tailwind CSS v4 for layout/spacing utilities. Tailwind's
  Preflight base-reset is deliberately excluded (`styles.scss` imports `tailwindcss/theme` +
  `tailwindcss/utilities` separately, not the `tailwindcss` shorthand) so it doesn't fight
  Material's and PrimeNG's own base styles — Tailwind is utility-only here, not the app's CSS
  reset. All three were an explicit choice for this project, not a default; expect to normalize
  toward fewer libraries once real screens reveal which one actually carries most of the UI.
- **HTTP**: `provideHttpClient` with two functional interceptors, `authInterceptor` (attaches the
  Bearer token, catches a 401, refreshes once via a shared/deduped `Observable` so concurrent
  401s don't each trigger their own `/auth/refresh` call, retries the original request) and
  `errorInterceptor` (last-resort PrimeNG toast for any error a component didn't handle itself;
  skips 401s since authInterceptor already owns that flow).
- **Auth storage**: access + refresh tokens and the current user in `localStorage` via
  `TokenStorageService` (the only thing allowed to touch it directly) — not an httpOnly cookie,
  because the backend's `/auth/refresh` expects the raw refresh token in the request body, not a
  cookie. This is a plain SPA-calls-API setup, not a BFF.
- **Route guards**: `authGuard` (must be logged in) and `roleGuard(['Seller', 'Admin'])` (must
  hold one of the given roles — mirrors the backend's `[Authorize(Roles = "...")]` convention).

## What's Built

- `core/auth/` — `models.ts` (mirrors `AuthResponse`/`RegisterCustomerCommand`/
  `RegisterSellerCommand` field-for-field, since ASP.NET Core's default JSON settings serialize
  camelCase and need no mapping layer), `token-storage.service.ts`, `auth.service.ts` (signals:
  `currentUser`, `isAuthenticated`, `roles`), `auth.guard.ts`, `auth.interceptor.ts`.
- `core/http/` — `problem-details.ts` (the RFC 7807 shape `ExceptionHandlingMiddleware` returns),
  `error.interceptor.ts`.
- `core/layout/shell.component.ts` — toolbar with login/register links or an account menu +
  logout, wraps `<router-outlet>`.
- `features/auth/login/`, `features/auth/register/` (a single page with a `SelectButton` toggling
  between the Customer and Seller forms — matches the backend's two separate registration
  commands), `features/home/` and `features/forbidden/` as placeholders.
- `environments/` — `apiUrl` pointing at `https://localhost:65334/api/v1` in development
  (matches `src/JewelryHub.API/Properties/launchSettings.json`), a relative `/api/v1` default for
  production (assumes a reverse-proxied deployment).

## Not Yet Wired Up / Known Gaps

- **Not tested against a live backend** — no SQL Server is available in the environment this was
  built in, so the login/register flow has been verified by build + a DI-wiring smoke test, not
  by an actual round trip. Worth a real end-to-end pass once a database is available.
- No password-reset/email-verification screens (the backend doesn't have these endpoints either —
  see [API_PROGRESS.md](API_PROGRESS.md)).
- No global loading indicator, no HTTP retry/offline handling.
- Testing is a placeholder smoke test only (`app.spec.ts`) — matches the backend's stance of
  deferring real test coverage until more functionality exists.

## Per-Module Frontend Status

| Module | Status |
|---|---|
| Foundation (auth, routing, HTTP, shell) | ✅ Complete |
| Customer UI | ❌ Missing |
| Seller UI | ❌ Missing |
| Admin UI | ❌ Missing |
| Union UI | ❌ Missing |

## How to Run

```bash
cd client
npm install   # already run once during scaffolding; re-run after pulling new commits
npm start     # ng serve — http://localhost:4200
```

Requires the API running at the URL in `src/environments/environment.development.ts`
(`https://localhost:65334` by default) with CORS allowing `http://localhost:4200` — already the
default in `appsettings.json`.

## Maintenance

Update this file whenever a feature module moves from ❌ to 🚧/✅, or the stack/library choices
change.
