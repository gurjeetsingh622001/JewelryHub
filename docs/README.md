# JewelryHub

JewelryHub is a jewelry marketplace and jewelry-business management platform. It serves four
kinds of users through one backend:

- **Customer** — browse products, manage a cart/wishlist, place orders, leave reviews.
- **Seller** — list products, manage inventory, fulfill orders, respond to reviews, go through
  KYC verification.
- **Jewelry Union** — a trade-body governance layer for sellers: create/join a union, officer
  roles, meetings with agendas/minutes/action items, governance polls, announcements, and a
  document library. See [BACKEND_PROGRESS.md](BACKEND_PROGRESS.md).
- **Admin** — approve/reject sellers, moderate reviews, manage the catalog, review KYC documents.
  There is no separate admin surface; admin capability is role-gated actions on the same
  controllers everyone else uses.

## Technology Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 8 |
| API | ASP.NET Core Web API, MVC Controllers (no Minimal APIs) |
| CQRS / mediator | MediatR |
| Validation | FluentValidation, run as a MediatR pipeline behavior |
| Data access | EF Core 8 (SQL Server), Repository + Unit of Work over the DbContext |
| Auth | Custom JWT access/refresh tokens, BCrypt password hashing (not ASP.NET Identity) |
| Real-time | SignalR (`/hubs/notifications`) |
| Logging | Serilog, plus a MediatR `LoggingBehavior` that times every request |
| API docs | Swashbuckle/Swagger (Development only) |
| Frontend | Angular 22, standalone components + signals, Angular Material + PrimeNG + Tailwind CSS |

## Solution Structure

```
JewelryHub.sln
src/
├── JewelryHub.Domain          # Entities, enums, zero external dependencies
├── JewelryHub.Application     # CQRS use cases (MediatR), validation, mapping, interfaces
├── JewelryHub.Persistence     # EF Core DbContext, entity configs, Repository/UnitOfWork, migrations
├── JewelryHub.Infrastructure  # JWT issuing, password hashing, current-user accessor
└── JewelryHub.API             # Controllers, SignalR hubs, middleware, Program.cs
client/                        # Angular SPA — separate npm workspace, not part of JewelryHub.sln
docs/                          # This documentation set
```

There is currently no `tests/` directory in this repository.

## Architecture

Clean Architecture / Onion Architecture with CQRS. Dependencies point inward only:

```
API → Infrastructure ─┐
API → Persistence ─────┼──→ Application → Domain
API → Application ─────┘
```

`Domain` has no package or project references at all — it's plain C# with no framework
dependency, by design. See [ARCHITECTURE.md](ARCHITECTURE.md) for the full breakdown of each
layer and the conventions new code should follow.

## Features (by completion — see [PROJECT_STATUS.md](PROJECT_STATUS.md) for detail)

Implemented end-to-end (Domain → Persistence → Application → API):
Auth, Cart, Categories, Notifications, Orders, Payments (partial — see below), Products, Reviews,
Sellers (incl. KYC approval flow), Wishlist, **Jewelry Union** (union lifecycle + admin approval,
membership + officer roles, announcements, documents, events, meetings with agendas/minutes/
action items, and governance polls with voting).

Modeled in the database but with no Application/API layer yet:
**Tax** (rate configuration only — tax is *applied* during checkout, but there's no CRUD surface
for tax rates).

Known incomplete sub-behaviors inside otherwise-working features:
- Payment confirmation is a stand-in for a real gateway webhook (Razorpay/Stripe) — the logic is
  real, but nothing calls it from an actual payment provider yet.

Frontend: the Angular foundation (auth, routing, HTTP layer) is built — see
[FRONTEND_PROGRESS.md](FRONTEND_PROGRESS.md) — but no feature screens exist yet.

## How to Run

### Backend

Prerequisites: .NET 8 SDK, SQL Server (local or remote), the `dotnet-ef` global tool.

```bash
# Restore & build
dotnet build JewelryHub.sln

# Apply the database schema (or just start the API — Program.cs calls
# Database.MigrateAsync() automatically on startup and seeds the Admin/Seller/Customer roles)
dotnet ef database update --project src/JewelryHub.Persistence --startup-project src/JewelryHub.Persistence

# Run the API
dotnet run --project src/JewelryHub.API
```

Swagger UI is available at `/swagger` when running in the `Development` environment.

### Frontend

Prerequisites: Node.js 20+, npm.

```bash
cd client
npm install
npm start   # ng serve — http://localhost:4200
```

Expects the API at the URL configured in `client/src/environments/environment.development.ts`
(`https://localhost:65334` by default — matches `launchSettings.json`).

### Configuration

All configuration lives in `src/JewelryHub.API/appsettings.json`. Set secrets (the JWT `Secret`,
the real connection string) via **User Secrets** or environment variables — never commit real
values. Key sections: `ConnectionStrings:DefaultConnection`, `Jwt` (Issuer/Audience/token
lifetimes/Secret), `Cors:AllowedOrigins` (defaults to `http://localhost:4200`, i.e. an Angular
dev server), `Serilog`.

For `dotnet ef` commands, the design-time factory
(`src/JewelryHub.Persistence/JewelryHubDbContextFactory.cs`) reads the connection string from the
`JEWELRYHUB_CONNECTION_STRING` environment variable, falling back to a local
`Trusted_Connection=True` default if unset.

## Database

EF Core 8, Code-First, SQL Server. GUID primary keys generated client-side. Audit fields
(`CreatedAtUtc/By`, `ModifiedAtUtc/By`) and soft-delete are stamped automatically by a
`SaveChangesInterceptor` — never set them by hand in a handler. See
[DATABASE.md](DATABASE.md) for the full table/relationship inventory.

## Authentication

Custom JWT: an access token plus an opaque refresh token, with refresh-token rotation (each use
revokes the old token and issues a new one; reuse of a revoked token revokes every session for
that user as a theft signal). Passwords are hashed with BCrypt. Roles (`Customer`, `Seller`,
`Admin`) are enforced with `[Authorize(Roles = "...")]` on individual controller actions — there
is no ASP.NET Core Identity and no cookie auth.

## Development Guidelines

Follow the conventions already established in the codebase rather than introducing new ones:

- **New use case** → add a `Features/{Area}/{Commands|Queries}/{Name}/{Name}Command.cs` (or
  `Query.cs`) in Application, bundling the record, its FluentValidation validator, and its
  handler in one file. Register nothing manually — MediatR/FluentValidation are wired via
  assembly scanning in `Application/DependencyInjection.cs`.
- **New entity** → add it to `Domain/{Aggregate}/`, an `IEntityTypeConfiguration<T>` under
  `Persistence/Configurations/{Aggregate}/`, a `DbSet<T>` on `JewelryHubDbContext`, and a lazy
  repository property on `UnitOfWork`. Then run
  `dotnet ef migrations add <Name> --project src/JewelryHub.Persistence --startup-project src/JewelryHub.Persistence`.
- **Controllers stay thin** — inject `IMediator` only, one-liner bodies. All logic belongs in the
  Application handler.
- **Don't throw generic exceptions.** Use the typed ones in
  `Application/Common/Exceptions/ApplicationExceptions.cs`
  (`NotFoundException`, `BusinessRuleException`, `ForbiddenAccessException`,
  `AuthenticationFailedException`) — the global `ExceptionHandlingMiddleware` maps them to
  RFC 7807 `ProblemDetails` responses automatically.
- **Lists return `PagedResult<T>`** (`Application/Common/Models/PagedResult.cs`); everything else
  returns a plain DTO. There is no generic `ApiResponse<T>` envelope.
- **Testing is not a current priority** (see [PROJECT_STATUS.md](PROJECT_STATUS.md)) — unit,
  integration, and architecture tests are planned for after functionality is complete.
- **Keep the docs in `docs/` in sync.** Whenever a feature is completed, update
  `PROJECT_STATUS.md`, `ROADMAP.md`, `CHANGELOG.md`, and (if applicable)
  `API_PROGRESS.md`/`DATABASE.md`. These files are the project's persistent memory across
  sessions — see [PROJECT_STATUS.md](PROJECT_STATUS.md) for the maintenance rule.
