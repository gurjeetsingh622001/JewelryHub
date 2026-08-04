# Architecture

## Style

Clean Architecture (Onion Architecture) with CQRS via MediatR, and a **vertical-slice**
organization inside the Application layer (feature folders, not technical-layer folders like
"Services"/"Repositories" inside Application).

```
                ┌─────────────────────┐
                │   JewelryHub.API    │  Controllers, SignalR Hubs, Middleware
                └──────────┬──────────┘
           ┌────────────────┴───────────────┐
┌──────────▼──────────┐          ┌──────────▼───────────┐
│ JewelryHub.Persistence│          │ JewelryHub.Infrastructure│  EF Core / JWT / BCrypt
└──────────┬──────────┘          └──────────┬───────────┘
           └────────────────┬───────────────┘
                ┌────────────▼────────────┐
                │  JewelryHub.Application  │  MediatR use cases, validation, interfaces
                └────────────┬────────────┘
                ┌────────────▼────────────┐
                │    JewelryHub.Domain     │  Entities, enums — zero dependencies
                └──────────────────────────┘
```

Dependency direction is enforced strictly inward. `JewelryHub.Domain.csproj` has no
`PackageReference` and no `ProjectReference` at all — no EF Core, no ASP.NET Core, nothing.
That's deliberate: the domain model must be usable, testable, and reasoned about without any
framework in the picture.

## Layers

### Domain (`src/JewelryHub.Domain`)

Plain C# entities, organized one folder per aggregate/bounded context: `Cart`, `Catalog`,
`Customers`, `Identity`, `Notifications`, `Orders`, `Payments`, `Reviews`, `Sellers`, `Tax`,
`Unions`, `Wishlist`, plus `Common` (`BaseEntity` — Guid `Id`, audit fields, `RowVersion`,
`ISoftDelete`) and `Enums`. No behavior beyond simple invariants lives here beyond what the
entities themselves encode.

### Application (`src/JewelryHub.Application`)

CQRS use cases via MediatR, organized as vertical slices:

```
Features/{Area}/{Commands|Queries}/{UseCaseName}/{UseCaseName}Command.cs
```

Each file bundles three things together: the MediatR request record (`IRequest<TResponse>`),
its `FluentValidation` validator, and its `IRequestHandler<,>` implementation. There is no
separate `*Handler.cs` file convention — resist splitting these apart for "consistency with other
codebases"; splitting them here would be inconsistent with this one.

Two open-generic pipeline behaviors wrap every request, registered in that fixed order in
`Application/DependencyInjection.cs`:

1. `LoggingBehavior<,>` — times the handler, logs a warning if it exceeds 1000ms.
2. `ValidationBehavior<,>` — runs the FluentValidation validator (if one is registered for the
   request type) and throws `FluentValidation.ValidationException` on failure, before the handler
   ever runs.

New cross-cutting behaviors (e.g. caching, authorization checks beyond role attributes) should be
added here in the same open-generic style, not as ad hoc code inside handlers.

`Common/` holds everything that doesn't belong to one feature: `Exceptions/` (the typed exception
hierarchy), `Interfaces/` (`IUnitOfWork`, `IJwtTokenService`, `ICurrentUserService`,
`IRealtimeNotifier`, etc. — defined here, implemented in Infrastructure/API), `Models/`
(`PagedResult<T>`), `Mappings/` (hand-written mapper classes per feature, e.g. `CartMapper`,
`ProductMapper` — coexisting with the registered AutoMapper package rather than replacing it),
`Behaviors/`, and `Services/` (`NotificationService`, the tax calculator).

### Persistence (`src/JewelryHub.Persistence`)

EF Core implementation. `JewelryHubDbContext` defines every `DbSet<T>`; `OnModelCreating` applies
all `IEntityTypeConfiguration<T>` classes via assembly scan and sets a global default
`decimal(18,4)` precision for unconfigured decimal columns.

Access pattern is **Repository + Unit of Work**, not raw `DbContext` injection into handlers:

- `Repository<T>.Query()` returns `AsNoTracking()` — the default for read paths.
- `Repository<T>.QueryTracking()` — for paths that need to mutate and save.
- `UnitOfWork` lazily constructs one `Repository<T>` per aggregate
  (`_field ??= new Repository<T>(_context)`) and exposes `SaveChangesAsync` /
  `ExecuteInTransactionAsync` (the latter wraps `CreateExecutionStrategy()` so it composes safely
  with `EnableRetryOnFailure`).

Cross-cutting persistence concerns are handled by `Interceptors/AuditableEntitySaveChangesInterceptor`:
it stamps `CreatedAtUtc/By` and `ModifiedAtUtc/By` from `ICurrentUserService`, and converts hard
deletes of `ISoftDelete` entities into an `IsDeleted = true` update. Handlers never set these
fields manually.

`Configurations/{Area}/{Entity}Configuration.cs` mirrors the Domain folder structure 1:1.
`Migrations/` holds the EF Core migration history (see [DATABASE.md](DATABASE.md)).
`JewelryHubDbContextFactory` is a design-time-only factory so `dotnet ef` commands don't need to
spin up the API host.

### Infrastructure (`src/JewelryHub.Infrastructure`)

Implements Application-defined interfaces that need a concrete technical dependency:
`IJwtTokenService` (issues access + refresh token pairs, hashes refresh tokens for storage),
`IPasswordHasher` (BCrypt), `ICurrentUserService` (reads the authenticated user's claims off
`IHttpContextAccessor`). Registered in `Infrastructure/DependencyInjection.cs`. Requires a
`<FrameworkReference Include="Microsoft.AspNetCore.App" />` in its `.csproj` because it's a plain
class library that still needs ASP.NET Core types (`ClaimsPrincipal.FindFirstValue`,
`IHttpContextAccessor`) that only ship in the shared framework, not as a standalone NuGet package
on modern .NET.

### API (`src/JewelryHub.API`)

Thin presentation layer — traditional MVC Controllers (not Minimal APIs), one per resource,
routed `api/v1/{resource}`. Every action constructor-injects `IMediator` only and is a one-liner:
send the command/query, map the result to an `ActionResult`. All business logic lives in
Application handlers — if you find yourself writing an `if` statement in a controller beyond
picking an HTTP status code, it probably belongs in a handler instead.

Also hosts: `Middleware/ExceptionHandlingMiddleware` (global error → `ProblemDetails` mapping),
`Hubs/NotificationsHub` + `Hubs/SubClaimUserIdProvider` (SignalR), `Services/SignalRNotifier`
(implements Application's `IRealtimeNotifier` — deliberately kept in API rather than
Infrastructure, since it's tied to the concrete `NotificationsHub` type the host registers), and
`Program.cs` (composition root — calls each layer's `Add{Layer}()` extension method, then adds
API-only registrations: controllers, SignalR, Swagger, JWT bearer auth with a SignalR
query-string token accommodation, CORS).

## Cross-Cutting Conventions

- **Error handling**: no try/catch in controllers or handlers. Throw one of the four typed
  exceptions in `Application/Common/Exceptions/ApplicationExceptions.cs`
  (`NotFoundException` → 404, `BusinessRuleException` → 400 with optional field error,
  `ForbiddenAccessException` → 403, `AuthenticationFailedException` → 401).
  `FluentValidation.ValidationException` is mapped separately to 400 with a
  `Dictionary<string,string[]>` grouped-by-property payload. Everything else falls through to a
  generic 500; stack traces are attached only in Development, and everything is logged.
- **Response shape**: plain DTOs for single items; `PagedResult<T>` for lists
  (`pageNumber`/`pageSize` query params default to `1`/`20` everywhere); `ProblemDetails` for
  errors. No generic `ApiResponse<T>` wrapper.
- **IDs**: client-generated GUIDs everywhere, not database-generated sequential integers — avoids
  leaking business volume (e.g. total order count) through the ID space.
- **Naming**: `{Resource}Controller`, routed lowercase-plural `api/v1/{resource}`; Commands/Queries
  are C# `record`s implementing `IRequest<TResponse>`; controller-only request-body DTOs are
  declared as `record`s at the bottom of the controller file that uses them.

## Known Architectural Gaps

- **No test project.** No architecture tests exist to enforce the dependency rules above
  mechanically — they're currently just convention, verified by code review.
- **No CI pipeline** — `.github/workflows/` exists but is empty.
- **Tax** has a Domain entity (`TaxRate`) applied during checkout, but no Application feature
  slice or API controller to manage rates — the only current gap of this shape. (Union governance
  — the previous entry here — now has a full Application/API layer; see
  [BACKEND_PROGRESS.md](BACKEND_PROGRESS.md).)
