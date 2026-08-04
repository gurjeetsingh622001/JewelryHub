# Project Status

Last updated: **2026-08-04** (Jewelry Union module + Orders shipping/rollup TODOs resolved +
Angular frontend foundation). This file, along with the rest of
`docs/`, is the project's
permanent memory — it should always reflect the actual state of the repository, independent of
any chat history. See the Maintenance Rule at the bottom.

## Completed

**Backend**

- ✔ Solution architecture (Clean Architecture, 5 projects — see [ARCHITECTURE.md](ARCHITECTURE.md))
- ✔ Domain entities and relationships (14 aggregates, 41 tables' worth of entities)
- ✔ EF Core configurations and DbContext
- ✔ Repository Pattern + Unit of Work
- ✔ Dependency Injection (per-layer composition)
- ✔ Authentication (JWT access/refresh with rotation, BCrypt hashing)
- ✔ Authorization (role-based, `[Authorize(Roles=...)]`)
- ✔ Auth API (register customer/seller, login, refresh, logout)
- ✔ Sellers API (profile, KYC document submission + admin review + approve/reject)
- ✔ Catalog API (Categories CRUD, Products CRUD + inventory adjustment)
- ✔ Cart API
- ✔ Wishlist API
- ✔ Orders API (checkout, retrieval, cancellation, seller fulfillment queue, item status),
  including per-seller shipping calculation (`IShippingCalculator`) and order-completion rollup
  (parent `Order` auto-transitions to `Delivered` once every item is, seller revenue/count
  rollups update per delivered item)
- ✔ Reviews API (create, seller response, admin moderation)
- ✔ Notifications API + real-time push (SignalR)
- ✔ Jewelry Union API: union creation + admin approval, membership (join/review/roles/removal),
  officer-gated announcements/documents/events, meetings with agenda items, RSVP, minutes and
  action items, and governance polls with voting (33 endpoints across `UnionsController`,
  `UnionMeetingsController`, `UnionPollsController` — see [API_PROGRESS.md](API_PROGRESS.md))
- ✔ Initial EF Core migration (`InitialCreate`, generated 2026-08-04 — the app can now create its
  schema; previously `Database.MigrateAsync()` had nothing to apply)
- ✔ Fixed 4 files with a build-breaking `RefreshToken` namespace/type collision, a missing
  `using Microsoft.EntityFrameworkCore;` in `UnitOfWork.cs`, an incorrect `IsRowVersion()` call
  site, and a missing ASP.NET Core `FrameworkReference` in Infrastructure — the solution did not
  compile before this pass; it now builds with 0 warnings/errors.
- ✔ Documentation set (this `docs/` folder)

**Frontend**

- ✔ Angular 22 workspace at `client/` — standalone components, signals, Angular Material +
  PrimeNG + Tailwind CSS combined (see [FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md) for why/how
  they coexist)
- ✔ Auth foundation: login + register (Customer/Seller toggle) pages, `AuthService` (JWT +
  refresh-token session state via signals), HTTP interceptors (Bearer-token attach, dedup'd
  401-refresh-and-retry, global error toast), `authGuard`/`roleGuard`, app shell with
  login/logout UI
- ✔ Verified: production build clean (0 warnings after budget adjustment), unit-test smoke
  passes (confirms the full DI graph — Router, HttpClient, AuthService, Material, PrimeNG —
  resolves at runtime), dev server boots cleanly. **Not** yet tested against a live backend (no
  database available in the environment this was built in).

## In Progress

Nothing is actively mid-implementation as of this pass — the backend foundation described above
is stable and builds cleanly. The next work is genuinely *new* work, not a resumption of
something half-written (see [ROADMAP.md](ROADMAP.md) for what's next).

## Pending

**Backend**

- ⏳ Real payment gateway integration (Razorpay/Stripe) — `ConfirmPaymentCommand` logic exists but
  nothing calls it from a real provider yet
- ⏳ Tax rate management API (currently seed/DB-edit only; applied at checkout but not manageable)
- ⏳ Password reset / email verification for Auth
- ⏳ Admin dashboard / reporting endpoints (today: role-gated actions only, no dedicated module)
- ⏳ Unit Tests, Integration Tests, Architecture Tests (explicitly deferred — see below)
- ⏳ CI/CD pipeline (`.github/workflows/` exists but is empty — explicitly deferred)

**Frontend**

- ❌ Customer UI (product browsing, cart, checkout, order history, reviews)
- ❌ Seller UI (dashboard, product/inventory management, fulfillment queue, KYC submission)
- ❌ Admin UI
- ❌ Union UI

(The Angular foundation exists — see above and [FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md) —
but no feature screens are built on top of it yet.)

## Explicit Non-Priorities (by decision, not oversight)

- **Testing** — deliberately deferred until after functionality is complete. Do not spend time on
  unit/integration/architecture tests until told otherwise.
- **CI/CD** — deliberately deferred. No GitHub Actions workflow is required right now.

## Maintenance Rule

Whenever a feature is completed, update, in this order:

1. `PROJECT_STATUS.md` (this file)
2. `ROADMAP.md`
3. `CHANGELOG.md`
4. `API_PROGRESS.md` (if any API changed)
5. `DATABASE.md` (if the schema changed — regenerate the table/relationship inventory from the
   new migration)

These five files must always reflect the current state of the codebase, so that development can
resume correctly even if chat history is lost.
