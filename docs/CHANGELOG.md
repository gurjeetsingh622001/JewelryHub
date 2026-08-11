# Changelog

Every completed unit of work gets an entry here: date, module, files modified, summary. This is
the project's development history — kept even if chat history is lost. Newest entries at the top.

---

## 2026-08-11 — Wishlist UI, Real File Upload, Two More `DbUpdateConcurrencyException` Bug Fixes, Large Demo-Data Pass (Verified Live)

**Module**: Backend (`Features/Cart`, `Features/Wishlist`, new `Features/Uploads`,
`UploadsController`, new `IFileStorageService`/`LocalFileStorageService`) + Frontend (new
`features/wishlist/`, new `core/uploads/`, new `shared/resolve-media-url.ts`, Seller product/KYC
forms) + a live demo-data generation script

**Trigger**: the user reported a live 500 error on `POST /api/v1/cart/items` — every add-to-cart
call failing — plus three feature requests in the same message: finish the Wishlist UI, add real
image/document upload, and generate a much larger demo dataset for testing.

**Bug #1 — `AddToCartCommand`, second occurrence**: a *different* root cause than the 2026-08-06
fix (that one was a no-tracking/tracking mismatch on the `Product` load; this one held). A
brand-new `CartItem` was attached to the tracked `Cart` only via `cart.Items.Add(...)`
collection-navigation, never through an explicit repository call. `CartItem.Id` is a
client-generated `Guid` (`BaseEntity`'s property initializer), and EF Core's change-tracker fixup
treats a newly-discovered entity with an already-set key ambiguously, issuing an `UPDATE`
(matching 0 rows, `DbUpdateConcurrencyException`) instead of an `INSERT`. Exact same shape as the
`Shipment` bug in `UpdateOrderItemStatusCommand` (2026-08-06). Fixed by adding a `CartItems`
repository to `IUnitOfWork` and calling `AddAsync` explicitly. Verified live via curl: both a
brand-new cart and an existing cart with a different new product now return 200.

**Bug #2 — `AddToWishlistCommand`, found live while building the Wishlist UI (not reported)**:
identical shape and identical fix (`WishlistItems` repository + explicit `AddAsync`). This made it
the third occurrence of the same root cause in this codebase, so the rest of `Features/` was swept
for the same `collection.Add(new Entity {...})` pattern — every other instance
(`RegisterCustomerCommand`, `RegisterSellerCommand`, `CreateProductCommand`'s Gemstones/Images)
attaches children *before* the parent's own explicit `AddAsync`, which is the safe ordering, so no
further instances exist.

**Wishlist UI**: the backend (`WishlistController`, 3 endpoints) had been complete since Phase 5
with zero frontend — the Navbar Heart icon and every "Add to Wishlist" button showed an honest
"coming soon" toast. Built `WishlistService` (signals, same Customer-only lifecycle as
`CartService`) and a `/wishlist` page (grid, Move to Cart, Remove, empty state); wired the Navbar
badge and the product-card/product-detail wishlist buttons to real toggle state.

**Real file upload**: nothing in the app had ever supported an actual file upload — product images
and KYC documents were both plain URL text fields. Added a generic `Features/Uploads` slice +
`UploadsController` (`api/v1/uploads`, `Seller,Admin` only): `POST product-images` / `POST
kyc-documents`, validating size (5 MB) and extension, saving via a new `IFileStorageService` —
`LocalFileStorageService` writes to the API's `wwwroot/uploads/`, served back out by
`app.UseStaticFiles()`. Chosen over cloud blob storage for this stage (per user decision) since it
needs zero external account/credentials; the interface boundary keeps a later swap contained to
Infrastructure. Existing commands (`CreateProductCommand`, `SubmitSellerDocumentCommand`) were
untouched — uploading is a separate client-side pre-step that returns a URL. Replaced the Seller
product form's "Image URL" and the KYC form's "Document URL" text fields with real file pickers
(upload spinner, thumbnail/filename preview). Added `shared/resolve-media-url.ts` to resolve the
backend's relative `/uploads/...` paths against the API's origin (needed since the Angular dev
server and API run on different ports) and applied it everywhere an image or document link renders
(product card, product detail gallery, cart, wishlist, the Admin pending-sellers document link).

**Demo data**: wrote a Node script driving the live API through real business flows (register →
submit KYC → admin-approve → create product / create union → join → admin-approve-membership),
not direct SQL. Added 14 sellers, 18 customers, 131 products, 5 unions with 3-7 members each —
zero errors across the full run. Brought the dev database to 32 customers, 22 approved sellers,
142 products, 6 unions.

**Files modified (backend)**:
- `src/JewelryHub.Application/Common/Interfaces/IUnitOfWork.cs` — added `CartItems`,
  `WishlistItems` repositories
- `src/JewelryHub.Persistence/UnitOfWork.cs` — same
- `src/JewelryHub.Application/Features/Cart/Commands/AddToCart/AddToCartCommand.cs` — explicit
  `AddAsync` for the new item
- `src/JewelryHub.Application/Features/Wishlist/Commands/AddToWishlist/AddToWishlistCommand.cs` —
  same fix
- `src/JewelryHub.Application/Common/Interfaces/IFileStorageService.cs` (new)
- `src/JewelryHub.Application/Features/Uploads/` (new) — `UploadedFileDto`, `UploadFileCommand`
- `src/JewelryHub.Infrastructure/Storage/LocalFileStorageService.cs` (new)
- `src/JewelryHub.Infrastructure/DependencyInjection.cs` — registered `IFileStorageService`
- `src/JewelryHub.API/Controllers/UploadsController.cs` (new)
- `src/JewelryHub.API/Program.cs` — added `app.UseStaticFiles()`
- `.gitignore` — added `src/JewelryHub.API/wwwroot/uploads/` (runtime-uploaded content, not source)

**Files modified (frontend)**:
- `client/src/app/features/wishlist/` (new) — `models.ts`, `wishlist.service.ts`,
  `wishlist-page/`
- `client/src/app/core/uploads/uploads.service.ts` (new)
- `client/src/app/shared/resolve-media-url.ts` (new)
- `client/src/app/app.routes.ts` — added `/wishlist`
- `client/src/app/core/layout/navbar/navbar.component.ts`/`.html` — real Wishlist link + badge
- `client/src/app/shared/product-card/product-card.component.ts`/`.html`/`.scss` — real wishlist
  toggle, `resolveMediaUrl`
- `client/src/app/features/products/product-detail/product-detail.component.ts`/`.html` — real
  wishlist toggle, `resolveMediaUrl`
- `client/src/app/features/cart/cart-page/cart-page.component.ts`/`.html` — `resolveMediaUrl`
- `client/src/app/features/seller/products/product-form/seller-product-form.component.ts`/`.html`/
  `.scss` — file-picker image upload
- `client/src/app/features/seller/kyc/seller-kyc.component.ts`/`.html`/`.scss` — file-picker
  document upload, `resolveMediaUrl` on the submitted-documents list
- `client/src/app/features/admin/pending-sellers/admin-pending-sellers.component.ts`/`.html` —
  `resolveMediaUrl` on the document review link
- `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`, `docs/API_PROGRESS.md`,
  `docs/BACKEND_PROGRESS.md`, `docs/FRONTEND_PROGRESS.md`

**Verified live end-to-end**: both Cart-bug paths and both Wishlist-bug paths return 200; wishlist
toggle from a product card → `/wishlist` page → Move to Cart; a real product photo uploaded during
creation renders correctly cross-origin; a real KYC document uploaded, submitted, and viewable; a
`.txt` upload correctly rejected with the extension-whitelist message; a Customer-role upload
attempt correctly gets 403; the demo-data script's output spot-checked in the browser (pagination,
category filter, union directory, Admin dashboard counts). Zero console errors on every run.

---

## 2026-08-11 — Admin Module: Dashboard, Seller/Union Approval Consumption, Review Moderation, User Management (Verified Live)

**Module**: Backend (new `Features/Admin/`, `AdminController`) + Frontend (new
`features/admin/`, extended `seller.service.ts`/`unions.service.ts`/`reviews.service.ts`)

**Scope decision**: closes Phase 12 (backend) and Phase 15 (frontend), the last module on the
originally planned v1 roadmap. Before writing any code, researched the existing codebase's own
convention for admin-gated actions via an Explore subagent: every admin queue query already lived
on its own resource's controller (`GetPendingSellersQuery` → `SellersController`,
`GetPendingUnionsQuery` → `UnionsController`), not centralized. Deliberately followed that
precedent rather than inventing a new one — seller/union approval and review moderation stayed put
— and scoped the new `AdminController` to only what genuinely had no existing home: a platform
dashboard, user account management (the first-ever Application-layer code to list `User` rows —
nothing did this before), and an unfiltered review-moderation browsing queue (the existing
`GetProductReviewsQuery` defaults to approved-only, which is wrong for an admin needing to see
hidden reviews too).

**Files modified (backend)**:
- `src/JewelryHub.Application/Features/Admin/Common/AdminDto.cs` (new) — `DashboardOverviewDto`,
  `UserSummaryDto`, `AdminReviewDto`
- `src/JewelryHub.Application/Features/Admin/Queries/GetDashboardOverview/` (new) — platform-wide
  counts: customers/sellers/unions pending & approved, orders and revenue this month (`GrandTotal`
  summed for non-cancelled/refunded orders since the start of the month), total/flagged reviews
- `src/JewelryHub.Application/Features/Admin/Queries/GetUsers/` (new) — search (email/first/last
  name) + role + active-status filters, paginated, includes `UserRoles.Role`
- `src/JewelryHub.Application/Features/Admin/Commands/SetUserActiveStatus/` (new) — activate/
  deactivate, throws `BusinessRuleException` if an admin tries to deactivate their own account
- `src/JewelryHub.Application/Features/Admin/Queries/GetAllReviews/` (new) — `isApproved`/
  `isFlagged` filters, no default filter (unlike the public review query)
- `src/JewelryHub.API/Controllers/AdminController.cs` (new) — `api/v1/admin`, class-level
  `[Authorize(Roles = "Admin")]`: `GET dashboard`, `GET users`, `POST users/{id}/status`,
  `GET reviews`

**Files modified (frontend)**:
- `client/src/app/features/admin/models.ts`, `admin.service.ts` (new)
- `client/src/app/features/admin/dashboard/`, `pending-sellers/`, `pending-unions/`, `reviews/`,
  `users/` (new components)
- `client/src/app/features/seller/seller.service.ts` — added `getPendingSellers`,
  `reviewDocument`, `approveSeller`, `rejectSeller`
- `client/src/app/features/unions/unions.service.ts` — added `getPendingUnions`, `approveUnion`
- `client/src/app/features/reviews/reviews.service.ts` — added `moderate`
- `client/src/app/app.routes.ts` — added `/admin` (`roleGuard(['Admin'])`) with `''`, `sellers`,
  `unions`, `reviews`, `users` children
- `client/src/app/core/layout/navbar/navbar.component.html` — Admin-only "Admin Dashboard" link
- `client/src/app/app.config.ts` — registered `Flag`, `UserCog` icons
- `docs/BACKEND_PROGRESS.md`, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`,
  `docs/API_PROGRESS.md`

**No new app bugs found** — both backend and frontend built clean on the first pass and live
verification hit no real product defects. Two bugs were found and fixed in the Playwright
verification script itself, and are recorded here for transparency rather than left implicit:
1. An ambiguous `page.click('button:has-text("Approve")')` matched the first of two "Approve"
   buttons on the Pending Sellers page (a second, pre-existing pending seller with zero KYC
   documents was also on the page) — the resulting `400 Bad Request` looked at first like a
   possible app bug. Confirmed via direct `curl` against `GET /api/v1/sellers/pending` that the
   backend's rejection (`BusinessRuleException`: "Cannot approve a seller who has not submitted
   any KYC documents") was correct behavior for the seller the script had actually clicked. Fixed
   by scoping the click to `.admin-pending__card` filtered by the intended seller's name.
2. After that fix, a second full run hit a `locator.click: Timeout` waiting for a "Verify" button
   that no longer rendered, because the document was already `Verified` from the first run's
   partial progress. Fixed by making the script tolerate either starting state
   (`if (await verifyBtn.count() > 0) { ...click... }`).

**Verified live end-to-end** as the seeded `admin@jewelryhub.local` account: dashboard rendered
real platform counts → verified a KYC document and approved a test seller ("Admin Verify Jewels")
→ hid then restored a customer review → searched for and deactivated then reactivated a user
account → confirmed a signed-in Seller account is redirected away from `/admin`. Zero console
errors on the final run.

This closes out the entire originally planned v1 roadmap — Customer, Seller, Union, and Admin are
all now built and verified live end-to-end.

---

## 2026-08-07 — Union UI: Meetings + Governance Polls (Fixed a Real Layout Bug, Verified Live)

**Module**: Frontend (`client/`, new `features/unions/meeting-form/`, `meeting-detail/`,
`poll-form/`, `poll-detail/`, `meetings.service.ts`, `polls.service.ts`) + demo data in the live DB

**Scope**: closes out Phase 16a, deliberately deferred from the core Union module pass. Pure
frontend — `UnionMeetingsController` and `UnionPollsController` were already complete.

**Files modified**:
- `client/src/app/features/unions/models.ts` — added `MeetingStatus`/`AgendaItemStatus`/
  `MeetingAttendanceStatus`/`ActionItemStatus`/`PollStatus` enums + labels, and every Meeting/Poll
  DTO and request interface
- `client/src/app/features/unions/meetings.service.ts`, `polls.service.ts` (new)
- `client/src/app/features/unions/meeting-form/`, `meeting-detail/`, `poll-form/`, `poll-detail/`
  (new)
- `client/src/app/features/unions/union-detail/` — added Meetings and Polls tabs
- `client/src/app/features/unions/my-unions/` — added a "My Action Items" section
- `client/src/app/app.routes.ts` — added `/unions/:unionId/meetings/new`+`/:meetingId` and
  `/unions/:unionId/polls/new`+`/:pollId`
- `client/src/app/app.config.ts` — registered `CalendarDays`, `Vote` icons
- `docs/FRONTEND_PROGRESS.md`, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`

**Frontend bug found and fixed**: the Record Minutes form's action-item row packed a description
input, a responsible-member `mat-select`, a due-date input, and a delete button into a single flex
row with `flex: 1` on each field and no minimum width — at normal viewport widths this squeezed
the dropdown and date field down to roughly 90px, making them illegible and (incidentally) hard
for Playwright to click reliably. Fixed with a responsive grid (`2fr 1.5fr 1fr auto`, collapsing
to one column below 640px).

**Test-tooling quirks investigated and ruled out as real bugs**: verification twice hit what
looked like broken data binding — a `mat-select` that wouldn't open and a `mat-radio-button` that
wouldn't select, both via Playwright's default `.click()` on the component's host element in
headless Chromium. Rather than assume the app was broken, inspected the DOM directly
(`aria-checked`, `outerHTML`) and confirmed clicking the actual native `<input>` inside each
component toggled it correctly and the Angular bindings (`ngModel`, `[disabled]`) responded
exactly as coded — these were Playwright/Material interaction quirks, not app bugs, and the fix
was in the test script, not the product.

**Verified live end-to-end**: as the founding officer, scheduled a meeting with a two-item agenda
(every active member auto-invited), recorded minutes with a decision summary and an action item
assigned to another member, and confirmed that member could RSVP; created a poll as `Draft`,
opened it, had a second member vote, confirmed the tally updated correctly and a second vote
attempt was rejected with the backend's real "already voted" error; confirmed the assigned action
item appeared under that member's "My Action Items" on `/unions/mine`. Zero console errors on the
final run.

This closes the full Union module (Phase 11 backend through Phase 16a frontend). Only the Admin
module remains on the roadmap.

---

## 2026-08-07 — Union UI: Directory, Create/Join, Membership, Announcements/Documents/Events (Fixed a Real Auth-Gating Bug, Verified Live)

**Module**: Frontend (`client/`, new `features/unions/`) + demo data in the live DB

**Scope decision**: the Union backend is large (core union + Meetings + Governance Polls). Asked
the user how to scope this pass; they chose "core union only" — browse/create/join, membership
management, officer-gated announcements/documents/events — deferring Meetings and Polls to a
follow-up phase (Phase 16a) to keep this pass sized similarly to the Seller module.

**Files modified**:
- `client/src/app/features/unions/` (new) — `models.ts`, `unions.service.ts`, `union-list/`,
  `union-create/`, `union-detail/` (`MatTabsModule`: Overview/Members/Announcements/Documents/
  Events), `my-unions/`
- `client/src/app/app.routes.ts` — added `/unions`, `/unions/new` and `/unions/mine`
  (Seller-guarded), `/unions/:id` (public)
- `client/src/app/core/layout/navbar/` — a public "Unions" top-level nav link, "My Unions" in the
  account menu for Sellers
- `client/src/app/app.config.ts` — registered new Lucide icons (`Crown`, `FileText`, `Landmark`,
  `Pin`, `UserPlus`, `Users`)
- `docs/FRONTEND_PROGRESS.md`, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`

**Frontend bug found and fixed**: `UnionsController`'s `members`, `announcements`, and `documents`
endpoints are `[Authorize]` (any logged-in user, but not anonymous) while `events` is
`[AllowAnonymous]`. `UnionDetailComponent` fetched all four unconditionally regardless of login
state, so an anonymous visitor correctly got 401s on three of them — which then triggered a
spurious `AuthService.refresh()` call that threw "No refresh token available" for a session that
never existed, surfacing as an uncaught `ResourceValueError` in the console. Initially
misdiagnosed as a concurrent-refresh race, since this is the first page in the app to fire ~5
simultaneous authenticated requests on load — ruled that out by firing 5 concurrent authenticated
requests directly at the backend via `curl` (all 5 succeeded cleanly), then found the real cause
by logging actual request/response pairs from the browser, which showed the failures correlated
with signed-out state, not timing. Fixed by gating those three resources on
`auth.isAuthenticated()` and showing a "Log in to view this" prompt in their place for anonymous
visitors.

**Verified live end-to-end**: registered a KYC-approved seller and created a union → approved it
as the dev admin → a second KYC-approved seller requested to join → the founding President
approved the request, then published an announcement, uploaded a document, and scheduled an event
as an officer → confirmed all of it renders correctly both for the signed-in officer and for an
anonymous visitor (who sees the public directory, the union's public info, and events, but a
"log in" prompt for members/announcements/documents). Zero console errors on the final run.

**Recurring environment quirk observed, not fully explained**: across this and previous
verification passes, several unrelated accounts (the dev admin twice, two different test
sellers) intermittently stopped being able to log in with their known-correct password —
sometimes with `IsLockedOut = true` and a stale `AccessFailedCount`, sometimes with no lockout at
all, just a non-matching password hash. The seed-only `SeedDevAdminAsync` path was ruled out (it
never touches an existing row). Each time, the fix was the same: register a throwaway account
with the desired password through the live `/auth/register` endpoint (so the app's own
`BCryptPasswordHasher` produces a valid hash) and copy that hash onto the affected account's row
directly via `sqlcmd`. Flagged in `docs/FRONTEND_PROGRESS.md` as worth investigating if it recurs.

---

## 2026-08-07 — Customer UI: Order History + Reviews (Fixed a Real CSS Bug, Verified Live)

**Module**: Frontend (`client/`, new `features/reviews/` and `features/orders/order-history/`) +
demo data in the live DB

**Files modified**:
- `client/src/app/features/orders/order-history/` (new) — paginated order list
  (`GetMyOrdersQuery`), links into the existing `order-detail/` page
- `client/src/app/features/reviews/` (new) — `models.ts`, `reviews.service.ts`
  (`getForProduct`/`create`)
- `client/src/app/features/orders/order-detail/` — added an inline "Write a Review" form per
  `Delivered` item (star buttons for product/seller rating, optional title/comment)
- `client/src/app/features/products/product-detail/` — added a "Customer Reviews" section
  (`GetProductReviewsQuery`)
- `client/src/app/app.routes.ts` — added `/orders` (Customer-guarded)
- `client/src/app/core/layout/navbar/navbar.component.html` — "My Orders" link in the account
  menu when `auth.hasRole('Customer')`
- `docs/FRONTEND_PROGRESS.md`, `docs/PROJECT_STATUS.md`, `docs/ROADMAP.md`

**Frontend bug found and fixed**: the interactive rating stars (both the review-submission form
and the review list) looked unfilled regardless of their actual state. Lucide's `Star` icon
renders `fill="none"` by default (it's a stroke-only outline icon), so changing its CSS `color`
for a "filled" star only recolors the stroke, not the interior — visually indistinguishable from
an "unfilled" star at a glance. Fixed by adding `::ng-deep svg { fill: currentColor }` to the
`--filled` state's styles in both `order-detail.component.scss` and
`product-detail.component.scss`.

**Two accounts needed a manual password fix directly in the local dev database** to complete live
verification, since there's no self-service password-reset flow: the dev admin account
(`admin@jewelryhub.local`) had `IsLockedOut = true` with a stale `AccessFailedCount`/
`LockoutEndUtc` combination left over from earlier session activity, and the
`customer-order-verify@example.com` test account (created during the previous Seller module
verification pass) had a password that no longer matched its stored hash. Both were fixed the same
way: register a brand-new throwaway account through the live `/auth/register/customer` endpoint
with the desired password (so the app's own `BCryptPasswordHasher` produces a correctly-formed
hash), then copy that hash onto the target account's row directly via `sqlcmd`. No password
hashing was reimplemented by hand. The two throwaway accounts couldn't be hard-deleted afterward
(one has a dependent `Customer` foreign-key row) and were left in place as harmless demo-data
clutter.

**Verified live end-to-end**: as `customer-order-verify@example.com`, opened Order History, opened
the one Delivered order, submitted a product + seller review with a title and comment — the review
immediately appeared on the product's detail page with the correct star rating, author, and
comment, and the product's aggregate rating updated to reflect it. Resubmitting a review for the
same item correctly surfaced the backend's real `BusinessRuleException` ("You have already
reviewed this purchase.") via the global error interceptor, rather than silently failing or
crashing. Zero console errors and zero unexpected HTTP error responses on the final run.

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
