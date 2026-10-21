Project: ResistanceSurvey — quick orientation for AI coding assistants

Purpose

- Help an automated coding agent become productive quickly: where to look, how to build/run, and project-specific conventions.

Big picture

- ASP.NET Core MVC app with EF Core for the data access layer (`RS.EF/RSDBContext.cs`). The legacy `RS.DAL` project has been removed; rely on EF and application services instead of Dapper helpers.
- Frontend: `RS.Web` (Vue 3 + TypeScript + Naive UI) served at `/app` from `wwwroot/rsweb`; the API is under `/api` (controllers in `RS.MVC/Controllers`, use cases in `RS.MVC/Features`). The old Razor pages are only in the `legacy` branch.
- Use cases: one handler per use case under `RS.MVC/Features/<area>` (classes ending in `Handler` are registered automatically by `AddFeatureHandlers`); no MediatR. Cases and protests are saved through `CaseWriter` (`Features/Cases/CaseCommands.cs`). `RS.MVC/Applications` holds only the pipeline client, sign-in users and the test-database migrator.

How the pieces talk

- EF Core (DbContext: `RS.EF/RSDBContext.cs`) is registered via connection string `RSConnectionString` in `RS.MVC/appsettings*.json`.
- Authentication: an HttpOnly cookie (`/api/auth/login`), configured in `RS.MVC/Program.cs`; two fixed users with passwords from configuration until per-person accounts (F4).

Build / run / CI

- Local dev: dotnet 9 (CI uses .NET 9). Primary project: `RS.MVC/RS.MVC.csproj`.
- Common commands:
  - Restore: dotnet restore
  - Build: dotnet build RS.MVC/RS.MVC.csproj
  - Run (watch): dotnet watch run --project RS.MVC/RS.MVC.csproj
  - Publish: dotnet publish -c Release -o ./publish
- CI: see `.github/workflows/main.yml` — uses dotnet publish and an FTP deploy step.

Conventions & patterns to follow

- Dependency registration: handlers are picked up automatically; shared services are registered as scoped in `Program.cs`.
- Request and response shapes are records next to their handler (or in `RS.MVC/Models` when shared); requests carry ids only, never entities.
- Soft-delete: many entities use a `Deleted` boolean rather than removing rows. Preserve and respect this flag when querying (most queries filter with !Deleted).
- Lists (corporations, reasons, protest types …): typed-in new values are resolved by `ListValueResolver` and kept clean in Tanımlar (`Features/Definitions`).
- DB changes: `RS.EF` holds the DbContext and migrations (`RS.EF/Data/Migrations`, starting from `Baseline`). Every model change needs a migration (`dotnet ef migrations add … --project RS.EF --startup-project RS.EF`); `SchemaTests` fails otherwise. Text columns use `Turkish_CI_AS`; only an owner (case, protest) cascades on delete.

Testing and safety

- Tests: xUnit in `RS.Tests` (EF in-memory provider; SQL translation checked with `ToQueryString` against the SQL Server provider). Run `dotnet test RS.Tests`.
- Avoid exposing secrets: the repository is public; secrets come from user-secrets locally and app settings in Azure.

Where to look for examples

- Use cases: `RS.MVC/Features/` (e.g. `Review/CreateCaseFromArticle.cs`, `Cases/CaseCommands.cs`, `Definitions/MergeDefinitions.cs`).
- DbContext and model registrations: `RS.EF/RSDBContext.cs` (DbSet list and OnModelCreating example).
- Startup & runtime wiring: `RS.MVC/Program.cs`.

Quick tips for edits

- Keep controller actions and view model shapes backward compatible: many consumers expect specific JSON shapes (CamelCase serializer configured in Startup).
- When changing routing, keep both scoped fallbacks: `Resistance/IndexVue/{*path:nonfile}` for the old Vue app and `app/{*path:nonfile}` for RS.Web. There is no global catch-all; unknown addresses return 404.
- When introducing schema changes, update the EF entities and corresponding application-layer logic that rely on the affected tables/columns.

If you are unsure

- Prefer minimal, well-scoped changes and point the maintainers to the modified files. Ask for the preferred migration strategy before adding DB migrations.

Contact for owner/context

- Review CI (`.github/workflows/main.yml`) and `RS.MVC/appsettings*.json` for environment-specific settings.

If you'd like changes to this guidance or to add examples for a specific task, tell me which area to expand (e.g., migrations, adding API endpoints, or frontend integration).
