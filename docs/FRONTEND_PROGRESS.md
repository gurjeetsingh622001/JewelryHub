# Frontend Progress

## Status: Full Customer + Seller + Union + Admin UI Complete, Plus Wishlist and Real File Upload

Last updated 2026-08-11. An Angular workspace exists at `client/` (sibling to `src/`, not part of
`JewelryHub.sln` since it isn't a .NET project). The entire Customer-facing storefront (auth, Home,
product browsing, cart, wishlist, checkout, order history, reviews), the entire Seller module
(dashboard, KYC, product/inventory management, order fulfillment, real photo/document upload), the
entire Union module (directory, create/join, membership management, announcements/documents/
events, meetings, governance polls), and the entire Admin module (dashboard, pending sellers/
unions, review moderation, user management) are built — all wired to the live backend APIs and
verified end to end against an actual running backend + database, not just a build. **Building
Checkout surfaced a hard backend blocker** (no way to ever create a `CustomerAddress`),
**verifying Cart live surfaced a real backend bug (twice — a different root cause each time, the
second also found in Wishlist)**, **verifying the Seller Order Fulfillment queue live surfaced
another real backend bug** (marking an item Shipped always 500'd), **verifying Reviews live
surfaced a real frontend CSS bug** (Lucide's stroke-only `Star` icon needs an explicit `fill`
override to actually look "filled"), **verifying the Union module live surfaced a real frontend
auth-gating bug** (three of its four detail tabs call `[Authorize]`-only endpoints, fetched
unconditionally regardless of login state), and **verifying Union Meetings live surfaced a real
layout bug** (an action-item form row squeezed three fields into an unusable width) — see Known
Gaps. Worth remembering: "the spec says it's done" and "builds and looks right" are both different
from "someone actually tried to use it end to end." Nothing planned is outstanding — further work
needs fresh direction, see [ROADMAP.md](ROADMAP.md).

## Design System

See [DESIGN_SYSTEM.md](DESIGN_SYSTEM.md) — the permanent visual/UX reference (colors, type,
spacing, component/library boundaries, icons, imagery sourcing). Every screen follows it. Brand
direction: a premium editorial jewelry boutique (Cartier/Tiffany/Swarovski/Pandora/Blue Nile as
inspiration only, never copied) — warm ivory + near-black ink + a single gold accent, Playfair
Display serif headings over Inter body copy, restrained motion, real curated Unsplash photography
(never placeholder boxes).

## Stack

- **Angular 22**, standalone components (no NgModules), signals for local/service state.
- **UI**: Angular Material (buttons, form fields, menus, button-toggles, snack bars) **+**
  Tailwind CSS v4 for layout/spacing utilities, styled to the design system's tokens via
  `--mat-sys-*` overrides. Tailwind's Preflight base-reset is deliberately excluded (`styles.scss`
  imports `tailwindcss/theme` + `tailwindcss/utilities` separately, not the `tailwindcss`
  shorthand) so it doesn't fight Material's own base styles.
- **PrimeNG was removed (2026-08-05).** It was part of the original 3-library stack choice, but
  the installed version (22.x) requires a paid PrimeUI license — without one it injects an
  "Invalid PrimeUI License" banner into every page via a closed shadow-DOM element
  (`primeng/fesm2022/primeng-license.mjs`, cryptographically verified, not a bug or something
  hideable with CSS). Rather than pay for a license or ship a broken-looking banner, PrimeNG,
  `@primeuix/themes`, and `primeicons` were uninstalled entirely and every usage swapped for a
  Material equivalent: `Toast`/`MessageService` → `MatSnackBar`; `SelectButton` (the
  Customer/Seller toggle on the register page) → `MatButtonToggleGroup`. See
  [DESIGN_SYSTEM.md](DESIGN_SYSTEM.md) § Components for the full note. If a real PrimeNG license
  is purchased later, re-adding it is straightforward.
- **Icons**: Lucide (`lucide-angular`) exclusively, registered once in `app.config.ts` via
  `LucideAngularModule.pick({...})`. Installed with `--legacy-peer-deps` since its peer range
  hasn't caught up to Angular 22 yet — functions correctly regardless (verified via a full build).
- **HTTP**: `provideHttpClient` with two functional interceptors, `authInterceptor` (attaches the
  Bearer token, catches a 401, refreshes once via a shared/deduped `Observable` so concurrent
  401s don't each trigger their own `/auth/refresh` call, retries the original request) and
  `errorInterceptor` (last-resort `MatSnackBar` for any error a component didn't handle itself;
  skips 401s since authInterceptor already owns that flow).
- **Auth storage**: access + refresh tokens and the current user in `localStorage` via
  `TokenStorageService` (the only thing allowed to touch it directly) — not an httpOnly cookie,
  because the backend's `/auth/refresh` expects the raw refresh token in the request body, not a
  cookie. This is a plain SPA-calls-API setup, not a BFF.
- **Route guards**: `authGuard` (must be logged in) and `roleGuard(['Seller', 'Admin'])` (must
  hold one of the given roles — mirrors the backend's `[Authorize(Roles = "...")]` convention).
  The Home route is intentionally **not** guarded — a storefront landing page is public, same as
  Cartier/Tiffany/Blue Nile; `authGuard` is reserved for account-specific pages (orders, wishlist)
  once they exist.
- **Fragment scrolling**: `provideRouter` uses `withInMemoryScrolling({ anchorScrolling: 'enabled' })`
  so the navbar/footer's category links (e.g. `/#category-rings`) actually scroll to the matching
  `id` on the Home page.

## What's Built

- `core/auth/` — `models.ts`, `token-storage.service.ts`, `auth.service.ts` (signals:
  `currentUser`, `isAuthenticated`, `roles`), `auth.guard.ts`, `auth.interceptor.ts`.
- `core/http/` — `problem-details.ts` (the RFC 7807 shape `ExceptionHandlingMiddleware` returns),
  `error.interceptor.ts` (now `MatSnackBar`-based).
- `core/layout/navbar/` — gold-tint announcement strip (real backend fact: free shipping over
  ₹5,000, matching `FlatRateShippingCalculator`'s threshold) + a sticky main bar (nav links,
  centered serif wordmark, search/wishlist/cart/account icon cluster) that gains a shadow on
  scroll, plus a charcoal off-canvas mobile panel. The cart icon is a real `routerLink="/cart"`
  with a live item-count badge (`CartService.itemCount()`); search/wishlist aren't built yet, so
  they still surface an honest "coming soon" `MatSnackBar`.
- `core/layout/footer/` — trust-signal row (shipping/security/craftsmanship/sourcing), brand +
  social links, sitemap columns, a newsletter form (also "coming soon" — no backend endpoint for
  it), copyright bar.
- `core/layout/shell.component.ts` — composes Navbar + `<router-outlet>` + Footer (previously an
  inline Material toolbar; replaced now that Navbar/Footer exist).
- `features/home/` — hero (full-bleed photo, serif headline, primary + `secondary-on-dark` CTAs),
  a 4-tile category grid (`id`-anchored for the nav's fragment links), a 3-up featured-pieces
  grid, and a brand-story split section with stats. Illustrative content only — Products/
  Categories APIs exist on the backend but the Customer UI isn't wired to them yet (Phase 13b).
- `core/layout/auth-layout/` — shared split-screen layout for Login/Register (see
  [DESIGN_SYSTEM.md](DESIGN_SYSTEM.md) § Auth Pages). Both pages are top-level routes outside the
  storefront Shell (`app.routes.ts` nests Home/Forbidden under a `ShellComponent` parent route
  instead) — no Navbar/Footer on auth pages by design.
- `features/auth/login/`, `features/auth/register/` (Customer/Seller toggle now a
  `MatButtonToggleGroup`) — redesigned to the design system: serif headline, eyebrow label,
  editorial photo + quote panel, no more generic `mat-card`.
- `features/home/`, `features/forbidden/`.
- `features/products/` — `models.ts` (mirrors `ProductListItemDto`/`ProductDto`/`CategoryDto`
  field-for-field, including numeric `MetalType`/`PurityType`/`ProductStatus`/`ProductSortOption`
  enums — the backend has no `JsonStringEnumConverter`, so `System.Text.Json`'s default applies:
  enums serialize as their ordinal, not a string; these must stay in exact sync with the C# enum
  definitions), `products.service.ts` (`getProducts`/`getProductById`/`getCategories`),
  `product-list/` (URL-driven filters via `rxResource` + `toSignal(route.queryParamMap)` — search,
  category, metal type, min/max price, sort, pagination — so the page is bookmarkable/shareable
  and Navbar/Home's category links drive it directly), `product-detail/` (gallery with thumbnail
  strip, spec table, gemstone list, a quantity stepper, a real Add to Cart, and — since
  2026-08-11 — a real Add to Wishlist).
- `shared/product-card/` — the reusable listing card (discount badge, out-of-stock badge,
  wishlist button, rating, price with strikethrough original), used by the Product List grid and
  intended for any future "you may also like"/search-result surface.
- `core/models/paged-result.ts` — mirrors the backend's `PagedResult<T>`, generic across every
  paged endpoint.
- `features/cart/` — `models.ts` (mirrors `CartDto`/`CartItemDto`), `cart.service.ts` (signal-based
  state; an `effect()` refreshes the cart the moment `AuthService` reports an authenticated
  Customer, and clears it on logout/role change — the backend's `CartController` is
  `[Authorize(Roles = "Customer")]`, so Sellers/Admins never have one), `cart-page/` (line items
  with an inline quantity stepper and remove button, order summary sidebar, empty state, "Proceed
  to Checkout" now a real `routerLink="/checkout"`). The Navbar's cart badge and Product Detail's
  Add to Cart both read/write through this same service, so every surface stays in sync without
  manual event wiring.
- `features/addresses/` — `models.ts` (mirrors `CustomerAddressDto`), `addresses.service.ts`
  (`getMyAddresses`/`create` — Update/Delete exist on the backend but have no frontend caller yet,
  since Checkout only ever needs to list and add).
- `features/orders/` — `models.ts` (mirrors `OrderDto`/`OrderItemDto`/etc. plus numeric
  `OrderStatus`/`PaymentMethod`/`PaymentStatus`/`ShipmentStatus` enums, same no-`JsonStringEnumConverter`
  reasoning as Products), `orders.service.ts` (`checkout`/`getById`/`getMyOrders`/`confirmPayment`/
  `cancel`), `checkout/` (address picker with an inline "add new address" form that appears
  automatically when the customer has none yet, a payment-method pill selector, an order summary
  sidebar, defaults billing to the same address as shipping — no separate billing-address UI in
  v1), `order-detail/` (doubles as the post-checkout confirmation page via a `?placed=true` query
  param and as a general order lookup — items, shipping address, payment, cost breakdown, a Cancel
  Order button shown only when the order's status is actually cancellable, and — added
  2026-08-07 — a "Write a Review" inline form per `Delivered` item: star buttons for product and
  seller ratings plus optional title/comment, calling `ReviewsService.create`. There's no
  server-side "already reviewed" flag on `OrderItemDto`, so the button just hides itself locally
  (a `Set<string>` of reviewed item ids) after a successful submit; on a fresh page load it
  reappears, and resubmitting correctly surfaces the backend's real `BusinessRuleException`
  ("You have already reviewed this purchase.") via the global error interceptor rather than
  silently failing. Checkout calls `OrdersService.confirmPayment` immediately after placing an
  order for every payment method except Cash on Delivery, standing in for a real payment gateway
  webhook (see `ConfirmPaymentCommand`'s backend remarks) since there's no real gateway to redirect
  to yet.
- `features/orders/order-history/` (added 2026-08-07) — a paginated list of the customer's own
  orders (`GetMyOrdersQuery`, 10 per page), each row linking into `order-detail/`. Routed at
  `/orders` (`roleGuard(['Customer'])`); Navbar's account menu gains a "My Orders" link when
  `auth.hasRole('Customer')`.
- `features/reviews/` (added 2026-08-07) — `models.ts` (mirrors `ReviewDto`/`CreateReviewCommand`),
  `reviews.service.ts` (`getForProduct`/`create`). Consumed by Order Detail (writing a review) and
  Product Detail (displaying them — a "Customer Reviews" section below the main product layout:
  per-review star rating, author first name, date, title, comment, and any seller response).
- `features/seller/` — `models.ts` (mirrors `SellerDto`/`SellerDocumentDto`/`SellerOrderItemDto`
  plus numeric `SellerVerificationStatus`/`DocumentVerificationStatus` enums), `seller.service.ts`
  (`getMyProfile`/`submitDocument`/`createProduct`/`updateProduct`/`adjustInventory`/
  `getOrderQueue`/`updateItemStatus`). Routed under `/seller` (`roleGuard(['Seller'])`):
  - `dashboard/` — KYC status badge + banner (links to the KYC page when not yet Approved),
    revenue/orders-fulfilled/rating stat cards, quick-link cards to Products/Orders/KYC.
  - `kyc/` — a document-type + URL submission form and a list of previously submitted documents
    with their review status.
  - `products/` — a list of the seller's own products (via `ProductsService.getProducts({ sellerId
    })`, since `CreateProductCommand` always sets `Status = Active` there's no draft/pending state
    to filter around yet) with an "Add Product" entry point; `product-form/` is one component
    serving both `/seller/products/new` (full `CreateProductCommand` field set — category, SKU,
    metal/purity/weights/pricing, hallmark, one optional image URL; gemstones aren't exposed in v1)
    and `/seller/products/:id/edit` (the `UpdateProductCommand` subset, plus a plain quantity-delta
    inventory adjuster calling `AdjustInventoryCommand` directly, no dedicated stock-history view).
  - `orders/` (`seller-orders.component`) — the fulfillment queue (`GetSellerOrderItemsQuery`),
    a status filter, and one action button per row that advances the item to the next status. The
    Shipped transition opens an inline Carrier + Tracking Number form instead of firing
    immediately, since the backend's `UpdateOrderItemStatusCommand` validator requires both fields
    only for that transition.
  - Navbar's account menu gains a "Seller Dashboard" link when `auth.hasRole('Seller')`.
- `features/unions/` (core scope added 2026-08-07, Meetings + Polls added 2026-08-07) — `models.ts`
  (mirrors every Union/Meeting/Poll DTO plus numeric `UnionMemberRole`/`UnionMembershipStatus`/
  `UnionEventStatus`/`MeetingStatus`/`AgendaItemStatus`/`MeetingAttendanceStatus`/
  `ActionItemStatus`/`PollStatus` enums and an `OFFICER_ROLES` set mirroring the backend's
  `UnionAuthorization.OfficerRoles`), `unions.service.ts` (core-scope endpoints —
  browse/create/join/members/pending-members/review-membership/remove-member/announcements/
  documents/events/event-status), `meetings.service.ts` and `polls.service.ts` (one each per
  backend controller).
  - `union-list/` — public directory (`GET /unions`, admin-approved only per the backend query),
    search by name, a "Start a Union"/"My Unions" pair shown only to Sellers.
  - `union-create/` — Seller-only creation form (name/description/logo/city/state/annual fee) —
    the founder becomes President and the union starts unapproved.
  - `union-detail/` — public (`/unions/:id`), `MatTabsModule` tabs: Overview (description, founder,
    established date, fee), Members (roster + a Pending Requests section with Approve/Reject,
    visible only to officers), Announcements/Documents/Events (list + an officer-only inline
    create form per tab, plus delete on announcements and cancel on upcoming events), and — added
    2026-08-07 — Meetings and Polls (summary lists linking to their own detail pages, an officer-
    only "New Meeting"/"New Poll" entry point; both resources gated on active membership, matching
    `GetMeetingsQuery`/`GetPollsQuery`'s own server-side check, to avoid a guaranteed
    `BusinessRuleException` for a logged-in non-member). Officer status (`isOfficer`) and
    membership status (`isActiveMember`/`isPendingMember`/`canJoin`) are all derived client-side
    from `GetMyMembershipsQuery`, filtered to the current union.
  - `my-unions/` — a seller's own memberships; since `UnionMemberDto` doesn't carry the union's
    name/city, each row's union is fetched via a `forkJoin` over `getById` calls (acceptable at
    the scale of "unions one seller belongs to," not paginated). Added 2026-08-07: a "My Action
    Items" section (`GetMyActionItemsQuery`) listing the seller's open action items across every
    union they belong to.
  - `meeting-form/` (added 2026-08-07) — officer-only, `/unions/:unionId/meetings/new`: title,
    location, virtual link, schedule, duration, and a dynamic agenda-topic list (add/remove rows).
  - `meeting-detail/` (added 2026-08-07) — `/unions/:unionId/meetings/:meetingId`: agenda items,
    the full attendee list with RSVP status, an RSVP widget (Confirm/Can't Attend) for the caller's
    own invite, officer-only meeting-status controls (Start/Cancel/Complete — the only transitions
    that make sense from `Scheduled`/`Ongoing`), and an officer-only "Record Minutes" form
    (optional agenda-item link, decision summary, discussion notes, a dynamic list of action items
    each assigned to an attendee with an optional due date). Agenda-item status and action-item
    status are both read-only here — the backend has no endpoint to change either.
  - `poll-form/` (added 2026-08-07) — officer-only, `/unions/:unionId/polls/new`: question, a
    single/multi-select toggle, an optional close date, and a dynamic 2+ option list; created as
    `Draft` per the backend's design (an officer must separately open it).
  - `poll-detail/` (added 2026-08-07) — `/unions/:unionId/polls/:pollId`: a radio (single-select)
    or checkbox (multi-select) ballot shown only while the poll is `Active` and the caller is an
    active member, live vote-count results with a simple proportional bar per option, and
    officer-only Open/Close controls. Like Reviews, there's no per-member "already voted" flag on
    `PollDto`, so the ballot hides itself locally after a successful submit (a session-only signal)
    and a revote correctly surfaces the backend's real "You have already voted in this poll." error
    rather than failing silently.
  - Navbar gains a public "Unions" top-level link and a "My Unions" account-menu entry for Sellers.
- `environments/` — `apiUrl` pointing at `https://localhost:65334/api/v1` in development, a
  relative `/api/v1` default for production.

## Imagery

10 curated Unsplash photos total (free tier, Unsplash License, verified individually — several
candidates were rejected either for being Unsplash+ paid photos or, in one case, for visibly
showing a real jewelry brand's name ("dinh van") printed on the display risers in the source
photo, which was replaced before shipping). 7 are used across Home/Login/Register (see
`HomeComponent` for the exact CDN URLs); 3 more were sourced for the demo products seeded
2026-08-06 (gold hoop earrings, diamond stud earrings, a pair of gold bangles).

## Not Yet Wired Up / Known Gaps

- **Verified against a live backend across three passes.** 2026-08-05: registered a seller,
  completed the KYC approve flow as the dev admin, created a category, and created three real
  products via the live API, then confirmed Product List/Detail render correctly end-to-end.
  2026-08-06 (Cart): exercised the full Cart flow live (add → navbar badge → view → update
  quantity → remove → empty state) with a freshly-registered customer — **this run caught a real
  backend bug** (`AddToCartCommand` threw a 500 on every add, a no-tracking/tracking EF Core
  mismatch) and a frontend one (three Lucide icons used in the new UI were never registered).
  2026-08-06 (Checkout): building Checkout surfaced a **hard backend blocker** before any UI
  verification was even possible — there was no endpoint anywhere to create a `CustomerAddress`,
  and `CreateOrderCommand` requires one. Built `CustomerAddressesController` (see
  [API_PROGRESS.md](API_PROGRESS.md)) to unblock it, then verified the full flow live: add to
  cart → checkout (auto-selected the customer's default address, defaulted to UPI) → place order
  → auto-confirm payment → land on the order confirmation page with the correct status, items,
  address, and cost breakdown → cart empties. Zero console errors throughout. Login/Register
  still haven't been round-tripped live (only Products/Categories/Cart/Checkout were exercised).
  2026-08-06 (Seller module): registered a fresh seller account, submitted a KYC document,
  approved both the document and the seller as the dev admin, listed a product, had a separately
  registered test customer buy and pay for it, then advanced the order through every fulfillment
  status (Processing → Shipped → Delivered) as the seller — the Dashboard's revenue/orders-
  fulfilled stats updated correctly afterward. **This run caught a real backend bug**
  (`UpdateOrderItemStatusCommand` 500'd on the Shipped transition — see
  [API_PROGRESS.md](API_PROGRESS.md)) and two frontend ones: a `GET /products/null` request fired
  on the "new product" route because `rxResource`'s `params` only skips its loader on `undefined`,
  not `null` (the route's id param is legitimately `null` in create mode); and the KYC form showed
  every required field as invalid immediately after a successful submit, because `form.reset()`
  clears values but not `FormGroupDirective`'s internal "submitted" flag — Material's default
  `ErrorStateMatcher` then treats every empty required field as touched-and-invalid. Fixed via
  `resetForm()` on the directive instead of `form.reset()` on the `FormGroup`.
  2026-08-07 (Order History + Reviews): reviewed a `Delivered` item on `customer-order-verify@example.com`'s
  order → the review appeared correctly on the product's detail page with the right star rating,
  title, and comment → resubmitting a review for the same item correctly surfaced the backend's
  real "You have already reviewed this purchase." error via the global error interceptor (expected
  behavior given no client-side "already reviewed" flag exists, not a bug). **This run caught a
  real CSS bug**: the interactive rating stars (Order Detail's review form, Product Detail's
  review list) looked identical whether "filled" or not, because Lucide's `Star` icon renders
  `fill="none"` by default — a `color` change alone only affects the stroke. Fixed by adding
  `::ng-deep svg { fill: currentColor }` to the `--filled` state in both components.
  2026-08-07 (Union module): created a union as a KYC-approved seller → admin-approved it as the
  dev admin → a second seller requested to join → approved as the founding President → published
  an announcement, uploaded a document, and scheduled an event as an officer — all render correctly
  for a signed-in officer. **This run caught a real frontend bug**: viewing the union detail page
  while signed out threw three console 401s (Members/Announcements/Documents are backend
  `[Authorize]` endpoints — any logged-in user, but not anonymous — while Events is
  `[AllowAnonymous]`; the page fetched all four unconditionally) which then cascaded into a
  spurious `AuthService.refresh()` call throwing "No refresh token available" for a session that
  never existed. Initially misdiagnosed as a concurrent-refresh race (the union detail page is the
  first in the app to fire ~5 simultaneous authenticated requests on load) — ruled that out by
  firing 5 concurrent authenticated requests directly at the backend via curl (all succeeded), then
  found the real cause by logging the actual request/response pairs, which showed the failures
  correlated with signed-out state, not concurrency. Fixed by gating the three endpoints on
  `auth.isAuthenticated()`.
  2026-08-11 (Wishlist UI + real file upload): reported live bug — add-to-cart 500ing again — was
  fixed first (a second, different root cause than the 2026-08-06 fix, see
  [API_PROGRESS.md](API_PROGRESS.md)), which prompted building out the two gaps flagged while
  investigating: Wishlist had a complete backend and zero frontend, and there was no real file
  upload anywhere in the app. Built `WishlistService` + `/wishlist` page + wired the Navbar/
  product-card/product-detail wishlist buttons — **this run caught a real backend bug**
  (`AddToWishlistCommand`, identical shape to the cart bug, found live rather than reported).
  Built a real file-picker upload flow for the Seller product form and KYC form (previously both
  required pasting an already-hosted URL), backed by a new generic Uploads API; added a shared
  `resolveMediaUrl()` helper since uploaded files come back as a relative path that needs
  resolving against the API's origin in dev (the Angular dev server and API run on different
  ports). Verified live: both cart-bug paths and both wishlist-bug paths now succeed; a real
  product photo uploaded during creation renders correctly; a real KYC document uploaded and
  submitted, with a working "view document" link; extension/size validation and the Customer-role
  403 all fire correctly. Zero console errors on the final run.
  2026-08-07 (Union Meetings + Polls): as the founding officer, scheduled a meeting with a
  two-item agenda (auto-inviting every active member), recorded minutes with an action item
  assigned to another member, and confirmed a second member could RSVP; created a poll as Draft,
  opened it, had the second member vote, confirmed the tally updated and a revote was correctly
  rejected; confirmed the assigned action item appeared under that member's "My Action Items."
  **This run caught a real layout bug**: the Record Minutes form's action-item row (description +
  responsible-member dropdown + due date + delete button, all one flex row) squeezed the dropdown
  and date field down to an unusable, largely illegible width at normal viewport sizes — fixed with
  a responsive grid that stacks to one column below 640px. Verification also hit two Playwright/
  Angular-Material interaction quirks along the way: clicking a `mat-select`'s or
  `mat-radio-button`'s host element (rather than its native `<input>`) silently no-ops in headless
  Chromium some of the time, leaving the bound model unchanged with no console error. Confirmed via
  direct DOM inspection (`aria-checked`, `outerHTML`) that the app's own data binding was correct
  and the underlying `<input>` toggling correctly — these were test-tooling artifacts, not app
  bugs, and needed no product code change (the click target in the test script was fixed instead).
- **Demo data was created directly against the live database**, across seven verification passes:
  five seller accounts (`seller.demo@jewelryhub.local`, `ananya.seller@jewelryhub.local`,
  `seller-test-verify@example.com`, `union-member-seller@example.com`,
  `union-member-seller2@example.com`), four categories (Rings, Necklaces, Earrings, Bracelets),
  eight products, a test customer (`checkout.smoke@example.com`) with one saved address and one
  placed order, a second test customer (`customer-order-verify@example.com`) whose order was
  carried all the way through Delivered and now has one review on it, one approved union
  ("Rajasthan Jewelers Guild" — 1 announcement, 1 document, 1 event, 3 active members, several
  test meetings each with recorded minutes and an action item, and a poll with one vote cast), and
  four throwaway `temp-hash-donor*` accounts (see below) with no orders/products of their own. Kept
  intentionally as ongoing sample data (user's decision, 2026-08-06) rather than cleaned up.
- **A much larger demo-data pass (2026-08-11)** was generated by a script driving the live API
  (registration/KYC-submit/admin-approve/create-product/create-union/join/approve-membership —
  real business flows, not direct SQL inserts) rather than by hand: 14 more KYC-approved sellers,
  18 more customers, 131 more products across the 4 existing categories with varied metal/purity/
  price/discount combinations, and 5 more admin-approved unions with 3-7 members each. Brought the
  dev database to 32 customers, 22 approved sellers, 142 products, 6 unions. Zero errors across the
  full scripted run; spot-checked live in the browser (product list pagination/filtering, union
  directory, Admin dashboard counts) with zero console errors.
- **Several accounts needed a manual password reset directly in the local dev database** across
  verification passes, since there's no self-service password-reset endpoint — this happened
  repeatedly enough (the dev admin twice, plus two different test sellers/customers) that it's
  worth flagging as a recurring, not-fully-explained environment quirk: a previously-working
  account's login would intermittently start failing with "Invalid email or password" despite no
  intentional change to its password, sometimes (`admin@jewelryhub.local`) with `IsLockedOut = true`
  and a stale `AccessFailedCount`/`LockoutEndUtc`, sometimes with no lockout at all — just a
  non-matching hash. The exact trigger was never conclusively identified (ruled out: the seed-only
  `SeedDevAdminAsync` path, which never touches an existing row). The reliable fix each time:
  register a brand-new throwaway account with the desired password through the live
  `/auth/register` endpoint (so the app's own `BCryptPasswordHasher` produces a correctly-formed
  hash — no password-hashing library was reimplemented by hand) and copy that hash onto the target
  account's row directly via `sqlcmd`. The four throwaway accounts
  (`temp-hash-donor@example.com` through `temp-hash-donor4@example.com`) couldn't be hard-deleted
  afterward (one has a dependent `Customer` FK row) and were left in place as harmless clutter
  rather than risk a cascading delete. **Worth investigating** if this recurs — possibly worth
  checking whether something outside the app (a backup/restore script, another process with DB
  access) is touching the `Users` table.
- **Checkout simplifications for v1**: billing address always equals shipping address (no separate
  billing UI); there's no way to edit or delete a saved address from the frontend yet (the backend
  supports both — see `AddressesService`); "confirm payment" is a client-side stand-in for a real
  gateway webhook, not an actual payment integration.
- **Seller module simplifications for v1**: product creation supports one uploaded photo (real
  file upload since 2026-08-11, via the new Uploads API) and no gemstones (the backend's
  `CreateProductCommand` supports full gemstone/multi-image lists); there's no way to change a
  product's photo after creation (the edit form has no image field);
  inventory adjustment is a single quantity-delta input with no adjustment history view; My
  Products has no pagination (fine at today's per-seller product counts, would need one before a
  seller has more than ~100 listings); there's no seller-facing analytics/reporting beyond the
  three Dashboard stat cards.
- **Reviews simplification for v1**: the "already reviewed" state is tracked client-side only for
  the current page session (a `Set<string>` of order-item ids), not read back from the server —
  there's no `hasReview`-style flag on `OrderItemDto` to check on load. A reloaded Order Detail
  page will show "Write a Review" again for an already-reviewed item; submitting just surfaces the
  backend's existing duplicate-review error rather than silently failing, so this is a UX rough
  edge, not a data-integrity issue.
- **Union module simplifications for v1**: member role changes and member removal have backend
  endpoints (`UpdateMemberRoleCommand`, `RemoveMemberCommand`) but no UI; "My Unions" fetches each
  membership's union via a `forkJoin` of individual `GET /unions/{id}` calls rather than a batch
  endpoint (fine at the scale of one seller's own memberships); every `datetime-local` input in the
  module (event/meeting scheduling, poll close date) is timezone-naive — no explicit timezone
  picker, just the browser's local time converted to UTC on submit. Meeting/Poll-specific: agenda-
  item status and action-item status are read-only in the UI because the backend has no endpoint
  to change either (`AgendaItemStatus` and `ActionItemStatus` are set at creation and never
  updated by any command); the poll ballot's "already voted" state is tracked client-side only for
  the current page session, same pattern and same reasoning as Reviews.
- No password-reset/email-verification screens (the backend doesn't have these endpoints either —
  see [API_PROGRESS.md](API_PROGRESS.md)).
- No global loading indicator, no HTTP retry/offline handling.
- Testing is a placeholder smoke test only (`app.spec.ts`) — matches the backend's stance of
  deferring real test coverage until more functionality exists.
- Home page's category tiles and hero CTA now link to real `/products` routes, but the "This
  Season's Edit" featured-pieces section and brand-story copy are still illustrative, not backed
  by live data.
- The Navbar/Home category links (Rings/Necklaces/Earrings/Bracelets) filter products via a text
  **search** match against the product name, not a real `categoryId` — there's no seeded mapping
  from "the four jewelry types" to actual admin-created Category rows yet. Revisit once category
  data is established.

## Per-Module Frontend Status

| Module | Status |
|---|---|
| Foundation (auth, routing, HTTP, shell) | ✅ Complete |
| Design system (`docs/DESIGN_SYSTEM.md`) | ✅ Complete |
| Customer UI — Home page | ✅ Complete (illustrative content, real nav) |
| Customer UI — Login/Register | ✅ Complete |
| Customer UI — Product browsing (list + detail) | ✅ Complete, verified live |
| Customer UI — Cart | ✅ Complete, verified live |
| Customer UI — Checkout + order confirmation | ✅ Complete, verified live |
| Customer UI — Order history (list), reviews | ✅ Complete, verified live |
| Seller UI — Dashboard, KYC, Products, Order Fulfillment | ✅ Complete, verified live |
| Union UI — Directory, Create/Join, Membership, Announcements/Documents/Events | ✅ Complete, verified live |
| Union UI — Meetings, Governance Polls | ✅ Complete, verified live |
| Admin UI | ✅ Complete, verified live (added 2026-08-11) |
| Wishlist UI | ✅ Complete, verified live (added 2026-08-11) |
| Real image/document upload (Seller product photo, KYC document) | ✅ Complete, verified live (added 2026-08-11) |

## How to Run

```bash
cd client
npm install   # re-run after pulling new commits
npm start     # ng serve — http://localhost:4200
```

Requires the API running at the URL in `src/environments/environment.development.ts`
(`https://localhost:65334` by default) with CORS allowing `http://localhost:4200` — already the
default in `appsettings.json`.

## Maintenance

Update this file whenever a feature module moves from ❌ to 🚧/✅, or the stack/library choices
change. Keep [DESIGN_SYSTEM.md](DESIGN_SYSTEM.md) in sync whenever a genuinely new visual pattern
is introduced.
