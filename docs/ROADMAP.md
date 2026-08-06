# Roadmap

Ordered by dependency, not by calendar. A phase should only start once the phases it depends on
are actually done — check [PROJECT_STATUS.md](PROJECT_STATUS.md) before assuming a "Completed"
phase below is still accurate.

## Phase 1 — Backend Foundation
**Completed.** Clean Architecture skeleton, Domain entities, EF Core configurations, DbContext,
Repository/UnitOfWork, DI composition per layer.

## Phase 2 — Authentication & Authorization
**Completed.** JWT issuance/rotation, BCrypt hashing, role-based `[Authorize]`, login lockout.
*Not done, not blocking*: password reset, email verification.

## Phase 3 — Seller Module
**Completed.** Profile, KYC document submission/review, approve/reject flow, pending queue.

## Phase 4 — Product Module
**Completed.** Categories (with tree via `ParentCategoryId`) and Products CRUD, inventory
adjustment, product status transitions.

## Phase 5 — Cart & Wishlist
**Completed.**

## Phase 6 — Orders
**Completed (2026-08-04).** Checkout, retrieval, cancellation, seller fulfillment queue, per-item
status updates all work end-to-end, including per-seller shipping calculation
(`IShippingCalculator`/`FlatRateShippingCalculator` — flat rate + inter-state surcharge +
weight surcharge, waived above a free-shipping threshold) and order-level auto-completion
(parent `Order` rolls to `Delivered` once every item is; seller revenue/count rollups update per
delivered item). Shipping rates are constants, not yet admin-configurable — a natural follow-up
once/if that's wanted, same shape as how Tax rates became DB-driven.

## Phase 7 — Payments
**Partially complete — blocked on an external decision.** `ConfirmPaymentCommand` has complete,
real logic (idempotency, state transition, inventory deduction, notification) but nothing calls
it from an actual payment gateway. Needs: pick a gateway (Razorpay vs Stripe — India-based
jewelry marketplace suggests Razorpay, but confirm with the user), implement the webhook
signature verification, wire it to call the existing command.

## Phase 8 — Reviews
**Completed.** Product/seller review listing, creation tied to a verified purchase
(`OrderItemId`), seller responses, admin moderation.

## Phase 9 — Notifications & Real-Time Layer
**Completed.** Persist-then-push notifications, SignalR hub with JWT-over-querystring auth for
the WebSocket handshake.

## Phase 10 — Tax Management
**Pending.** `TaxRate` entities exist and are applied at checkout, but there's no CRUD API. Small
phase — likely a half-day of work following the exact same pattern as Categories.

## Phase 11 — Jewelry Union APIs
**Completed (2026-08-04).** `Features/Unions/` Application slice plus three controllers
(`UnionsController`, `UnionMeetingsController`, `UnionPollsController`) — 33 endpoints covering
union creation + admin approval, membership (join/review/officer roles/removal), officer-gated
announcements/documents/events, meetings with agenda items/RSVP/minutes/action items, and
governance polls with voting. Authorization model chosen: union officer role
(President/VicePresident/Secretary) *or* platform Admin for governance actions; Admin has no
bypass for actions that need a `UnionMember` author (creating a meeting/poll/announcement) since
Admin accounts don't have a membership row to attribute the record to. See
[BACKEND_PROGRESS.md](BACKEND_PROGRESS.md) and [API_PROGRESS.md](API_PROGRESS.md) for full detail.
No schema changes were needed — Domain/Persistence already had every table this phase needed.

## Phase 12 — Admin Module (dedicated)
**Pending.** Currently admin capability is role-gated actions bolted onto other controllers with
no dedicated dashboard. Needs: decide whether this stays distributed (current pattern, works fine
for approve/reject/moderate-style actions) or gets a real `AdminController` for
platform-wide concerns (user management, role assignment, reporting/analytics). Recommend
deciding this only after Phase 11, since Union approval/moderation will likely need admin
actions too and should follow whichever pattern is chosen here.

## Phase 13 — Frontend Foundation (Angular)
**Completed (2026-08-04).** Angular 22 workspace at `client/` — Material + Tailwind (PrimeNG was
tried and removed, see Phase 13a), JWT auth with dedup'd refresh-and-retry, route guards,
login/register pages, app shell. See [FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md).

## Phase 13a — Design System + Home Page + Auth Redesign (Angular)
**Completed (2026-08-05).** Permanent design system (`docs/DESIGN_SYSTEM.md`) — premium editorial
jewelry-boutique direction, warm ivory/gold/charcoal palette, Playfair Display + Inter type,
Lucide icons, curated Unsplash imagery. Built Navbar, Footer, and the Home page (hero, category
grid, featured pieces, brand story) to it, then redesigned Login/Register (shared
`AuthLayoutComponent`, split-screen photo+quote) and moved them outside the storefront Shell — no
Navbar/Footer on auth pages by design; fixed an architecture gap where `App` had rendered Shell
unconditionally on every route. **Also discovered and resolved mid-build**: the installed PrimeNG
version (22.x) requires a paid PrimeUI license or it shows an "Invalid PrimeUI License" banner on
every page — found via an actual `ng serve` + headless-Chromium check, not just a build pass.
PrimeNG was removed; `Toast`/`SelectButton` usages replaced with `MatSnackBar`/
`MatButtonToggleGroup`. Verified live (fonts, colors, fragment-scroll nav, mobile menu, zero
console errors).

## Phase 13b — Customer UI: Product Browsing (Angular)
**Completed (2026-08-05).** Product list (URL-driven search/category/metal/price/sort/pagination
filters via `rxResource`) and detail pages, wired to the live Products/Categories APIs; a reusable
`ProductCardComponent`; Navbar/Home category links now navigate to real `/products` routes
(filtered by a text-search stand-in until real Category-to-type data exists). **First frontend
slice verified against an actual running backend + database**, not just a build — registered a
seller, ran the KYC approval flow as the dev admin, created a category and three real products via
the live API, and confirmed everything renders correctly end-to-end with zero console errors. See
[FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md) for the demo data left in the DB from this.

## Phase 13c — Customer UI: Cart (Angular)
**Completed (2026-08-06).** `CartService` (signals, auto-refreshes on auth state change since the
backend's cart is Customer-only), a live item-count badge in the Navbar, a quantity stepper + real
Add to Cart on Product Detail, and a `/cart` page (line items, quantity update, remove, subtotal,
empty state — "Proceed to Checkout" is an honest "coming soon" snack bar since Checkout doesn't
exist yet). **Verifying this live caught a real backend bug**: `AddToCartCommand` threw a 500 on
every single add-to-cart call (a no-tracking/tracking EF Core entity mismatch — see
[API_PROGRESS.md](API_PROGRESS.md)), plus a frontend one (three Lucide icons used in the new UI
were never registered in `app.config.ts`, so they silently failed to render). Both fixed and
re-verified end-to-end with zero console errors. Demo data expanded in the process: a second
seller, 3 more categories, 4 more products (7 total) — kept intentionally as ongoing sample data.

## Phase 13d — Customer UI: Checkout + Order Confirmation (Angular)
**Completed (2026-08-06).** Address book (select a saved address or add a new one inline —
auto-shown if the customer has none yet), payment method selection, place order, and
auto-confirmation of payment (stands in for a real gateway webhook for every method except Cash
on Delivery, which is collected on delivery instead), landing on an order confirmation/detail
page. **Building this surfaced a hard backend blocker before any frontend verification was even
possible**: `CreateOrderCommand` requires a `ShippingAddressId`/`BillingAddressId`, but no
endpoint anywhere let a customer create a `CustomerAddress` — checkout was structurally
impossible. Closed by adding `CustomerAddressesController` (full CRUD) and completing
`CustomerAddress`'s `ISoftDelete` implementation — see [API_PROGRESS.md](API_PROGRESS.md) and
[DATABASE.md](DATABASE.md). Verified live end-to-end with a real customer (add to cart → checkout
→ confirmed order with correct items/address/payment/totals → cart empties), zero console errors.

## Phase 13e — Customer UI: Order History (List) + Reviews (Angular)
**Pending.** Builds directly on Phase 13d — `GetMyOrdersQuery`/`OrdersService.getMyOrders` already
exist on both sides, this is "just" a list page linking into the `order-detail` page Phase 13d
already built. Reviews (`ReviewsController`) is fully built on the backend and has no frontend
yet either.

## Phase 14 — Seller Dashboard (Angular)
**Completed (2026-08-06).** `/seller` area (`roleGuard(['Seller'])`) with a Dashboard (KYC status,
revenue/orders-fulfilled/rating stats, quick links), KYC document submission + status list, My
Products (list + a combined create/edit form, plus a plain quantity-delta inventory adjuster), and
an Order Fulfillment queue (status filter, advance Confirmed → Processing → Shipped → Delivered,
Carrier/Tracking Number collected inline for the Shipped step). Verified live end-to-end: a real
seller account registered → KYC submitted → admin-approved → product listed → bought by a test
customer → fulfilled through every status → Dashboard stats updated correctly, zero console
errors. **Surfaced and fixed a real backend bug** (`UpdateOrderItemStatusCommand` 500 on marking an
item Shipped) and two frontend bugs (a stray `GET /products/null` request on the "new product"
route; the KYC form showing false validation errors right after a successful submit) — see
[API_PROGRESS.md](API_PROGRESS.md) and [FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md).

## Phase 15 — Admin Dashboard (Angular)
**Pending.** Depends on Phase 12's decision about whether a dedicated Admin API exists to back it.

## Phase 16 — Union Dashboard (Angular)
**Pending.** Depends on Phase 11.

## Phase 17 — Reports / Analytics
**Pending.** No reporting endpoints exist anywhere yet (sales, seller performance, union
activity). Likely follows naturally once Admin (Phase 12) exists to consume from.

## Phase 18 — Testing
**Pending — deliberately deferred.** Unit tests, integration tests, and architecture tests
(enforcing the Clean Architecture dependency rules mechanically) all start here, once feature
work above has settled down. Do not pull this phase forward without being asked.

## Phase 19 — CI/CD
**Pending — deliberately deferred.** `.github/workflows/` exists but is empty by design for now.

## Maintenance

Update this file whenever a phase's status changes, and whenever a new phase is identified.
