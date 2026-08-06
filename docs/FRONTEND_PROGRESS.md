# Frontend Progress

## Status: Foundation Through Seller Module Complete

Last updated 2026-08-06. An Angular workspace exists at `client/` (sibling to `src/`, not part of
`JewelryHub.sln` since it isn't a .NET project). The auth flow, HTTP layer, routing, a full
premium design system, the public Home page, redesigned Login/Register pages, real product
browsing (list + detail), a real shopping cart, a full checkout flow (address book, payment
method, place order, confirmation), and a full Seller module (dashboard, KYC, product/inventory
management, order fulfillment) are built — all wired to the live backend APIs and verified end to
end against an actual running backend + database, not just a build. **Building Checkout surfaced a
hard backend blocker** (no way to ever create a `CustomerAddress`), **verifying Cart live surfaced
a real backend bug**, and **verifying the Seller Order Fulfillment queue live surfaced another
real backend bug** (marking an item Shipped always 500'd) — see Known Gaps. Worth remembering:
"the spec says it's done" and "builds and looks right" are both different from "someone actually
tried to use it end to end." Order history (a list of past orders) and reviews don't exist yet for
Customers, nor do the Admin/Union dashboards — see [ROADMAP.md](ROADMAP.md) Phase 13e onward.

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
  strip, spec table, gemstone list, a quantity stepper, and a real Add to Cart — Wishlist is still
  an honest "coming soon" snack bar).
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
  Order button shown only when the order's status is actually cancellable). Checkout calls
  `OrdersService.confirmPayment` immediately after placing an order for every payment method
  except Cash on Delivery, standing in for a real payment gateway webhook (see
  `ConfirmPaymentCommand`'s backend remarks) since there's no real gateway to redirect to yet.
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
- **Demo data was created directly against the live database**, across four verification passes:
  three seller accounts (`seller.demo@jewelryhub.local`, `ananya.seller@jewelryhub.local`,
  `seller-test-verify@example.com`), four categories (Rings, Necklaces, Earrings, Bracelets), eight
  products, a test customer (`checkout.smoke@example.com`) with one saved address and one placed
  order, and a second test customer (`customer-order-verify@example.com`) whose order was carried
  all the way through Delivered during Seller module verification. Kept intentionally as ongoing
  sample data (user's decision, 2026-08-06) rather than cleaned up.
- **Checkout simplifications for v1**: billing address always equals shipping address (no separate
  billing UI); there's no way to edit or delete a saved address from the frontend yet (the backend
  supports both — see `AddressesService`); "confirm payment" is a client-side stand-in for a real
  gateway webhook, not an actual payment integration.
- **Seller module simplifications for v1**: product creation supports one image URL and no
  gemstones (the backend's `CreateProductCommand` supports full gemstone/multi-image lists);
  inventory adjustment is a single quantity-delta input with no adjustment history view; My
  Products has no pagination (fine at today's per-seller product counts, would need one before a
  seller has more than ~100 listings); there's no seller-facing analytics/reporting beyond the
  three Dashboard stat cards.
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
| Customer UI — Order history (list), reviews | ❌ Missing |
| Seller UI — Dashboard, KYC, Products, Order Fulfillment | ✅ Complete, verified live |
| Admin UI | ❌ Missing |
| Union UI | ❌ Missing |

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
