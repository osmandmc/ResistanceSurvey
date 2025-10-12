Project: ResistanceSurvey — quick orientation for AI coding assistants

Purpose

- Help an automated coding agent become productive quickly: where to look, how to build/run, and project-specific conventions.

Big picture

- ASP.NET Core MVC app with EF Core for the data access layer (`RS.EF/RSDBContext.cs`). The legacy `RS.DAL` project has been removed; rely on EF and application services instead of Dapper helpers.
- Frontend: Razor Views plus a Vue 3 standalone SPA served under a catch-all route (see `RS.MVC/Program.cs` -> MapFallbackToController("IndexVue", "Resistance")).
- Services: application-level services live under `RS.MVC/Applications` (e.g., `ResistanceApplication.cs`) and are registered in `RS.MVC/Program.cs` / `Startup.cs` as scoped services.

How the pieces talk

- EF Core (DbContext: `RS.EF/RSDBContext.cs`) is registered via connection string `RSConnectionString` in `RS.MVC/appsettings*.json`.
- Authentication: JWT configured in `RS.MVC/Program.cs` and `Startup.cs` using `AppSettings.Secret` from configuration.

Build / run / CI

- Local dev: dotnet 9 (CI uses .NET 9). Primary project: `RS.MVC/RS.MVC.csproj`.
- Common commands:
  - Restore: dotnet restore
  - Build: dotnet build RS.MVC/RS.MVC.csproj
  - Run (watch): dotnet watch run --project RS.MVC/RS.MVC.csproj
  - Publish: dotnet publish -c Release -o ./publish
- CI: see `.github/workflows/main.yml` — uses dotnet publish and an FTP deploy step.

Conventions & patterns to follow

- Dependency registration: application services are registered as scoped in `Program.cs` / `Startup.cs`. Follow this pattern for new services.
- DTOs and ViewModels live under `RS.COMMON/DTO` and `RS.MVC/Models` respectively. Keep mapping and shape changes local to `RS.MVC/Applications`.
- Soft-delete: many entities use a `Deleted` boolean rather than removing rows. Preserve and respect this flag when querying (most queries filter with !Deleted).
- Named lookups: simple lookup tables (City, District, Category, etc.) are pulled through the application layer; check `ResistanceApplication` helpers before adding direct queries.
- DB changes: `RS.EF` contains the DbContext and entity definitions. There is no migrations folder in the repo — coordinate schema changes with maintainers; for local work use EF migrations aligned to the connection string.

Testing and safety

- There are no automated unit tests in the repo. When adding tests, prefer xUnit and put them under a new test project following the solution conventions.
- Avoid exposing secrets: `appsettings.development.json` currently contains a connection string; do not hardcode new credentials in this repo.

Where to look for examples

- Application service patterns and entity mappings: `RS.MVC/Applications/ResistanceApplication.cs` (complex business logic, binding helper methods like BindCorporations).
- DbContext and model registrations: `RS.EF/RSDBContext.cs` (DbSet list and OnModelCreating example).
- Startup & runtime wiring: `RS.MVC/Program.cs` and `RS.MVC/Startup.cs`.

Quick tips for edits

- Keep controller actions and view model shapes backward compatible: many consumers expect specific JSON shapes (CamelCase serializer configured in Startup).
- When changing routing, preserve the fallback route for Vue: MapFallbackToController("IndexVue", "Resistance") to avoid breaking SPA routes.
- When introducing schema changes, update the EF entities and corresponding application-layer logic that rely on the affected tables/columns.

If you are unsure

- Prefer minimal, well-scoped changes and point the maintainers to the modified files. Ask for the preferred migration strategy before adding DB migrations.

Contact for owner/context

- Review CI (`.github/workflows/main.yml`) and `RS.MVC/appsettings*.json` for environment-specific settings.

If you'd like changes to this guidance or to add examples for a specific task, tell me which area to expand (e.g., migrations, adding API endpoints, or frontend integration).
