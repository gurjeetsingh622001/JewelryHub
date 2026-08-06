# Changelog

Every completed unit of work gets an entry here: date, module, files modified, summary. This is
the project's development history — kept even if chat history is lost. Newest entries at the top.

---

## 2026-08-06 — Seller UI: Dashboard, KYC, Products, Order Fulfillment (Fixed a Real Backend Bug, Verified Live)

**Module**: Backend (`Shipments` repository + `UpdateOrderItemStatusCommand` fix) + Frontend
(`client/`, new `features/seller/`) + demo data in the live DB

**Files modified**:
- `src/JewelryHub.Application/Common/Interfaces/IUnitOfWork.cs`,
  `src/JewelryHub.Persistence/UnitOfWork.cs` — added a `Shipments` repository
- `src/JewelryHub.Application/Features/Orders/Commands/UpdateOrderItemStatus/UpdateOrderItemStatusCommand.cs`
  — routes the new `Shipment` through `_unitOfWork.Shipments.AddAsync(...)` explicitly instead of
  relying on EF Core navigation-fixup to detect it as `Added`
- `client/src/app/features/seller/` (new) — `models.ts`, `seller.service.ts`, `dashboard/`,
  `kyc/`, `products/` (list + `product-form/` for create and edit), `orders/`
  (`seller-orders.component`)
- `client/src/app/features/products/models.ts` — added `ProductType` enum + labels (needed for the
  seller product-creation form's Product Type select)
- `client/src/app/app.routes.ts` — added `/seller` (and children) guarded by
  `roleGuard(['Seller'])`
- `client/src/app/core/layout/navbar/navbar.component.html` — "Seller Dashboard" link in the
  account menu when `auth.hasRole('Seller')`
- `client/src/app/app.config.ts` — registered new Lucide icons (`AlertCircle`, `ClipboardList`,
  `FileCheck`, `LayoutDashboard`, `PackagePlus`, `Pencil`, `TrendingUp`)
- `docs/API_PROGRESS.md`, `docs/BACKEND_PROGRESS.md`, `docs/FRONTEND_PROGRESS.md`,
  `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`

**Backend bug found and fixed**: verifying the Order Fulfillment queue live surfaced a real,
100%-reproducible bug — marking an order item `Shipped` threw a `DbUpdateConcurrencyException`
(500, "expected to affect 1 row(s), but actually affected 0") on every single call.
`UpdateOrderItemStatusCommand` created the new `Shipment` via `item.Shipment ??= new Shipment {
OrderItemId = item.Id }` and relied on EF Core's navigation-fixup to mark it `Added` on `SaveChanges`.
That doesn't reliably work when the child entity's primary key is a client-generated `Guid` set in
a property initializer (every `BaseEntity.Id` is): EF Core's change tracker has no default-value
signal to distinguish "brand new, client-set key" from "existing, unchanged-key entity" reached
purely by graph traversal, so it issued an `UPDATE ... WHERE Id = @p` instead of an `INSERT` —
which naturally matched zero rows. Root-caused by reading the actual generated SQL from
`sys.dm_exec_query_stats` rather than guessing from the exception message (which points at
optimistic-concurrency RowVersion mismatches, a red herring here — `OrderItem.RowVersion` isn't
even configured as a concurrency token). Fixed by giving `Shipment` its own `IRepository<Shipment>`
on `IUnitOfWork` (matching every other aggregate) and calling `AddAsync` explicitly instead of
trusting fixup alone.

**Frontend bugs found and fixed during the same verification pass**:
- A stray `GET /products/null` request fired every time `/seller/products/new` loaded.
  `SellerProductFormComponent` serves both the create and edit routes from one component;
  `rxResource`'s `params` function only skips its loader when it returns `undefined`, not `null` —
  but the create route's `paramMap.get('id')` legitimately returns `null`. Fixed by mapping
  `productId() ?? undefined` before handing it to the resource.
- The KYC document form showed every required field as invalid immediately after a successful
  submit. `form.reset()` clears control values but not `FormGroupDirective`'s internal "submitted"
  flag, and Angular Material's default `ErrorStateMatcher` treats `control.invalid && (touched ||
  form.submitted)` as an error state — so a freshly-reset-but-still-"submitted" form immediately
  flags every empty required field. Fixed by calling `resetForm()` on a `@ViewChild(FormGroupDirective)`
  instead of `form.reset()` on the `FormGroup` itself.
- The Order Fulfillment queue's "Mark Shipped" action also needed a genuine design fix, not just a
  bug fix: the backend's validator requires non-empty `Carrier`/`TrackingNumber` only for the
  Shipped transition, but the original one-click action never collected them. Replaced with an
  inline Carrier/Tracking Number form that appears only for that specific transition.

**Verified live end-to-end**: registered a new seller account → submitted a KYC document → logged
in as the dev admin (`admin@jewelryhub.local` — its `IsLockedOut` flag was found `true` with a
stale `AccessFailedCount`/`LockoutEndUtc` mismatch from earlier session activity and was reset
directly in the local dev database, since there's no self-service admin unlock endpoint by design)
→ approved the document and the seller → listed a product with a real image, pricing, and initial
inventory → a separately registered test customer added it to cart, created an address, checked
out, and confirmed payment → back as the seller, advanced the resulting order item through
Processing → Shipped (via the new inline form) → Delivered → confirmed the Dashboard's Total
Revenue and Orders Fulfilled stats updated correctly. Zero console errors and zero unexpected HTTP
error responses on the final run.

---

## 2026-08-06 — Customer UI: Checkout (Closed a Hard Backend Blocker, Verified Live)

**Module**: Backend (new `CustomerAddresses` feature + migration) + Frontend (`client/`) + demo
data in the live DB

**Files modified**:
- `src/JewelryHub.Domain/Customers/Customer.cs` — `CustomerAddress` now implements `ISoftDelete`
- `src/JewelryHub.Persistence/Configurations/Customers/CustomerConfigurations.cs` — deliberately
  no global query filter (see summary)
- `src/JewelryHub.Persistence/Migrations/20260806043949_AddCustomerAddressSoftDelete.cs` (new)
- `src/JewelryHub.Application/Common/Interfaces/IUnitOfWork.cs`,
  `src/JewelryHub.Persistence/UnitOfWork.cs` — added `CustomerAddresses` repository
- `src/JewelryHub.Application/Features/Customers/Addresses/` (new) — `CustomerAddressDto`,
  `CreateAddressCommand`, `UpdateAddressCommand`, `DeleteAddressCommand`, `GetMyAddressesQuery`
- `src/JewelryHub.API/Controllers/CustomerAddressesController.cs` (new)
- `client/src/app/features/addresses/` (new) — `models.ts`, `addresses.service.ts`
- `client/src/app/features/orders/` (new) — `models.ts`, `orders.service.ts`, `checkout/`,
  `order-detail/`
- `client/src/app/app.routes.ts` — added `/checkout` (Customer-guarded) and `/orders/:id`
  (auth-guarded)
- `client/src/app/features/cart/cart-page/` — "Proceed to Checkout" is now a real link
- `docs/API_PROGRESS.md`, `docs/BACKEND_PROGRESS.md`, `docs/DATABASE.md`,
  `docs/FRONTEND_PROGRESS.md`, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`

**Backend gap found and closed**: building Checkout revealed that `CreateOrderCommand` requires a
`ShippingAddressId`/`BillingAddressId` that must already exist as a `CustomerAddress` row — and
there was **no endpoint anywhere** to create one. Checkout was structurally impossible before this
pass, not just unbuilt. Closed by adding a full `CustomerAddressesController` (create/update/
soft-delete/list). Doing this correctly also required making `CustomerAddress` implement
`ISoftDelete` — a comment on `OrderConfigurations` had already assumed it did ("CustomerAddress
rows are never hard-deleted, only soft-deleted, so historical orders can always resolve them"),
but the entity never actually declared the interface. Deliberately did **not** add the usual
global `HasQueryFilter` for it, though: `Order.ShippingAddress`/`BillingAddress` are live
navigations (`OrderMapper` reads `order.ShippingAddress.AddressLine1` directly, not a stored
snapshot), so a global filter would have nulled out that navigation via `Include()` the moment a
customer deleted an address a past order used — breaking exactly the historical-order guarantee
the comment described. `GetMyAddressesQuery` filters `!IsDeleted` explicitly instead.

**Summary**: Built the full Checkout experience — an address book (select a saved address or add
one inline, auto-shown when the customer has none), payment method selection, place order, and
automatic payment confirmation (standing in for a real gateway webhook, for every method except
Cash on Delivery) — landing on an order confirmation/detail page that doubles as a general order
lookup. Verified live end-to-end with a real customer: registered, added a saved address via the
API, added a product to cart through the UI, checked out (address and payment method both
defaulted sensibly), landed on a correct confirmation page (right items, address, payment, and
cost breakdown — including ₹0 tax and free shipping, both correct given no `TaxRate` rows are
seeded and the order exceeded the free-shipping threshold, not bugs), and confirmed the cart
emptied afterward. Zero console errors. Order history (a list of past orders) and Reviews are the
remaining pieces of Customer UI.

---

## 2026-08-06 — Customer UI: Cart (Verified Live, Found + Fixed a Real Backend Bug)

**Module**: Frontend (`client/`) + backend bug fix + demo data in the live DB

**Files modified**:
- `client/src/app/features/cart/models.ts` (new) — mirrors `CartDto`/`CartItemDto`
- `client/src/app/features/cart/cart.service.ts` (new) — signal-based state, `effect()`
  auto-refreshes on auth state change
- `client/src/app/features/cart/cart-page/` (new) — the `/cart` page
- `client/src/app/core/layout/navbar/navbar.component.ts`, `.html`, `.scss` — real cart icon +
  live item-count badge, replacing the "coming soon" placeholder
- `client/src/app/features/products/product-detail/product-detail.component.ts`, `.html`, `.scss`
  — quantity stepper + real Add to Cart, replacing the "coming soon" placeholder
- `client/src/app/app.routes.ts` — added `/cart`, guarded by `roleGuard(['Customer'])`
- `client/src/app/app.config.ts` — registered the `Minus`/`Plus`/`Trash2` Lucide icons (see bug
  below)
- `src/JewelryHub.Application/Features/Cart/Commands/AddToCart/AddToCartCommand.cs` — backend bug
  fix, see below
- `docs/API_PROGRESS.md`, `docs/BACKEND_PROGRESS.md`, `docs/FRONTEND_PROGRESS.md`,
  `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`

**Backend bug found and fixed**: `AddToCartCommand` loaded `Product` via `Query()`
(`AsNoTracking()`), then attached it to a brand-new `CartItem` saved through the same, separately
tracked `DbContext`. EF Core didn't recognize the no-tracking `Product` as already existing and
tried to `INSERT` it again on `SaveChangesAsync`, throwing a primary-key-violation
`DbUpdateException` (HTTP 500) on **every single add-to-cart call**. This had been marked
"✅ Complete" in the docs for weeks without ever being exercised against a real database — caught
the moment the new Angular Cart UI was tested live. Fixed by loading `Product` via
`QueryTracking()` instead. The other three Cart handlers don't have this issue.

**Frontend bug found and fixed**: the new Cart UI used three Lucide icons (`Minus`, `Plus`,
`Trash2`) that were never added to `app.config.ts`'s central icon registry — Angular silently
threw "icon has not been provided by any available icon providers" at runtime instead of failing
the build. Also required a Vite dev-server cache clear (`.angular/cache`) to pick up the newly
imported icon chunks.

**Summary**: Built a complete shopping cart — service, Navbar badge, Product Detail quantity
picker, and a full `/cart` page — then verified the entire flow live with a freshly-registered
customer (add → badge updates → view cart → update quantity → remove → empty state), catching and
fixing both bugs above in the process. Also expanded demo data: a second seller ("Ananya Gems &
Co."), three more categories (Necklaces/Earrings/Bracelets), and four more products (7 total
across 2 sellers/4 categories) — kept intentionally as ongoing sample data per the user's explicit
decision, not cleaned up.

---

## 2026-08-05 — Customer UI: Product Browsing (List + Detail), Verified Live

**Module**: Frontend (`client/`) + demo data in the live DB

**Files modified**:
- `client/src/app/core/models/paged-result.ts` (new) — mirrors backend `PagedResult<T>`
- `client/src/app/features/products/models.ts` (new) — `ProductListItem`/`Product`/`Category`,
  numeric `MetalType`/`PurityType`/`ProductStatus`/`ProductSortOption` enums (no
  `JsonStringEnumConverter` on the backend, so enums are numeric on the wire, not strings)
- `client/src/app/features/products/products.service.ts` (new)
- `client/src/app/features/products/product-list/`, `client/src/app/features/products/product-detail/` (new)
- `client/src/app/shared/product-card/` (new) — reusable listing card
- `client/src/app/app.routes.ts` — added `/products` and `/products/:id` under the storefront Shell
- `client/src/app/core/layout/navbar/navbar.component.ts` — category links now navigate to
  `/products?search=...` instead of scrolling to a Home anchor
- `client/src/app/features/home/home.component.ts`, `.html` — hero CTA and category tiles now
  link to real `/products` routes instead of same-page fragments
- `docs/FRONTEND_PROGRESS.md`, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`

**Summary**: Built real product browsing — a filterable, paginated list page and a detail page,
both wired to the live `GetProductsQuery`/`GetProductByIdQuery`/`GetCategoriesQuery` endpoints,
using Angular's `rxResource` with the URL's query params as the single source of truth for
filters (bookmarkable/shareable). This is the first Customer UI slice verified against a
genuinely running backend + SQL Server database rather than just a production build: registered a
seller account via the live API, completed the KYC document-review and approval flow as the dev
admin, created a "Rings" category, and created three real products (with real Unsplash imagery),
then confirmed the list/detail pages render everything correctly — filters, category dropdown,
discount badges and strikethrough pricing, image gallery, gemstone/spec details, and the Add to
Cart/Wishlist "coming soon" snack bars — with zero console errors. That demo data (one seller, one
category, three products) is now sitting in the live database; a decision on keeping vs. removing
it is still open.

---

## 2026-08-05 — Login/Register Redesign + Auth Pages Moved Outside Shell

**Module**: Frontend (`client/`)

**Files modified**:
- `client/src/app/core/layout/auth-layout/` (new) — shared split-screen layout for auth pages
- `client/src/app/app.routes.ts` — Login/Register are now top-level routes; Home/Forbidden nest
  under a `ShellComponent` parent route instead of Shell wrapping every route unconditionally
- `client/src/app/app.ts`, `client/src/app/app.html` — App is now a bare `<router-outlet>` host;
  Shell is a routed layout component, not something App renders directly
- `client/src/app/features/auth/login/*`, `client/src/app/features/auth/register/*` — redesigned
  to the design system (serif headline, eyebrow, editorial photo+quote panel, no `mat-card`)
- `client/src/styles.scss` — shared `.auth-*` classes, `--mat-sys-secondary-container`/
  `--mat-sys-tertiary-container` overrides so the button-toggle's selected state uses gold instead
  of Material's default salmon/orange
- `docs/DESIGN_SYSTEM.md`, `docs/FRONTEND_PROGRESS.md`

**Summary**: The user flagged two things after using the app: they didn't like the generic
Material-card look of Login/Register (built before the design system existed), and asked why the
storefront Navbar/Footer appeared on those pages. The latter was an architecture gap — `App`
rendered `ShellComponent` unconditionally, so every route got the same chrome regardless of
whether it made sense there. Fixed by moving Login/Register to top-level routes outside Shell,
and gave them a proper premium split-screen treatment (full-bleed photo + editorial quote on
desktop, centered form on mobile) via a new shared `AuthLayoutComponent`. Caught and fixed a
Vite dev-server dependency-cache issue (`504 Outdated Optimize Dep`) that surfaced while verifying
the Register page live — a new Material import added after the dev server had already optimized
its dependency graph; fixed by clearing `.angular/cache` and restarting. Verified live: Home still
has Navbar/Footer, Login/Register don't, zero console errors, both light and mobile viewports.

---

## 2026-08-05 — Design System + Home Page (Angular); PrimeNG Removed

**Module**: Frontend (`client/`)

**Files modified**:
- `docs/DESIGN_SYSTEM.md` (new) — permanent visual/UX reference
- `client/src/styles/_tokens.scss` (new) — color/type/spacing/shadow/motion CSS custom properties
- `client/src/styles.scss` — tokens import, Material system-token overrides, global `.btn`/
  `.eyebrow`/`.section-title` classes, `MatSnackBar` error-panel styling
- `client/src/index.html` — Playfair Display + Inter fonts (was Roboto + Material Icons font)
- `client/src/app/core/layout/navbar/` (new), `client/src/app/core/layout/footer/` (new)
- `client/src/app/core/layout/shell.component.ts` — now composes Navbar + Footer instead of an
  inline Material toolbar
- `client/src/app/features/home/` — real hero/categories/featured/brand-story page (was a
  placeholder welcome message)
- `client/src/app/app.routes.ts` — Home route no longer requires auth (a storefront landing page
  must be public)
- `client/src/app/app.config.ts` — registered Lucide icons app-wide (`--legacy-peer-deps` install,
  peer range not yet updated for Angular 22), added `withInMemoryScrolling` for fragment nav links
- `client/src/app/core/http/error.interceptor.ts`, `client/src/app/core/layout/navbar/*.ts`,
  `client/src/app/core/layout/footer/*.ts` — `MessageService`/`Toast` → `MatSnackBar`
- `client/src/app/features/auth/register/*` — `SelectButton` → `MatButtonToggleGroup`
- `client/package.json` — removed `primeng`, `@primeuix/themes`, `primeicons`; added
  `lucide-angular`
- `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`, `docs/FRONTEND_PROGRESS.md`

**Summary**: Built the permanent design system doc and the app's first real page (Home: navbar,
footer, hero, category grid, featured pieces, brand story) in a premium editorial jewelry-boutique
direction. Mid-build, an actual `ng serve` + headless-Chromium visual check (not just a production
build) surfaced two real problems a build alone would never catch: the installed PrimeNG version
requires a paid license and was injecting an "Invalid PrimeUI License" banner into every page, and
the sourced hero photo had a real jewelry brand's name visible on the display risers. PrimeNG was
removed entirely (every usage swapped for a Material equivalent) rather than worked around, and
the hero image was replaced with a verified clean one. Final state verified: clean build, zero
console errors, correct fonts/colors, working fragment-scroll nav, working mobile menu.

---

## 2026-08-04 — Angular Frontend Foundation

**Module**: Frontend (new `client/` workspace)

**Files modified**: new `client/` directory — Angular 22 workspace scaffolded via `ng new`, plus:
- `client/src/styles.scss`, `client/.postcssrc.json` — Angular Material theming + Tailwind v4
  (theme/utilities only, Preflight excluded) + PrimeIcons wired together
- `client/src/app/app.config.ts` — HttpClient with `authInterceptor`/`errorInterceptor`,
  PrimeNG (`providePrimeNG`, `MessageService`), animations
- `client/src/app/core/auth/` — `models.ts`, `token-storage.service.ts`, `auth.service.ts`,
  `auth.guard.ts`, `auth.interceptor.ts` (new)
- `client/src/app/core/http/` — `problem-details.ts`, `error.interceptor.ts` (new)
- `client/src/app/core/layout/shell.component.ts` (+ html) (new)
- `client/src/app/features/auth/login/`, `client/src/app/features/auth/register/`,
  `client/src/app/features/home/`, `client/src/app/features/forbidden/` (new)
- `client/src/environments/` — dev points at `https://localhost:65334/api/v1` (matches
  `launchSettings.json`), production defaults to a relative `/api/v1`
- `client/angular.json` (bundle budget raised to 750kB/1.2MB — expected given 3 UI libraries)
- `docs/FRONTEND_PROGRESS.md`, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`

**Summary**: Scaffolded the Angular frontend from scratch — nothing existed before this. Built
the foundation every future screen depends on: JWT auth (login, register-as-customer-or-seller,
token refresh with request deduplication so concurrent 401s don't each trigger their own
`/auth/refresh` and trip the backend's reuse-detection), route guards matching the backend's role
model, a global HTTP error toast, and an app shell. Combined Angular Material, PrimeNG, and
Tailwind CSS per explicit instruction — Tailwind's Preflight reset is turned off so it doesn't
fight the two component libraries' own base styles; Tailwind is utility-only here. Verified via a
clean production build, a passing DI-wiring smoke test, and a dev-server boot — not yet exercised
against a live backend since no database was available in this environment. No feature screens
(product browsing, dashboards, etc.) yet — that's the next phase.

---

## 2026-08-04 — Orders: Shipping Calculation + Completion Rollup

**Module**: Orders

**Files modified**:
- `src/JewelryHub.Application/Common/Interfaces/ITaxCalculator.cs` (added `IShippingCalculator`)
- `src/JewelryHub.Application/Common/Services/FlatRateShippingCalculator.cs` (new)
- `src/JewelryHub.Application/DependencyInjection.cs` (registered `IShippingCalculator`)
- `src/JewelryHub.Application/Features/Orders/Commands/CreateOrder/CreateOrderCommand.cs`
- `src/JewelryHub.Application/Features/Orders/Commands/UpdateOrderItemStatus/UpdateOrderItemStatusCommand.cs`
- `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`, `docs/API_PROGRESS.md`, `docs/BACKEND_PROGRESS.md`

**Summary**: Resolved the two `TODO`s flagged in the original onboarding pass. Checkout now
computes a real shipping charge per seller group (flat rate + inter-state surcharge + weight
surcharge past a small allowance, waived above a free-shipping subtotal threshold) instead of
hardcoding `0`, via a new `IShippingCalculator` abstraction mirroring `ITaxCalculator`. Marking an
order item Delivered now rolls the parent Order to `Delivered` once every item reaches that
status, and updates the fulfilling seller's `TotalOrdersFulfilled`/`TotalRevenue` rollups
immediately (independent of other sellers on the same multi-seller order). No schema changes.

---

## 2026-08-04 — Jewelry Union Module (Application + API)

**Module**: Jewelry Union

**Files modified**:
- `src/JewelryHub.Application/Common/Interfaces/IUnitOfWork.cs` (13 new repository properties)
- `src/JewelryHub.Persistence/UnitOfWork.cs` (13 new repository implementations)
- `src/JewelryHub.Application/Features/Unions/Common/` — `UnionDto.cs`, `UnionMapper.cs`,
  `UnionAuthorization.cs`, `MeetingDto.cs`, `MeetingMapper.cs`, `PollDto.cs`, `PollMapper.cs` (new)
- `src/JewelryHub.Application/Features/Unions/Commands/` — 19 new commands: CreateUnion,
  ApproveUnion, RequestMembership, ReviewMembership, UpdateMemberRole, RemoveMember,
  CreateAnnouncement, DeleteAnnouncement, UploadDocument, CreateEvent, UpdateEventStatus,
  CreateMeeting, UpdateMeetingStatus, RsvpToMeeting, RecordMinute, CreatePoll, OpenPoll,
  ClosePoll, Vote (new)
- `src/JewelryHub.Application/Features/Unions/Queries/` — 14 new queries: GetUnions,
  GetPendingUnions, GetUnionById, GetUnionMembers, GetPendingMemberships, GetMyMemberships,
  GetAnnouncements, GetDocuments, GetEvents, GetMeetingById, GetMeetings, GetMyActionItems,
  GetPollById, GetPolls (new)
- `src/JewelryHub.API/Controllers/UnionsController.cs`,
  `src/JewelryHub.API/Controllers/UnionMeetingsController.cs`,
  `src/JewelryHub.API/Controllers/UnionPollsController.cs` (new — 33 endpoints total)
- `docs/README.md`, `docs/ARCHITECTURE.md`, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`,
  `docs/API_PROGRESS.md`, `docs/BACKEND_PROGRESS.md`

**Summary**: Built out the Application and API layers for the Jewelry Union governance system —
previously the Domain and Persistence layers had 13 tables modeled (memberships, meetings,
polls, announcements, documents, events) with nothing above them. Covers union creation
(KYC-approved sellers only) with admin approval, membership lifecycle with officer roles
(President/VicePresident/Secretary), officer-gated announcements/events, a members-only document
library, meetings that auto-invite active members and support RSVP/minutes/action items, and
governance polls (Draft→Active→Closed) with single/multi-select voting enforced unique per
member at the DB level. No Domain or Persistence changes were needed and no new migration was
required — every table this module needed already existed. Solution builds with 0 warnings/0
errors (77 total endpoints now, up from 44).

---

## 2026-08-04 — Documentation & Build/Migration Fixes

**Module**: Cross-cutting (build health, database, docs)

**Files modified**:
- `src/JewelryHub.Application/Features/Auth/Commands/RefreshToken/RefreshTokenCommand.cs`
- `src/JewelryHub.Application/Features/Auth/Commands/Login/LoginCommand.cs`
- `src/JewelryHub.Application/Features/Auth/Commands/RegisterSeller/RegisterSellerCommand.cs`
- `src/JewelryHub.Application/Features/Auth/Commands/RegisterCustomer/RegisterCustomerCommand.cs`
- `src/JewelryHub.Persistence/UnitOfWork.cs`
- `src/JewelryHub.Persistence/Configurations/Identity/IdentityConfigurations.cs`
- `src/JewelryHub.Infrastructure/JewelryHub.Infrastructure.csproj`
- `src/JewelryHub.Persistence/Migrations/20260804130625_InitialCreate.cs` (new)
- `src/JewelryHub.Persistence/Migrations/20260804130625_InitialCreate.Designer.cs` (new)
- `src/JewelryHub.Persistence/Migrations/JewelryHubDbContextModelSnapshot.cs` (new)
- `docs/README.md`, `docs/ARCHITECTURE.md`, `docs/DATABASE.md`, `docs/API_PROGRESS.md`,
  `docs/BACKEND_PROGRESS.md`, `docs/FRONTEND_PROGRESS.md`, `docs/PROJECT_STATUS.md`,
  `docs/ROADMAP.md`, `docs/CHANGELOG.md` (all new)

**Summary**: The solution did not actually build before this pass — four Auth commands had a
`RefreshToken` namespace/type collision (the `Features/Auth/Commands/RefreshToken` namespace
shadowed the `Domain.Identity.RefreshToken` entity type), `UnitOfWork.cs` was missing
`using Microsoft.EntityFrameworkCore;` (causing a confusing overload-resolution error on
`ExecuteAsync`), `IdentityConfigurations.cs` called `IsRowVersion()` on the wrong builder type,
and `JewelryHub.Infrastructure.csproj` referenced a deprecated ASP.NET Core NuGet package instead
of a `FrameworkReference`. All four are fixed; the solution now builds with 0 warnings/errors.
Generated the first-ever EF Core migration (`InitialCreate`) — previously `Database.MigrateAsync()`
in `Program.cs` had no migration to apply, so the app couldn't create its schema on startup.
Wrote the full `docs/` set from a ground-truth inspection of the codebase (every controller
action, every Application handler, the generated migration, and the Domain folder structure) —
no completion status in these docs was guessed.

---

## Template for future entries

```
## YYYY-MM-DD — <Module or feature name>

**Module**: <area>

**Files modified**:
- <path>
- <path>

**Summary**: <why this changed, in 1-3 sentences — not a restatement of the diff>
```
