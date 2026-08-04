# Backend Progress

Per-module completion status across all four layers (Domain → Persistence → Application → API).
Derived directly from source inspection, last updated 2026-08-04 (Jewelry Union module added) —
see [API_PROGRESS.md](API_PROGRESS.md) for the endpoint-level detail behind the Application/API
columns here.

| Module | Domain | Persistence | Application | API | Overall |
|---|---|---|---|---|---|
| Auth / Identity | ✅ | ✅ | ✅ (5 use cases) | ✅ (5 endpoints) | **Complete** |
| Customers | ✅ | ✅ | — (touched via other features only) | — | **Complete for current needs** (no standalone CRUD, none required yet) |
| Sellers (incl. KYC) | ✅ | ✅ | ✅ (6 use cases) | ✅ (7 endpoints) | **Complete** |
| Catalog — Categories | ✅ | ✅ | ✅ (4 use cases) | ✅ (4 endpoints) | **Complete** |
| Catalog — Products | ✅ | ✅ | ✅ (6 use cases) | ✅ (6 endpoints) | **Complete** |
| Cart | ✅ | ✅ | ✅ (5 use cases) | ✅ (5 endpoints) | **Complete** |
| Wishlist | ✅ | ✅ | ✅ (3 use cases) | ✅ (3 endpoints) | **Complete** |
| Orders | ✅ | ✅ | ✅ (7 use cases) | ✅ (7 endpoints) | **Partially complete** — shipping cost hardcoded, no auto-complete rollup |
| Payments | ✅ | ✅ | ✅ (folded into Orders) | ✅ (folded into Orders) | **Partially complete** — confirmation logic is real, no real gateway wired in |
| Tax | ✅ | ✅ | ⚠️ applied at checkout only, no CRUD | ❌ no controller | **Partially complete** |
| Reviews | ✅ | ✅ | ✅ (5 use cases) | ✅ (5 endpoints) | **Complete** |
| Notifications | ✅ | ✅ | ✅ (3 use cases + real-time push) | ✅ (3 endpoints + SignalR hub) | **Complete** |
| **Jewelry Union** | ✅ (13 tables, richly modeled) | ✅ | ✅ (19 commands, 14 queries) | ✅ (33 endpoints, 3 controllers) | **Complete** |
| Admin (as a distinct module) | n/a | n/a | ⚠️ folded into other modules | ⚠️ folded into other modules | **No dedicated module — role-gated actions only** |

## Detail by Status

### ✅ Complete

**Auth**: registration (customer + seller), login with lockout after 5 failed attempts (15 min),
JWT access + refresh token issuance, refresh-token rotation with reuse detection (a replayed
revoked token revokes every session for that user), logout. Missing: password reset, email
verification — not blocking for v1, worth flagging before public launch.

**Sellers**: profile retrieval, KYC document submission and per-document admin review, seller
approval (gated on all documents being Verified) and rejection, pending-sellers admin queue. This
is the most fully-built module in the codebase.

**Catalog (Categories + Products)**: full CRUD, category tree via self-referencing
`ParentCategoryId`, inventory adjustment, product status transitions (admin-gated).

**Cart / Wishlist**: standard add/update/remove/clear/get — nothing missing for v1.

**Reviews**: product and seller review listing, review creation (tied to a specific
`OrderItemId`, so only actual purchasers can review), seller responses, admin moderation.

**Notifications**: list/mark-read/mark-all-read, plus a genuinely complete real-time layer
(SignalR hub with JWT-over-querystring auth for the WebSocket handshake, persist-then-push so
offline users still see notifications later).

**Jewelry Union** (added 2026-08-04): union creation (KYC-approved sellers only, founder becomes
President) with admin approval before the union is publicly visible; membership lifecycle
(request → officer/admin review → active, plus role changes and removal, with a safeguard
against removing a union's only President); officer-gated announcements, a members-only document
library, and public trade events; meetings that auto-invite every active member as an attendee,
support RSVP, and record minutes with follow-up action items tied to a due date and owner;
governance polls (Draft → Active → Closed) with single- or multi-select voting, one vote per
member enforced at the DB level via a unique index. Authorization for governance actions that
need a "which member did this" attribution (creating a meeting/poll/announcement, recording
minutes) requires the caller to actually hold an officer membership — there's no bare-Admin
bypass for these, since Admin accounts have no `UnionMember` row to attribute the record to.
Actions that don't need that attribution (approving a union, reviewing a membership, closing a
poll) do allow Admin as an alternate to the officer check. No Domain or Persistence changes were
needed — the schema already had every table this module uses.

### ⚠️ Partially Complete

**Orders**: checkout, retrieval (single/mine/seller queue), cancellation, per-item status
updates — all real, EF-backed logic. Two known gaps, both marked with `TODO` comments in source:
- `Features/Orders/Commands/CreateOrder/CreateOrderCommand.cs:137` — shipping charge hardcoded to
  `0`; no rate calculation engine.
- `Features/Orders/Commands/UpdateOrderItemStatus/UpdateOrderItemStatusCommand.cs:104` — no
  auto-transition of the parent `Order` to a completed state once every `OrderItem` is delivered.

**Payments**: `ConfirmPaymentCommand` has real, complete logic (idempotency check, order/payment
state transition, inventory deduction, notification dispatch) but its own doc comment describes
it as a stand-in for a real payment gateway webhook (Razorpay/Stripe) — nothing in the codebase
calls it from an actual payment provider yet. This is a placeholder for an integration point, not
a stub with fake logic.

**Tax**: `TaxRate` entities exist and are applied during checkout by
`Application/Common/Services/GstTaxCalculator`, but there's no way to manage tax rates through
the API — they can currently only be seeded or edited directly against the database.

### ❌ Not Started

**Admin as its own module**: there is no `AdminController` or `Features/Admin` folder. Every
admin capability today is a role-gated action tucked into an otherwise-customer/seller-facing
controller (approve sellers, moderate reviews, manage categories/products). There's no
platform-wide dashboard, user/role management, or reporting endpoint set.

## What's NOT Missing (verified, not assumed)

A repository-wide search for `NotImplementedException`, "not implemented", and stub/fake/dummy
markers found **zero** hits anywhere in Domain, Application, Persistence, or Infrastructure.
Every one of the 44 original MediatR handlers, plus the 33 added for the Jewelry Union module
(77 total), has real, EF-Core-backed logic — there is no "looks done but is actually a stub"
module hiding in what's listed as ✅ above.

## Cross-Cutting Backend Status

| Concern | Status |
|---|---|
| DI composition | ✅ complete, one `Add{Layer}()` extension per layer |
| Repository + Unit of Work | ✅ complete, lazy per-aggregate repositories |
| Validation pipeline | ✅ complete (FluentValidation via MediatR behavior) |
| Global error handling | ✅ complete (`ProblemDetails` middleware + typed exceptions) |
| Logging | ✅ complete (Serilog + `LoggingBehavior`) |
| Auth/authorization | ✅ complete for existing modules; no user/role management API |
| EF Core migrations | ✅ `InitialCreate` generated 2026-08-04 (none existed before) |
| Testing (unit/integration/architecture) | ❌ not started — explicitly deferred, see [PROJECT_STATUS.md](PROJECT_STATUS.md) |
| CI/CD | ❌ not started — `.github/workflows/` is empty, explicitly deferred |
