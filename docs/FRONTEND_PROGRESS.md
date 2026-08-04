# Frontend Progress

## Status: Not Started

Checked directly on 2026-08-04: there is **no Angular project, no `angular.json`, no
`package.json`, no `node_modules`, and no frontend source directory anywhere in this repository**.
The repo currently contains only the `JewelryHub.sln` and `src/` (the five .NET projects).

## Evidence That a Frontend Is Planned

Even though no frontend code exists, the backend is already shaped around one:

- `Program.cs` registers a CORS policy literally named `"AngularApp"`.
- `Cors:AllowedOrigins` in `appsettings.json` defaults to `http://localhost:4200` — the standard
  Angular CLI dev-server port.
- The JWT-over-SignalR accommodation in `Program.cs` (`OnMessageReceived` pulling the token from
  the `access_token` query string) exists specifically because browser-based SignalR clients
  (like an Angular app using `@microsoft/signalr`) can't set an `Authorization` header on the
  WebSocket upgrade request.

So the backend is API-ready for an Angular client, but zero frontend work has actually started.

## Per-Module Frontend Status

| Module | Status |
|---|---|
| Customer UI | ❌ Missing |
| Seller UI | ❌ Missing |
| Admin UI | ❌ Missing |
| Union UI | ❌ Missing |

## Maintenance

Update this file the moment an Angular project is scaffolded into the repo — at minimum note the
Angular version, the state-management approach chosen, and which modules above have moved from
❌ to 🚧/✅.
