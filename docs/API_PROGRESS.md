# API Progress

Ground truth as of 2026-08-11 (`AdminController` added): 14 controllers, 85 endpoints, all backed
by a real MediatR handler with EF-Core-backed logic. Zero orphaned commands (every Application
handler has exactly one controller action calling it) and zero dangling references (every
controller action points at a command/query that exists). Route prefix for all controllers:
`api/v1/`.

## AuthController — `api/v1/auth` (class-level `[AllowAnonymous]`)

| Verb | Route | Handler | Status |
|---|---|---|---|
| POST | `register/customer` | `RegisterCustomerCommand` | ✅ Complete |
| POST | `register/seller` | `RegisterSellerCommand` | ✅ Complete |
| POST | `login` | `LoginCommand` | ✅ Complete (lockout after 5 failed attempts, 15 min) |
| POST | `refresh` | `RefreshTokenCommand` | ✅ Complete (rotation + reuse detection) |
| POST | `logout` | `LogoutCommand` | ✅ Complete |

**Missing**: password reset / forgot-password flow, email verification, change-password endpoint.

## AdminController — `api/v1/admin` (class-level `[Authorize(Roles = "Admin")]`) — added 2026-08-11

| Verb | Route | Handler | Status |
|---|---|---|---|
| GET | `dashboard` | `GetDashboardOverviewQuery` | ✅ Complete |
| GET | `users` | `GetUsersQuery` | ✅ Complete (search, role filter, isActive filter, paginated) |
| POST | `users/{id}/status` | `SetUserActiveStatusCommand` | ✅ Complete (self-deactivation blocked) |
| GET | `reviews` | `GetAllReviewsQuery` | ✅ Complete (isApproved/isFlagged filters, for moderation browsing) |

**Deliberately holds only what has no other home.** Approving a seller/union or moderating a
specific review stay on `SellersController`/`UnionsController`/`ReviewsController` — this
controller exists for genuinely new cross-cutting concerns (a platform dashboard, user account
management, and a way to *find* a review to moderate, since none of the existing per-product/
per-seller review queries expose one). `GetUsersQuery` is the first Application-layer code to ever
list/paginate `User` rows — before this, `IUnitOfWork.Users` was only consumed inside `Auth`.
`SetUserActiveStatusCommand` blocks an admin from deactivating their own account but does not
revoke already-issued refresh tokens for a deactivated user (only blocks *future* logins, which
`LoginCommand` already checks via `User.IsActive`) — good enough for "stop a problem account from
logging in again," not full session termination.

## CartController — `api/v1/cart` (Customer)

| Verb | Route | Handler | Status |
|---|---|---|---|
| GET | `` | `GetCartQuery` | ✅ Complete |
| POST | `items` | `AddToCartCommand` | ✅ Complete (fixed 2026-08-06 — see note below) |
| PUT | `items/{productId}` | `UpdateCartItemQuantityCommand` | ✅ Complete |
| DELETE | `items/{productId}` | `RemoveFromCartCommand` | ✅ Complete |
| DELETE | `` | `ClearCartCommand` | ✅ Complete |

**Bug found and fixed (2026-08-06)**: `AddToCartCommand` loaded `Product` via `Query()`
(`AsNoTracking()`), then attached it to a brand-new `CartItem` saved through the same, separately
tracked `DbContext`. EF Core didn't recognize the no-tracking `Product` instance as already
existing and tried to `INSERT` it again on `SaveChangesAsync`, throwing a primary-key-violation
`DbUpdateException` (500) on **every** add-to-cart call. This had been marked "✅ Complete" for
weeks without ever actually being exercised against a real database — found the moment the
Angular Cart feature was tested live. Fixed by loading `Product` via `QueryTracking()` instead, so
it participates in the same change-tracking graph as the `Cart` it's being attached to. The other
three Cart handlers (`UpdateCartItemQuantity`, `RemoveFromCart`, `ClearCart`) don't have this
issue — they only mutate entities already loaded through the same tracked query, never attach a
separately-queried entity to a new one.

**Missing**: nothing obvious for a v1 cart — this module is complete.

## CustomerAddressesController — `api/v1/customer-addresses` (Customer) — added 2026-08-06

| Verb | Route | Handler | Status |
|---|---|---|---|
| GET | `` | `GetMyAddressesQuery` | ✅ Complete |
| POST | `` | `CreateAddressCommand` | ✅ Complete (first address is always forced default) |
| PUT | `{id}` | `UpdateAddressCommand` | ✅ Complete |
| DELETE | `{id}` | `DeleteAddressCommand` | ✅ Complete (soft-delete — see [DATABASE.md](DATABASE.md)) |

**Gap found and closed (2026-08-06)**: this controller didn't exist at all until Checkout was
built. `CreateOrderCommand` requires a `ShippingAddressId`/`BillingAddressId` that must already
exist, but there was **no endpoint anywhere** to create a `CustomerAddress` — Checkout was
literally impossible to complete before this. Completing it also required making
`CustomerAddress` implement `ISoftDelete` (a comment on `OrderConfigurations` had already assumed
it did, but the entity never actually declared the interface) — see the migration note in
[DATABASE.md](DATABASE.md).

**Missing**: no "set as default" endpoint separate from `Update` (toggle `IsDefault` via a full
update instead); no address-book management UI beyond what Checkout itself offers.

## CategoriesController — `api/v1/categories`

| Verb | Route | Auth | Handler | Status |
|---|---|---|---|---|
| GET | `` | Anonymous (Admin sees inactive) | `GetCategoriesQuery` | ✅ Complete |
| POST | `` | Admin | `CreateCategoryCommand` | ✅ Complete |
| PUT | `{id}` | Admin | `UpdateCategoryCommand` | ✅ Complete |
| DELETE | `{id}` | Admin | `DeleteCategoryCommand` | ✅ Complete |

## NotificationsController — `api/v1/notifications` (any authenticated)

| Verb | Route | Handler | Status |
|---|---|---|---|
| GET | `` | `GetMyNotificationsQuery` | ✅ Complete |
| POST | `{id}/read` | `MarkNotificationAsReadCommand` | ✅ Complete |
| POST | `read-all` | `MarkAllNotificationsAsReadCommand` | ✅ Complete |

Plus real-time push: `Hubs/NotificationsHub` (SignalR, `/hubs/notifications`) and
`Services/SignalRNotifier`. Persist-then-push pattern — a DB row is always written first, so the
polling API and the live push never disagree.

## OrdersController — `api/v1/orders`

| Verb | Route | Auth | Handler | Status |
|---|---|---|---|---|
| POST | `` | Customer | `CreateOrderCommand` | ✅ Complete (shipping computed per seller via `IShippingCalculator`, see below) |
| GET | `{id}` | Any authenticated | `GetOrderByIdQuery` | ✅ Complete |
| GET | `me` | Customer | `GetMyOrdersQuery` | ✅ Complete |
| POST | `{id}/cancel` | Any authenticated | `CancelOrderCommand` | ✅ Complete |
| POST | `{id}/payments/{paymentId}/confirm` | Customer | `ConfirmPaymentCommand` | ⚠️ Complete logic, but stands in for a real gateway webhook — nothing calls it from an actual payment provider |
| GET | `seller/queue` | Seller | `GetSellerOrderItemsQuery` | ✅ Complete |
| PATCH | `items/{orderItemId}/status` | Seller, Admin | `UpdateOrderItemStatusCommand` | ✅ Complete (rolls the parent Order to Delivered once every item is; updates the fulfilling seller's revenue/count rollups) |

**Bug found and fixed (2026-08-06)**: marking an item `Shipped` threw a `DbUpdateConcurrencyException`
(500, "expected to affect 1 row(s), but actually affected 0") on every call. The handler created the
new `Shipment` via `item.Shipment ??= new Shipment { OrderItemId = item.Id }` and relied on EF Core's
navigation-fixup to detect it as `Added` — but `Shipment.Id` (like every `BaseEntity`) is a
client-generated `Guid` set in a property initializer *before* EF ever sees the instance, so the
change tracker had no default-value signal to tell "brand new, client-set key" apart from "existing,
unchanged-key entity," and issued an `UPDATE ... WHERE Id = @p` instead of an `INSERT` — which
naturally matched zero rows. Confirmed via the actual generated SQL (`sys.dm_exec_query_stats`), not
guessed. Fixed by adding an `IRepository<Shipment> Shipments` to `IUnitOfWork`/`UnitOfWork` (same
pattern as every other aggregate) and routing the new `Shipment` through
`_unitOfWork.Shipments.AddAsync(...)` explicitly instead of relying on graph fixup alone. Found while
building and live-testing the Angular Seller Order Fulfillment queue — see
[FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md).

**Shipping** (added 2026-08-04): `IShippingCalculator` (`Application/Common/Interfaces/ITaxCalculator.cs`)
+ `FlatRateShippingCalculator` — flat base rate, an inter-state surcharge, and a per-gram
surcharge past a small weight allowance, waived above a free-shipping subtotal threshold.
Computed per seller group (each seller ships independently) and summed into
`Order.ShippingCharges`. Not yet admin-configurable — see **Missing** below.

**Missing**: real payment gateway integration (Razorpay/Stripe), refunds/returns, an
admin-configurable shipping-rate table (today's rates are constants in
`FlatRateShippingCalculator`, same shape as `GstTaxCalculator` before tax rates were made
DB-driven).

## ProductsController — `api/v1/products`

| Verb | Route | Auth | Handler | Status |
|---|---|---|---|---|
| GET | `` | Anonymous | `GetProductsQuery` | ✅ Complete |
| GET | `{id}` | Anonymous | `GetProductByIdQuery` | ✅ Complete |
| POST | `` | Seller | `CreateProductCommand` | ✅ Complete |
| PUT | `{id}` | Seller, Admin | `UpdateProductCommand` | ✅ Complete |
| PATCH | `{id}/status` | Admin | `UpdateProductStatusCommand` | ✅ Complete |
| POST | `{id}/inventory/adjust` | Seller, Admin | `AdjustInventoryCommand` | ✅ Complete |

**Missing**: bulk import, image upload endpoint (schema has `ProductImages` but no visible
upload flow was found beyond whatever URL fields the create/update commands accept), search
facets beyond whatever `GetProductsQuery` filters on.

## ReviewsController — `api/v1/reviews`

| Verb | Route | Auth | Handler | Status |
|---|---|---|---|---|
| GET | `product/{productId}` | Anonymous | `GetProductReviewsQuery` | ✅ Complete |
| GET | `seller/{sellerId}` | Anonymous | `GetSellerReviewsQuery` | ✅ Complete |
| POST | `` | Customer | `CreateReviewCommand` | ✅ Complete |
| POST | `{id}/response` | Seller | `RespondToReviewCommand` | ✅ Complete |
| POST | `{id}/moderate` | Admin | `ModerateReviewCommand` | ✅ Complete |

## SellersController — `api/v1/sellers`

| Verb | Route | Auth | Handler | Status |
|---|---|---|---|---|
| GET | `me` | Seller | `GetSellerProfileQuery` | ✅ Complete |
| POST | `me/documents` | Seller | `SubmitSellerDocumentCommand` | ✅ Complete |
| GET | `pending` | Admin | `GetPendingSellersQuery` | ✅ Complete |
| GET | `{id}` | Admin | `GetSellerProfileQuery` | ✅ Complete |
| POST | `documents/{documentId}/review` | Admin | `ReviewSellerDocumentCommand` | ✅ Complete |
| POST | `{id}/approve` | Admin | `ApproveSellerCommand` | ✅ Complete (checks all documents Verified first) |
| POST | `{id}/reject` | Admin | `RejectSellerCommand` | ✅ Complete |

This is the most complete module in the API, including the full KYC review lifecycle.

## WishlistController — `api/v1/wishlist` (Customer)

| Verb | Route | Handler | Status |
|---|---|---|---|
| GET | `` | `GetWishlistQuery` | ✅ Complete |
| POST | `items/{productId}` | `AddToWishlistCommand` | ✅ Complete |
| DELETE | `items/{productId}` | `RemoveFromWishlistCommand` | ✅ Complete |

## UnionsController — `api/v1/unions`

| Verb | Route | Auth | Handler | Status |
|---|---|---|---|---|
| GET | `` | Anonymous | `GetUnionsQuery` | ✅ Complete (public directory, approved unions only) |
| POST | `` | Seller | `CreateUnionCommand` | ✅ Complete (KYC-approved sellers only; founder becomes President) |
| GET | `pending` | Admin | `GetPendingUnionsQuery` | ✅ Complete |
| GET | `me/memberships` | Seller | `GetMyMembershipsQuery` | ✅ Complete |
| GET | `{id}` | Anonymous | `GetUnionByIdQuery` | ✅ Complete (404s an unapproved union to non-owners/non-Admin) |
| POST | `{id}/approve` | Admin | `ApproveUnionCommand` | ✅ Complete |
| POST | `{id}/join` | Seller | `RequestMembershipCommand` | ✅ Complete |
| GET | `{id}/members` | Any authenticated | `GetUnionMembersQuery` | ✅ Complete |
| GET | `{id}/members/pending` | Officer/Admin (enforced in handler) | `GetPendingMembershipsQuery` | ✅ Complete |
| POST | `memberships/{membershipId}/review` | Officer/Admin | `ReviewMembershipCommand` | ✅ Complete |
| PATCH | `memberships/{membershipId}/role` | Officer/Admin | `UpdateMemberRoleCommand` | ✅ Complete |
| DELETE | `memberships/{membershipId}` | Officer/Admin | `RemoveMemberCommand` | ✅ Complete (blocks removing a union's only President) |
| GET | `{id}/announcements` | Any authenticated | `GetAnnouncementsQuery` | ✅ Complete |
| POST | `{id}/announcements` | Officer only (no Admin bypass — needs a member author) | `CreateAnnouncementCommand` | ✅ Complete |
| DELETE | `announcements/{announcementId}` | Officer/Admin | `DeleteAnnouncementCommand` | ✅ Complete |
| GET | `{id}/documents` | Any active member/Admin | `GetDocumentsQuery` | ✅ Complete |
| POST | `{id}/documents` | Any active member | `UploadDocumentCommand` | ✅ Complete |
| GET | `{id}/events` | Anonymous | `GetEventsQuery` | ✅ Complete |
| POST | `{id}/events` | Officer/Admin | `CreateEventCommand` | ✅ Complete |
| PATCH | `events/{eventId}/status` | Officer/Admin | `UpdateEventStatusCommand` | ✅ Complete |

## UnionMeetingsController — `api/v1/union-meetings`

| Verb | Route | Auth | Handler | Status |
|---|---|---|---|---|
| GET | `?unionId=` | Any active member/Admin | `GetMeetingsQuery` | ✅ Complete |
| POST | `` | Officer only | `CreateMeetingCommand` | ✅ Complete (auto-invites every active member as an attendee) |
| GET | `{id}` | Any active member/Admin | `GetMeetingByIdQuery` | ✅ Complete (full detail: agenda, attendees, minutes, action items) |
| PATCH | `{id}/status` | Officer/Admin | `UpdateMeetingStatusCommand` | ✅ Complete |
| POST | `{id}/rsvp` | Any active member | `RsvpToMeetingCommand` | ✅ Complete |
| POST | `{id}/minutes` | Officer only | `RecordMinuteCommand` | ✅ Complete (validates agenda item and action-item assignees belong to the meeting/union) |
| GET | `my-action-items` | Seller | `GetMyActionItemsQuery` | ✅ Complete (open items across every union the caller belongs to) |

## UnionPollsController — `api/v1/union-polls`

| Verb | Route | Auth | Handler | Status |
|---|---|---|---|---|
| GET | `?unionId=` | Any active member/Admin | `GetPollsQuery` | ✅ Complete |
| POST | `` | Officer only | `CreatePollCommand` | ✅ Complete (created Draft; needs an explicit Open) |
| GET | `{id}` | Any active member/Admin | `GetPollByIdQuery` | ✅ Complete (live vote counts per option) |
| POST | `{id}/open` | Officer/Admin | `OpenPollCommand` | ✅ Complete |
| POST | `{id}/close` | Officer/Admin | `ClosePollCommand` | ✅ Complete |
| POST | `{id}/vote` | Seller (active member) | `VoteCommand` | ✅ Complete (one vote per member, enforced at the DB level) |

**Missing for Union**: no way to edit a union's profile after creation (only create + approve
exist), no way to leave a union voluntarily (only an officer/Admin can remove a member), no file
upload endpoint for documents (same convention as `SubmitSellerDocumentCommand` — `FileUrl` is
expected to already point at an uploaded file).

## No Controller Exists For

- **AdminController** — there is no dedicated admin surface. Admin capability is entirely
  `[Authorize(Roles = "Admin")]` actions bolted onto CategoriesController, ProductsController,
  ReviewsController, SellersController, and OrdersController. There's no admin dashboard/
  aggregation endpoint (e.g. platform-wide stats, user management, role management).
- **TaxController** — `TaxRate` is a real entity, applied during checkout via
  `Common/Services/GstTaxCalculator`, but there's no CRUD endpoint to manage tax rates — they can
  only be seeded or edited directly in the database today.
- **CustomersController** — `Customer` itself (profile fields like `DateOfBirth`,
  `ProfileImageUrl`) still has no standalone CRUD surface; it's only touched indirectly through
  Cart/Orders/Reviews/Wishlist via `ICurrentUserService`. Its `Addresses` sub-resource *does* now
  have a full surface — see `CustomerAddressesController` above.
- **Users/Roles management** — no endpoint to list/manage users or assign roles beyond what
  happens automatically at registration.

## Maintenance

Update this file whenever a controller action is added, removed, or its handler's completeness
changes.
