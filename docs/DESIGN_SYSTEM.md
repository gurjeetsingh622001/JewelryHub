# Design System

The permanent visual/UX reference for the Angular frontend (`client/`). Every screen — current
and future, Customer/Seller/Admin/Union alike — follows this unless the user explicitly changes
it. If a new page needs something not covered here, extend this file first, then build the page.

## Brand Direction

JewelryHub reads as a **premium, editorial jewelry boutique** — closer to Cartier, Tiffany & Co.,
Swarovski, Pandora, and Blue Nile than to a generic e-commerce template. Inspiration only, never
copied assets/marks. In practice that means:

- **Restraint over density.** Generous white space, few elements per screen, one clear focal
  point. If a screen feels crowded, remove something before adding a fix.
- **Serif display type + clean sans body** — the single biggest lever for a "luxury" feel.
- **Warm neutrals, not stark white/black.** A soft ivory background and warm near-black ink, with
  gold as the one accent that carries meaning (price, CTA, active state) — not decoration.
- **Quiet motion.** Fades and gentle elevation on hover, never bounce/spring easing or anything
  that calls attention to itself.
- **Real photography, never placeholders.** Curated royalty-free jewelry/lifestyle imagery
  (Unsplash/Pexels/Pixabay) everywhere a product photo, hero image, or editorial shot is needed —
  see [Imagery](#imagery).

## Color

Defined as CSS custom properties in `client/src/styles/_tokens.scss`, loaded once via
`styles.scss`. Never hardcode a hex value in a component's SCSS — reference the token.

| Token | Value | Use |
|---|---|---|
| `--color-bg` | `#FAF7F2` | Page background (warm ivory, not `#fff`) |
| `--color-bg-alt` | `#F3ECE0` | Alternate section background (warm sand, for zebra-striping sections) |
| `--color-surface` | `#FFFFFF` | Cards, elevated panels |
| `--color-ink` | `#221F1B` | Primary text (warm near-black, never pure `#000`) |
| `--color-ink-muted` | `#6B6258` | Secondary text, captions |
| `--color-ink-faint` | `#A79E90` | Disabled/placeholder text |
| `--color-border` | `#E6DCC8` | Hairline borders, dividers |
| `--color-gold` | `#B8934A` | **The** brand accent — CTAs, prices, active nav state, links |
| `--color-gold-deep` | `#8C6D33` | Gold hover/pressed state |
| `--color-gold-tint` | `#F3E9D8` | Gold-tinted backgrounds (badges, highlighted rows) |
| `--color-emerald` | `#2F4A3C` | Secondary accent — used sparingly for "New"/success/gemstone motifs |
| `--color-emerald-tint` | `#E4ECE7` | Emerald-tinted backgrounds |
| `--color-error` | `#B3413E` | Form errors, destructive actions |
| `--color-charcoal` | `#1A1816` | Dark surfaces (footer, mobile nav overlay) |
| `--color-charcoal-ink` | `#EDE7DC` | Text on `--color-charcoal` |

Gold is a **signal**, not wallpaper — one gold element per view (a CTA button, a price, an active
tab) reads as intentional; five gold elements reads cheap. When in doubt, use ink or muted ink and
save gold for the one thing that should draw the eye.

## Typography

Two Google Fonts, loaded in `index.html` (`Playfair Display` weights 400/500/600/700,
`Inter` weights 400/500/600).

- **Display** (`--font-display: 'Playfair Display', Georgia, serif`) — every heading (`h1`–`h3`),
  hero copy, section titles, price display on product detail. Never used for body copy or UI
  chrome (buttons, form labels, nav) — a whole screen set in serif feels like a wedding invite,
  not a store.
- **Body** (`--font-body: 'Inter', -apple-system, sans-serif`) — everything else: paragraphs,
  buttons, form fields, nav links, captions.
- **Eyebrow labels** (category tags, section kickers like "NEW ARRIVALS") — `--font-body`,
  `--text-xs`, `600` weight, `0.12em` letter-spacing, uppercase. Used to introduce a section
  before its serif headline, not as a replacement for one.

| Token | Size | Typical use |
|---|---|---|
| `--text-xs` | 0.75rem | Eyebrow labels, fine print |
| `--text-sm` | 0.875rem | Captions, secondary metadata |
| `--text-base` | 1rem | Body copy |
| `--text-lg` | 1.125rem | Lead paragraphs |
| `--text-xl` | 1.375rem | `h3` |
| `--text-2xl` | 1.75rem | `h2` |
| `--text-3xl` | 2.25rem | `h1` (section titles) |
| `--text-4xl` | 3rem | Hero headline (mobile) |
| `--text-5xl` | 3.75rem | Hero headline (desktop) |

Line-height: `1.15` for display/headings, `1.6` for body copy. Never justify text.

## Spacing, Radius, Elevation, Motion

- **Spacing scale** (rem, 4px base): `0.25 · 0.5 · 0.75 · 1 · 1.5 · 2 · 3 · 4 · 6 · 8 · 12`.
  Section vertical padding is generous — minimum `6rem` (`py-24`) between major homepage
  sections on desktop, never less than `3rem` on mobile.
- **Radius**: `--radius-sm: 2px` (inputs, small chips), `--radius-md: 4px` (buttons),
  `--radius-lg: 8px` (cards, images), `--radius-full: 999px` (pills, avatar). Kept small on
  purpose — heavy rounding (the `rounded-2xl` trend) reads as consumer-app, not boutique.
- **Shadow**: `--shadow-sm`, `--shadow-md`, `--shadow-lg` — all soft, low-opacity
  (`rgba(34,31,27,…)`, never pure black), large blur radius, minimal spread. A resting card uses
  `--shadow-sm` or nothing; hover lifts to `--shadow-md`.
- **Motion**: `--duration-fast: 150ms`, `--duration-base: 250ms`, `--duration-slow: 400ms`, all
  with `--ease-standard: cubic-bezier(.4,0,.2,1)`. Hover states transition color/shadow/transform
  over `--duration-fast`–`--duration-base`. Entrance animations (e.g. a section fading up into
  view) use `--duration-slow`. Never use spring/bounce easing — it reads playful, not premium.

## Layout & Responsiveness

- Breakpoints follow Tailwind's defaults (`sm 640 / md 768 / lg 1024 / xl 1280 / 2xl 1536`) since
  Tailwind utilities are used for layout/spacing throughout.
- Content max-width `1280px` (`max-w-7xl`), centered, with responsive horizontal padding
  (`px-4` mobile → `px-8` desktop).
- Mobile-first: build the narrow layout first, then add `md:`/`lg:` overrides. Every page must be
  fully usable at 375px width — no fixed-width elements that force horizontal scroll.
- Grids: product/category grids are `grid-cols-2` on mobile, `md:grid-cols-3`,
  `lg:grid-cols-4` — never more than 4 columns even on very wide screens; density is not the goal.

## Components

- **Standalone Angular components only** — no NgModules, matching the existing codebase
  convention. One component per file, colocated `.ts` / `.html` / `.scss`.
- **Library boundaries** (deliberate split, so the app doesn't look like a Material demo):
  - **Custom HTML + SCSS + Tailwind** for anything visual/marketing-facing — navbar, footer,
    hero sections, product cards, category tiles, badges. This is where the brand actually lives,
    so it gets full bespoke control rather than a component kit's default look.
  - **Angular Material** for complex interactive form/overlay primitives where correct
    accessibility behavior (focus trap, keyboard nav, ARIA) matters more than bespoke visuals:
    text inputs, selects, button-toggles, dialogs, menus, snack bars. Styled to match brand tokens
    via `--mat-sys-*` overrides, not left at Material defaults.
  - **PrimeNG is not used** — it was part of the original stack choice but the installed version
    (22.x) turned out to require a paid PrimeUI license; without one it injects a shadow-DOM
    "Invalid PrimeUI License" banner into every page (`node_modules/primeng/fesm2022/primeng-license.mjs`
    — a real, cryptographically-verified gate, not a bug). Rather than pay for a license or ship a
    broken-looking banner, PrimeNG/`@primeuix/themes`/`primeicons` were removed entirely
    (2026-08-05) and every prior PrimeNG usage was replaced with a Material equivalent: `Toast`/
    `MessageService` → `MatSnackBar`, `SelectButton` → `MatButtonToggleGroup`. If PrimeNG is
    wanted later, it needs a purchased license key passed to `providePrimeNG` first.
  - **Tailwind** utility classes for layout/spacing/flex/grid glue everywhere; Preflight stays
    disabled (see [FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md)) so it never fights Material's base
    styles.
- **Buttons**: two visual styles — *primary* (solid `--color-gold` background, `--color-bg` text,
  used once per view for the one action that matters) and *secondary* (transparent/outlined,
  `--color-ink` text, `--color-border` or `--color-ink` border). A `--secondary-on-dark` variant
  (`--color-charcoal-ink` text/border) exists for a secondary button placed over a dark/photo
  background — e.g. the hero — where the default secondary's ink-on-transparent styling would be
  nearly invisible. No more than one primary button visible at a time in a given section.
- **Cards** (product, category, article): `--color-surface` background, `--radius-lg`, `1px solid
  --color-border` or `--shadow-sm` (not both — pick one depending on background), image on top
  with fixed aspect-ratio (`aspect-[3/4]` for product photography, jewelry reads best in a tall
  frame), content padded `1.5rem`, hover lifts to `--shadow-md` and scales the image slightly
  (`scale-105`, clipped by `overflow-hidden`) over `--duration-base`.

## Icons

**Lucide** (`lucide-angular` package) exclusively — clean thin-line icons that match the minimal
luxury aesthetic. Do not mix in Heroicons/Font Awesome/Material Symbols glyphs; pick one library
and stay consistent. (Angular Material's own component-internal icons — e.g. the select dropdown
arrow, checkbox check — are unaffected; this rule is about icons *we* choose to place in the UI.)
Default stroke width `1.5`, default size `20`–`24px` depending on context (nav/action icons `20`,
feature/empty-state icons `32`–`48`).

## Auth Pages (Login / Register)

Login and Register are deliberately **outside the storefront Shell** — no Navbar/Footer, so
nothing invites someone to wander off mid-signin/signup (see `app.routes.ts`: they're top-level
routes, while everything else nests under a `ShellComponent` parent route). Both use the shared
`core/layout/auth-layout/AuthLayoutComponent`:

- **Desktop (`lg` and up)**: split-screen. Left panel is a full-bleed photo (`position: sticky`,
  so a long form scrolls past it rather than stretching it) with a dark scrim, the wordmark
  linking home in the top-left corner, and an italic serif editorial quote pinned near the
  bottom. Right panel is the projected page content (`<ng-content>`), centered in a column whose
  width is configurable via `[maxWidth]` (Login: default `26rem`; Register: `34rem`, to fit the
  Seller form's 2–3 column field rows).
- **Mobile**: the photo panel is hidden entirely (`display: none` below `lg`) — a centered
  wordmark sits above the form instead. Prioritize the form over the atmosphere on small screens.
- Shared chrome classes (`.auth-lead`, `.auth-form`, `.auth-form__row[--three]`, `.auth-submit`,
  `.auth-switch`, `.auth-note`, `.account-type-toggle`) live in the global `styles.scss`, not
  per-component — both pages need identical spacing/typography for these, so duplicating them
  per component would just be two copies to keep in sync.
- Material's button-toggle (the Customer/Seller switch) needed its own token overrides —
  `--mat-sys-secondary-container`/`--mat-sys-tertiary-container` control its selected-state fill,
  separately from `--mat-sys-primary-container` which controls buttons/inputs. Both are mapped to
  the gold tint so the toggle doesn't default to Material's stock salmon/orange.

## Imagery

Real, curated, royalty-free photography only — **never** a gray placeholder box or lorem-picsum
filler, even during scaffolding. Source from Unsplash, Pexels, or Pixabay (all license-compatible
with commercial use, no attribution legally required though Unsplash's own etiquette appreciates
it). Pick images that are actually on-theme (fine jewelry, rings/necklaces/earrings close-ups,
elegant neutral-toned lifestyle shots) rather than generic stock. Always specify an explicit
`width`/`height` or `aspect-ratio` to prevent layout shift, and use a responsive/appropriately
sized source URL rather than a full-resolution original.

## Working Pattern For New Pages

For every new page/component going forward: briefly state the UI approach (layout, key
components, imagery) before writing code, then generate `.ts` / `.html` / `.scss` together. This
file is the shared reference so that explanation can stay short — "hero + 3-up category grid,
per the design system" is enough context once this doc exists.

## Maintenance

Update this file whenever a genuinely new pattern is introduced (a new component archetype, a
color/type addition) — not for one-off page content. If a future page needs to deviate from
something here, note the deviation and why, rather than silently drifting.
