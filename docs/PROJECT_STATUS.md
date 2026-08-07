# Project Status

Last updated: **2026-08-07** (Angular Union module — core scope: browse/create/join, membership
management, officer-gated announcements/documents/events — verified live). This file, along with
the rest of
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
- ✔ **Fixed a real `AddToCartCommand` bug (2026-08-06)** — it loaded `Product` via a no-tracking
  query then attached it to a new `CartItem` saved through the same tracked `DbContext`, so EF
  Core tried to `INSERT` the already-existing product again (primary-key violation, 500 on every
  add-to-cart call). Found the moment the Angular Cart feature was tested against a real database
  — see [API_PROGRESS.md](API_PROGRESS.md). Fixed by loading it `QueryTracking()` instead.
- ✔ **`CustomerAddressesController` added (2026-08-06)** — closed a hard blocker: `CreateOrderCommand`
  requires a `ShippingAddressId`/`BillingAddressId` that must already exist, but no endpoint
  anywhere let a customer create one. Full CRUD (create/update/soft-delete/list), plus made
  `CustomerAddress` actually implement `ISoftDelete` (a comment elsewhere had assumed it already
  did) — new migration `AddCustomerAddressSoftDelete`. See [API_PROGRESS.md](API_PROGRESS.md) and
  [DATABASE.md](DATABASE.md) for why this entity deliberately has no global query filter.
- ✔ **Fixed a real `UpdateOrderItemStatusCommand` bug (2026-08-06)** — marking an order item
  `Shipped` threw a `DbUpdateConcurrencyException` (500) on every call, because the new `Shipment`
  was attached to the tracked `OrderItem` only via navigation assignment and EF Core's change
  tracker mistook its client-generated `Guid` key for an existing entity, issuing an `UPDATE`
  instead of an `INSERT`. Found live while building the Angular Seller Order Fulfillment queue —
  see [API_PROGRESS.md](API_PROGRESS.md). Fixed by adding a `Shipments` repository to
  `IUnitOfWork` and calling `AddAsync` explicitly.

**Frontend**

- ✔ Angular 22 workspace at `client/` — standalone components, signals, Angular Material +
  Tailwind CSS (see [FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md); PrimeNG was tried and removed —
  the installed version requires a paid license, see below)
- ✔ Auth foundation: login + register (Customer/Seller toggle, now `MatButtonToggleGroup`) pages,
  `AuthService` (JWT + refresh-token session state via signals), HTTP interceptors (Bearer-token
  attach, dedup'd 401-refresh-and-retry, global `MatSnackBar` error toast), `authGuard`/
  `roleGuard`, app shell with login/logout UI
- ✔ Permanent design system (`docs/DESIGN_SYSTEM.md`) — premium editorial jewelry-boutique
  direction (warm ivory/gold/charcoal palette, Playfair Display + Inter type, Lucide icons,
  curated Unsplash imagery), plus the Home page built to it: Navbar (announcement strip, sticky
  nav, mobile off-canvas panel), Footer (trust signals, sitemap, newsletter), hero, category grid,
  featured pieces, brand story
- ✔ **PrimeNG removed (2026-08-05).** The installed version (22.x) requires a paid PrimeUI license
  or it injects an "Invalid PrimeUI License" banner into every page (a real, cryptographically-
  verified gate, not a bug — found via an actual browser check, not just a build). Uninstalled
  entirely; every usage (`Toast`, `SelectButton`) replaced with a Material equivalent
  (`MatSnackBar`, `MatButtonToggleGroup`).
- ✔ Login/Register redesigned to the design system via a shared `AuthLayoutComponent`
  (split-screen photo+quote / centered form), and moved to top-level routes outside the storefront
  Shell — no Navbar/Footer on auth pages by design. Fixed a real architecture gap in the process:
  `App` had been rendering Shell unconditionally, so every route got the same chrome regardless of
  whether it belonged there.
- ✔ Real product browsing (list + detail), wired to the live Products/Categories APIs — URL-driven
  filters (search/category/metal/price/sort/pagination) via `rxResource`, a reusable
  `ProductCardComponent`, image gallery + spec table on the detail page.
- ✔ **First slice verified against a genuinely live backend + database (2026-08-05).** Registered
  a seller, ran it through the KYC approval flow as the dev admin, created a category and three
  real products via the live API, then confirmed the Product List/Detail pages render everything
  correctly — filters, category dropdown, discount badges, gallery — with zero console errors.
- ✔ Real shopping cart (2026-08-06): `CartService` (signals, auto-refresh on auth change), a live
  item-count badge in the Navbar, a quantity stepper + real Add to Cart on Product Detail, and a
  `/cart` page (line items, quantity update, remove, subtotal, empty state). Verified live
  end-to-end (add → badge → view → update → remove) with a freshly-registered customer — this run
  is what surfaced the `AddToCartCommand` backend bug above, plus a frontend one (three Lucide
  icons used in the new UI were never added to the app's icon registry, so they silently failed to
  render — fixed). Login/Register have not yet been round-tripped live.
- ✔ Full checkout flow (2026-08-06): address book (select saved / add new inline), payment method
  selection, place order, auto-confirm payment (stands in for a real gateway webhook — every
  method except Cash on Delivery), and an order confirmation/detail page. Verified live end to
  end with a real customer: add to cart → checkout → order confirmed with the correct items,
  address, payment, and cost breakdown (including the free-shipping threshold and $0 tax, both
  correctly reflecting existing, documented backend behavior, not bugs) → cart empties. Zero
  console errors.
- ✔ Demo data expanded (2026-08-06): a second seller, 3 more categories (Necklaces/Earrings/
  Bracelets), 4 more products (7 total across 2 sellers/4 categories), and — from Checkout
  verification — a test customer with a saved address and one placed, payment-confirmed order.
  Kept intentionally as ongoing sample data per the user's decision, not cleaned up.
- ✔ Seller module (2026-08-06): a `/seller` area (role-guarded) with a Dashboard (KYC status
  banner, revenue/orders/rating stats, quick links), a KYC page (submit a document, see status of
  previously submitted ones), My Products (list + a combined create/edit form covering pricing,
  metal/purity/weight, hallmark, and a plain inventory-adjust control), and an Order Fulfillment
  queue (status filter, advance Confirmed → Processing → Shipped → Delivered, with an inline
  Carrier/Tracking Number form for the Shipped step since the backend requires both). Verified
  live end-to-end with a real seller account: registered → KYC submitted → admin-approved → listed
  a product → a test customer bought it → advanced it through every fulfillment status → the
  Dashboard's revenue/orders-fulfilled stats updated correctly. This run is what surfaced the
  `UpdateOrderItemStatusCommand` backend bug above — fixed and re-verified with zero console
  errors. Also fixed two smaller frontend bugs found in the process: a `GET /products/null` request
  on the "new product" route (an `rxResource` `null`-vs-`undefined` params pitfall) and the KYC
  form showing every required field as invalid immediately after a successful submit (`form.reset()`
  doesn't clear `FormGroupDirective`'s submitted flag — fixed via `resetForm()`).
- ✔ Order History + Reviews (2026-08-07): a paginated `/orders` list (`order-history/`) linking
  into the existing order-detail page, plus a full Reviews slice — a "Write a Review" inline form
  on any `Delivered` order item (star ratings for product and seller, optional title/comment) and
  a Customer Reviews section on Product Detail. Verified live: reviewed a delivered item →
  appeared on the product page with the correct rating → re-reviewing the same item correctly
  surfaced the backend's real "already reviewed" error via the global error interceptor (there's
  no server-side "already reviewed" flag on `OrderItemDto`, so the Write-a-Review button hides
  itself locally after a successful submit but reappears on a fresh page load — a documented v1
  simplification, not a bug). **Found and fixed a real CSS bug**: Lucide's `Star` icon is
  stroke-only by default, so recoloring it gold via CSS `color` alone left every "filled" star
  looking identical to an "unfilled" one — fixed with `::ng-deep svg { fill: currentColor }`.
- ✔ Union module — core scope (2026-08-07): a public `/unions` directory (search, admin-approved
  only), `/unions/new` (Seller-only creation — founder becomes President, awaits admin approval),
  `/unions/mine` (a seller's own memberships), and `/unions/:id` (tabbed detail — Overview/Members/
  Announcements/Documents/Events) with officer-gated actions (create announcement/document/event,
  approve/reject pending membership requests) shown only to the union's own President/Vice
  President/Secretary. **Meetings and Governance Polls are explicitly out of scope for this pass**
  — a deliberate, user-approved scope cut to keep this sized like the Seller module; their backend
  is already complete and unconsumed by any frontend, see [ROADMAP.md](ROADMAP.md) Phase 16a.
  **Found and fixed a real bug**: three of the four detail-page tabs call `[Authorize]` (not
  anonymous) endpoints, but the page fetched all of them unconditionally — an anonymous visitor
  therefore got legitimate 401s that then triggered a spurious token-refresh attempt for a session
  that never existed. Fixed by gating those fetches on `auth.isAuthenticated()`. Verified live
  end-to-end (create → admin-approve → second seller joins → founder approves the request →
  publishes an announcement, uploads a document, schedules an event), zero console errors for both
  a signed-in officer and an anonymous visitor.

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

- ❌ Admin UI
- ❌ Union Meetings + Governance Polls UI (core Union UI is done — see Completed above and
  [ROADMAP.md](ROADMAP.md) Phase 16a)

(The full Customer UI — Home, auth, product browsing, cart, checkout, order history, reviews —
the full Seller UI, and the core Union UI are now done, see Completed above. The Angular
foundation these are built on is described in [FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md).)

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
