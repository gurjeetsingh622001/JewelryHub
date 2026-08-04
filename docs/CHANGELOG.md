# Changelog

Every completed unit of work gets an entry here: date, module, files modified, summary. This is
the project's development history — kept even if chat history is lost. Newest entries at the top.

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
