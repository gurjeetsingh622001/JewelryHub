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
**Completed (2026-08-11).** Decided, per explicit user choice, on a dedicated `AdminController`
(`api/v1/admin`) rather than staying fully distributed — but scoped to only what genuinely had no
existing home, keeping the codebase's established convention (a resource's admin queue query lives
on that resource's own controller) for everything that already fit it. Added: `GET dashboard`
(platform-wide counts — customers/sellers/unions pending & approved, orders/revenue this month,
total/flagged reviews), `GET users` + `POST users/{id}/status` (the first-ever Application-layer
User listing and activate/deactivate, with a guard against self-deactivation), and `GET reviews`
(an unfiltered-by-default moderation browsing queue). Seller/union approval and review moderation
deliberately stayed on `SellersController`/`UnionsController`/`ReviewsController`. Role/permission
management and deeper reporting/analytics remain out of scope — see Phase 17. See
[BACKEND_PROGRESS.md](BACKEND_PROGRESS.md) and [API_PROGRESS.md](API_PROGRESS.md) for full detail.

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
**Completed (2026-08-07).** `order-history/` (paginated list via `GetMyOrdersQuery`, linking into
the existing `order-detail` page) plus a full Reviews slice: a "Write a Review" inline form on
Order Detail for any `Delivered` item (product + seller star ratings, optional title/comment;
correctly surfaces the backend's "already reviewed" business-rule error since there's no
server-side flag to hide the button after the fact — a documented v1 simplification), and a
Customer Reviews section on Product Detail (`GetProductReviewsQuery`) showing star ratings, title,
comment, and any seller response. Verified live end-to-end: reviewed a delivered order → review
appeared on the product page with the correct rating → resubmitting the same item's review
correctly showed "You have already reviewed this purchase." via the global error interceptor.
**Found and fixed a real CSS bug in the process**: Lucide's `Star` icon is stroke-only
(`fill="none"`) by default, so a `color` change alone only recolors the outline — interactive
rating stars looked unfilled regardless of state until `::ng-deep svg { fill: currentColor }` was
added.

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
**Completed (2026-08-11).** A role-guarded `/admin` area (`roleGuard(['Admin'])`) consuming the new
`AdminController` (Phase 12): a Dashboard (stat cards for customers/sellers/unions/orders/revenue/
reviews, plus quick-link action cards), a Pending Sellers page (per-document Verify/Reject, seller
Approve/Reject with an inline reason field — reusing `SellersController`'s existing endpoints),
a Pending Unions page (Approve, reusing `UnionsController`), a Reviews moderation page
(`MatButtonToggleGroup` All/Flagged/Hidden filter, Hide/Restore reusing `ReviewsController`'s
moderate endpoint), and a Users page (search + role filter, Deactivate/Reactivate). An Admin-only
navbar link was added, gated on `auth.hasRole('Admin')`.

Verified live end-to-end as the seeded admin account: dashboard rendered real counts → verified a
KYC document and approved the pending seller → hid then restored a review → deactivated then
reactivated a user via search → confirmed a non-admin (Seller) account is redirected away from
`/admin`. Zero console errors on the final run. No new backend or frontend app bugs were found
during this pass — the only issues encountered were two bugs in the Playwright verification script
itself (an ambiguous `button:has-text("Approve")` selector matching the wrong one of two pending-
seller cards, and a script that assumed a fixed starting document-verification state on re-runs),
both diagnosed by cross-checking backend state directly via `curl` before ruling out an app bug.

This closes out the full Admin module — Phase 12 (backend) and Phase 15 (frontend) are both
complete, which in turn closes out every module in the originally planned v1 roadmap (Customer,
Seller, Union, Admin).

## Phase 16 — Union Dashboard (Angular)
**Core union scope completed (2026-08-07)** — scoped down by explicit user decision to keep this
pass sized like the Seller module: browse/create/join unions, membership management, and
officer-gated announcements/documents/events. **Meetings (agenda/RSVP/minutes/action items) and
Governance Polls (create/open/close/vote) are deliberately deferred to a follow-up phase** — see
Phase 16a below; their backend (`UnionMeetingsController`, `UnionPollsController`, 22 endpoints)
is already complete and unconsumed by any frontend.

Built: a public `/unions` directory (search by name, admin-approved unions only —
`GetUnionsQuery` already filters this server-side), `/unions/new` (Seller-only creation form —
founder becomes President, union starts unapproved), `/unions/mine` (a seller's own memberships,
joining membership rows to their union's name/city via a `forkJoin` since `UnionMemberDto` doesn't
carry it), and `/unions/:id` (public detail page, `MatTabsModule` tabs for Overview/Members/
Announcements/Documents/Events). Officer-gated actions (create announcement/document/event,
approve/reject pending membership requests) are shown only when the caller's own membership role
is President/VicePresident/Secretary — computed client-side from `GetMyMembershipsQuery`, mirroring
the backend's `UnionAuthorization.OfficerRoles` set exactly.

**Found and fixed a real bug during verification**: `members`, `announcements`, and `documents`
are backend `[Authorize]` endpoints (any logged-in user, but not anonymous) — only `events` is
`[AllowAnonymous]`. The page fetched all four unconditionally regardless of login state, so an
anonymous visitor correctly got 401s on three of them, which in turn triggered a spurious
refresh-token attempt for a session that never existed (`AuthService.refresh()` throwing "No
refresh token available"). Fixed by gating those three resources on `auth.isAuthenticated()` and
showing a "Log in to view this" prompt instead. (Initially misdiagnosed as a concurrent-refresh
race — ruled that out by directly firing 5 concurrent authenticated requests at the backend via
curl, which all succeeded cleanly, before finding the real cause in the actual request/response
log.) Verified live end-to-end: created a union as a KYC-approved seller → admin-approved it → a
second seller requested to join → approved as the founding President → published an announcement,
uploaded a document, and scheduled an event as an officer → confirmed all of it renders correctly
for both a signed-in officer and an anonymous visitor, zero console errors.

## Phase 16a — Union Meetings + Governance Polls (Angular)
**Completed (2026-08-07).** Deferred from Phase 16 to keep that pass a reasonable size; this phase
was pure frontend since the backend (`UnionMeetingsController`, `UnionPollsController`) was already
complete.

Built, added to `/unions/:id` as two new tabs (Meetings, Polls — both gated on the caller being an
active member or Admin, matching `GetMeetingsQuery`/`GetPollsQuery`'s own server-side check):
- **Meetings**: `/unions/:unionId/meetings/new` (officer-only — title, location, virtual link,
  schedule, duration, a dynamic agenda-topic list) and `/unions/:unionId/meetings/:meetingId`
  (agenda, the full attendee list with RSVP status, an RSVP widget for the caller's own invite,
  officer status controls — Start/Cancel/Complete — and an officer-only "Record Minutes" form:
  optional agenda-item link, decision summary, discussion notes, and a dynamic action-item list
  each assigned to an attendee with an optional due date). Agenda-item status and action-item
  status are read-only in this UI — the backend has no endpoint to change either, so there was
  nothing to build there.
- **Polls**: `/unions/:unionId/polls/new` (officer-only — question, single/multi-select toggle,
  optional close date, a dynamic 2+ option list; created as `Draft` per the backend's design) and
  `/unions/:unionId/polls/:pollId` (a radio/checkbox ballot shown only while `Active`, live vote
  counts with a simple bar-chart-style result view, and officer Open/Close controls). Like Reviews,
  there's no per-member "already voted" flag on `PollDto`, so the ballot just hides itself locally
  after a successful submit and a revote correctly surfaces the backend's real "already voted"
  error rather than failing silently.
- **My Action Items**: added to `/unions/mine` — the caller's open action items across every union
  they belong to (`GetMyActionItemsQuery`), each showing its due date and status.

**Found and fixed a real layout bug during verification**: the Record Minutes form's action-item
row (description + responsible-member dropdown + due date + delete button, all in one flex row)
squeezed the dropdown and date input down to an unusable, near-illegible width. Fixed with a
responsive grid (stacks to one column below 640px). Verification also surfaced two Playwright/
Angular-Material interaction quirks (clicking a `mat-select`/`mat-radio-button` host element
instead of its native `<input>` can silently no-op in headless Chromium) — confirmed these were
test-tooling artifacts, not app bugs, by inspecting `aria-checked`/`outerHTML` directly and showing
the underlying model updates correctly once the native control is toggled.

Verified live end-to-end: scheduled a meeting (auto-inviting every active member) → recorded
minutes with an action item → a second member confirmed their RSVP; created a poll as Draft →
opened it → a second member voted → tallies updated correctly and re-voting was rejected; the
assigned action item appeared under "My Action Items" for the responsible member. Zero console
errors on the final run.

This closes out the full Union module — Phase 11 (backend) through Phase 16a (frontend) are all
complete.

## Phase 17 — Reports / Analytics
**Pending.** No reporting endpoints exist yet beyond the Admin dashboard's basic counts (Phase 12/
15 — customers/sellers/unions/orders/revenue/reviews totals only, no trends/breakdowns/exports).
Deeper reporting (sales over time, seller performance, union activity) and role/permission
management are the natural next additions to `AdminController` now that it exists to hold them.

## Phase 18 — Testing
**Pending — deliberately deferred.** Unit tests, integration tests, and architecture tests
(enforcing the Clean Architecture dependency rules mechanically) all start here, once feature
work above has settled down. Do not pull this phase forward without being asked.

## Phase 19 — CI/CD
**Pending — deliberately deferred.** `.github/workflows/` exists but is empty by design for now.

## Phase 20 — Wishlist UI, File Upload, Cart/Wishlist Bug Fixes, Large Demo-Data Pass
**Completed (2026-08-11).** A post-v1 polish pass triggered by a live 500 bug report on
add-to-cart, which snowballed into finding and fixing the same bug in Wishlist, then closing the
two remaining gaps flagged during that work (Wishlist had a complete backend and zero frontend;
there was no real file upload anywhere in the app) and finally generating a much larger demo
dataset for testing against.

- **Cart/Wishlist bugs**: `AddToCartCommand` (again — see Phase 13c for the first occurrence,
  which was a different root cause) and `AddToWishlistCommand` both threw
  `DbUpdateConcurrencyException` (500) the moment a customer added a genuinely new item, because
  the new `CartItem`/`WishlistItem` was attached to its parent only via collection-navigation
  (`cart.Items.Add(...)`), never through an explicit repository `AddAsync` — EF Core's change
  tracker treated the entity's client-generated `Guid` key ambiguously and issued an `UPDATE`
  matching 0 rows instead of an `INSERT`. Exact same shape as the `Shipment` bug from Phase 14.
  Fixed both by adding a `CartItems`/`WishlistItems` repository to `IUnitOfWork` and calling
  `AddAsync` explicitly; then swept the rest of `Features/` for the same shape and confirmed no
  other instance exists (every other `collection.Add(new Entity{...})` call in the codebase
  happens *before* the parent's own explicit `AddAsync`, which is the safe ordering).
- **Wishlist UI**: `WishlistService` + a `/wishlist` page, the Navbar Heart icon, and the
  product-card/product-detail "Add to Wishlist" buttons — all previously either missing or showing
  an honest "coming soon" toast even though the backend (`WishlistController`, 3 endpoints) had
  been complete since Phase 5.
- **Generic file upload**: `Features/Uploads` + `UploadsController` (`Seller,Admin` only) —
  `POST product-images` / `POST kyc-documents`, saved to local disk (`wwwroot/uploads/`, served via
  `app.UseStaticFiles()`) behind a new `IFileStorageService` abstraction, chosen over cloud blob
  storage for this stage since it needs zero external account/credentials. The Seller product
  form's and KYC form's URL text fields were replaced with real file pickers. A new
  `resolveMediaUrl()` helper resolves the backend's relative `/uploads/...` paths against the API's
  origin (needed since the Angular dev server and API run on different ports) and is applied
  everywhere an image or document link renders.
- **Demo data**: a scripted pass against the live API (real registration/KYC/approval/product/
  union flows, not direct SQL) added 14 sellers, 18 customers, 131 products, and 5 unions with
  members, bringing the dev database to 32 customers, 22 approved sellers, 142 products, 6 unions.

Verified live end-to-end throughout: both cart bug paths (new cart, new item in existing cart) and
both wishlist bug paths return 200; wishlist toggle → `/wishlist` page → Move to Cart; a real image
uploaded during product creation renders correctly cross-origin; a real KYC document uploaded and
submitted; the demo-data script's output spot-checked in the browser (pagination, category filter,
union directory, Admin dashboard counts). Zero console errors on every run. See
[CHANGELOG.md](CHANGELOG.md) for the full file list.

## Maintenance

Update this file whenever a phase's status changes, and whenever a new phase is identified.
