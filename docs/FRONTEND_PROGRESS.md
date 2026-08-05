# Frontend Progress

## Status: Foundation + Design System + Home + Auth Pages + Product Browsing Complete

Last updated 2026-08-05. An Angular workspace exists at `client/` (sibling to `src/`, not part of
`JewelryHub.sln` since it isn't a .NET project). The auth flow, HTTP layer, routing, a full
premium design system, the public Home page, redesigned Login/Register pages, and real product
browsing (list + detail, wired to the live Products/Categories APIs) are built. **This is the
first slice verified against an actual running backend + database**, not just a build — see
below. Cart, checkout, order history, reviews, and the Seller/Admin/Union dashboards don't exist
yet — see [ROADMAP.md](ROADMAP.md) Phase 13b onward.

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
  scroll, plus a charcoal off-canvas mobile panel. Search/wishlist/cart aren't built yet, so they
  surface an honest "coming soon" `MatSnackBar` instead of a dead link or a route to a page that
  doesn't exist.
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
  strip, spec table, gemstone list, Add to Cart/Wishlist as honest "coming soon" snack bars since
  Cart isn't built yet).
- `shared/product-card/` — the reusable listing card (discount badge, out-of-stock badge,
  wishlist button, rating, price with strikethrough original), used by the Product List grid and
  intended for any future "you may also like"/search-result surface.
- `core/models/paged-result.ts` — mirrors the backend's `PagedResult<T>`, generic across every
  paged endpoint.
- `environments/` — `apiUrl` pointing at `https://localhost:65334/api/v1` in development, a
  relative `/api/v1` default for production.

## Imagery

7 curated Unsplash photos (free tier, Unsplash License, verified individually — several
candidates were rejected either for being Unsplash+ paid photos or, in one case, for visibly
showing a real jewelry brand's name ("dinh van") printed on the display risers in the source
photo, which was replaced before shipping). See `HomeComponent` for the exact CDN URLs in use.

## Not Yet Wired Up / Known Gaps

- **Verified against a live backend for the first time (2026-08-05).** The API and a real SQL
  Server database were both running; verification included registering a seller, completing the
  KYC approve flow as the dev admin, creating a category, and creating three real products via
  the live API, then confirming the Product List/Detail pages render them correctly end-to-end
  (filters, category dropdown, discount badge, gallery, spec table, "coming soon" snack bars) with
  zero console errors. Login/Register still haven't been round-tripped this way (only Products/
  Categories were exercised) — worth doing next.
- **Demo data was created directly against the live database** as part of this verification: a
  seller account (`seller.demo@jewelryhub.local`), a "Rings" category, and three products
  ("Aurelia Solitaire Ring", "Meera Diamond Necklace", "Zara Gold Hoop Earrings"). Decide whether
  to keep this as sample data or remove it before real use.
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
| Customer UI — Cart, checkout, orders, reviews | ❌ Missing |
| Seller UI | ❌ Missing |
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
