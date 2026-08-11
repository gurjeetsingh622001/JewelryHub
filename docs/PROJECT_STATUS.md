# Project Status

Last updated: **2026-08-11** (post-v1 polish pass: Wishlist UI, a generic file-upload feature for
product photos/KYC documents, two real `DbUpdateConcurrencyException` bugs found and fixed in Cart
and Wishlist, and a large live demo-data generation pass). This file, along with the rest of
`docs/`, is the project's permanent memory — it should always reflect the actual state of the
repository, independent of any chat history. See the Maintenance Rule at the bottom.

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
- ✔ **Admin API added (2026-08-11)** — a dedicated `Features/Admin` folder + `AdminController`
  (`api/v1/admin`, class-level `[Authorize(Roles = "Admin")]`): `GET dashboard` (platform-wide
  counts), `GET users` + `POST users/{id}/status` (the first-ever Application-layer User listing/
  activate-deactivate, with a guard against self-deactivation), and `GET reviews` (an unfiltered
  moderation browsing queue). Researched the codebase's existing convention first (a resource's
  admin queue query lives on that resource's own controller, not a central one) and deliberately
  kept seller/union approval and review moderation where they already lived, adding only what had
  no existing home. See [API_PROGRESS.md](API_PROGRESS.md) and [BACKEND_PROGRESS.md](BACKEND_PROGRESS.md).
- ✔ **Fixed a real `AddToCartCommand` bug, second occurrence (2026-08-11)** — reported live as a
  500 on every add-to-cart call. Root cause was the *same shape* as the Shipment bug above but in a
  different handler: a brand-new `CartItem` was attached only via `cart.Items.Add(...)` navigation,
  never through an explicit repository `AddAsync`, so EF Core's change tracker treated its
  client-generated `Guid` key ambiguously and issued an `UPDATE` matching 0 rows instead of an
  `INSERT`. Fixed identically — added a `CartItems` repository to `IUnitOfWork` and call `AddAsync`
  explicitly. Verified live: both "brand-new cart" and "existing cart, new product" paths now
  return 200. See [API_PROGRESS.md](API_PROGRESS.md).
- ✔ **Found and fixed the same bug a third time, in `AddToWishlistCommand` (2026-08-11)** — caught
  during live verification of the new Wishlist UI (see below), not reported by the user this time.
  Identical root cause and identical fix (`WishlistItems` repository + explicit `AddAsync`). Also
  swept the rest of `Features/` for the same `collection.Add(new Entity {...})`-before-any-explicit-
  `Add` shape — every other instance (`RegisterCustomerCommand`, `RegisterSellerCommand`,
  `CreateProductCommand`'s Gemstones/Images) attaches children *before* the parent's own explicit
  `AddAsync` call, which is the safe ordering, so no further instances exist.
- ✔ **Generic file-upload feature added (2026-08-11)** — a new `Features/Uploads` slice +
  `UploadsController` (`api/v1/uploads`, `Seller,Admin` only): `POST product-images` and
  `POST kyc-documents`, both validating file size (5 MB) and extension, saving to local disk under
  the API's `wwwroot/uploads/` (new `IFileStorageService`/`LocalFileStorageService` in
  Infrastructure — swappable for cloud blob storage later without touching any handler), served
  back out via `app.UseStaticFiles()`. Existing commands (`CreateProductCommand`,
  `SubmitSellerDocumentCommand`) were untouched — they already just take a URL string, so uploading
  is a separate client-side pre-step, not baked into those commands. See
  [API_PROGRESS.md](API_PROGRESS.md).

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
- ✔ Union Meetings + Governance Polls (2026-08-07) — the deferred Phase 16a piece, closing out the
  full Union module. Two new tabs on `/unions/:id` (Meetings, Polls, gated on active membership),
  plus `/unions/:unionId/meetings/new`+`/:meetingId` (schedule with a dynamic agenda list, RSVP,
  officer status controls, and an officer "Record Minutes" form with dynamic action items assigned
  to attendees) and `/unions/:unionId/polls/new`+`/:pollId` (create as Draft, a radio/checkbox
  ballot shown only while Active, live results, officer Open/Close). "My Action Items" added to
  `/unions/mine`. **Found and fixed a real layout bug**: the Record Minutes action-item row crammed
  three form fields into one flex row, squeezing the dropdown/date fields down to an unusable
  width — fixed with a responsive grid. Verification also caught two Playwright/Angular-Material
  interaction quirks (clicking a `mat-select`/`mat-radio-button`'s host element rather than its
  native `<input>` can silently no-op in headless Chromium) — confirmed via `aria-checked`/
  `outerHTML` inspection that these were test-tooling artifacts, not app bugs. Verified live
  end-to-end: scheduled a meeting → recorded minutes with an action item → a second member RSVP'd;
  created and opened a poll → a second member voted → results and re-vote rejection both correct →
  the assigned action item appeared under "My Action Items." Zero console errors.
- ✔ Admin module (2026-08-11) — the last remaining planned module. A role-guarded `/admin` area:
  a Dashboard (platform stat cards + quick links), Pending Sellers (per-document verify, seller
  approve/reject with an inline reason), Pending Unions (approve), Reviews (All/Flagged/Hidden
  filter, Hide/Restore), and Users (search + role filter, Deactivate/Reactivate). No new backend
  or frontend app bugs were found during verification this time — the only issues hit were two
  test-script bugs in the Playwright verification script itself (an ambiguous `button:has-text`
  selector matching the wrong one of two "Approve" buttons on the page, and a script that assumed
  a fixed starting state on re-runs), both diagnosed by cross-checking backend state directly via
  `curl` before concluding the app was correct. Verified live end-to-end as an admin (dashboard →
  verify a KYC document → approve the seller → hide then restore a review → deactivate then
  reactivate a user) and confirmed a non-admin (Seller) account is redirected away from `/admin`.
  Zero console errors on the final run.
- ✔ Wishlist UI (2026-08-11) — the Heart icon in the Navbar, and the "Add to Wishlist" buttons on
  `ProductCardComponent` and Product Detail, previously all showed an honest "coming soon" toast;
  now wired to the always-complete Wishlist API. New `WishlistService` (signals, same
  Customer-only lifecycle as `CartService`) and a `/wishlist` page (grid of saved items, Move to
  Cart, Remove, empty state). Wishlist state (filled heart, live navbar badge count) is reactive
  everywhere a product can be wishlisted. This is the work that surfaced the `AddToWishlistCommand`
  backend bug above — found live, not by the user, fixed and re-verified with zero console errors.
- ✔ Real image/document upload UI (2026-08-11) — the Seller product form's "Image URL" text field
  and the KYC form's "Document URL" text field (both previously required pasting an already-hosted
  URL) are now real file pickers backed by the new Uploads API, with an upload-in-progress spinner
  and a thumbnail/filename preview. New shared `resolveMediaUrl()` helper (`shared/`) resolves the
  backend's relative `/uploads/...` paths against the API's origin — needed because the Angular dev
  server and API run on different ports, so a bare relative path would otherwise resolve against
  the wrong origin; applied everywhere an image or KYC document link renders (product card/detail,
  cart, wishlist, the Admin pending-sellers document link). Verified live: uploaded a real product
  photo during creation (rendered correctly cross-origin), uploaded and submitted a real KYC
  document, confirmed the "must be JPG/PNG/WEBP" and "must be JPG/PNG/WEBP/PDF" validation messages
  and the Customer-role 403 all fire correctly. Zero console errors.
- ✔ Large demo-data generation pass (2026-08-11) — a scripted pass against the live API (not direct
  SQL) added 14 more KYC-approved sellers, 18 more customers, 131 more products spread across the 4
  existing categories and varied metal/purity/price/discount combinations, and 5 more admin-approved
  unions with 3-7 members each recruited from the new sellers. Brings the dev database to 32
  customers, 22 approved sellers, 142 products, and 6 unions. Zero errors across the full run;
  spot-checked live in the browser (product list pagination, category filter, union directory,
  Admin dashboard counts) with zero console errors. Kept as ongoing sample data, same as every
  other demo account in this project.

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
- ⏳ Role/permission management API and deeper reporting/analytics beyond the Admin dashboard's
  basic counts (the dashboard/user-management/review-queue itself is done, see Completed above)
- ⏳ Unit Tests, Integration Tests, Architecture Tests (explicitly deferred — see below)
- ⏳ CI/CD pipeline (`.github/workflows/` exists but is empty — explicitly deferred)

**Frontend**

Nothing planned is outstanding. The full Customer UI — Home, auth, product browsing, cart,
checkout, order history, reviews — the full Seller UI, the full Union UI (including Meetings and
Governance Polls), and now the full Admin UI are all done, see Completed above. The Angular
foundation these are built on is described in [FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md). Any
further frontend work needs fresh direction, not a resumption of planned scope.

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
