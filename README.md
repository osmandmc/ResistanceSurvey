# direnistakip (ResistanceSurvey)

ASP.NET Core 9 app for recording labour resistances and protests from news articles.

## Secrets

No secrets are stored in this repository. The app needs these settings (it refuses to start without the connection string):

| Setting | What |
|---|---|
| `ConnectionStrings:RSConnectionString` | SQL Server connection string |
| `AppSettings:AdminPass` | Only for the very first sign-in: while the database has no account, `admin` with this password creates the first yönetici. Ignored once an account exists |

**Accounts** (F4a): everyone signs in with their own account. A yönetici invites people in Tanımlar → Kullanıcılar, which gives a one-time link (`/app/welcome?token=…`, 7 days) to send them directly; they set their own password through it. A forgotten password gets a new link the same way. Passwords and links are stored only as hashes.

**Locally**, keep them in .NET user-secrets (stored in your home folder, outside the repository). From the repository root:

```bash
dotnet user-secrets --project RS.MVC set "ConnectionStrings:RSConnectionString" "<connection string>"
dotnet user-secrets --project RS.MVC set "AppSettings:AdminPass" "<password>"
```

User-secrets are read when the app runs in the Development environment, and by the EF tools (`dotnet ef … --project RS.EF --startup-project RS.EF`). Environment variables work too (`ConnectionStrings__RSConnectionString`, `AppSettings__AdminPass`, …).

**Production** (SmarterASP) still runs the old app from the `legacy` branch: its deploy workflow (`.github/workflows/main.yml` on that branch) writes them into the published `appsettings.json` from the GitHub secrets `RS_CONNECTION_STRING`, `JWT_SECRET`, `ADMIN_PASSWORD` and `USER_PASSWORD`, and fails if any is missing.

**Test environment** (Azure): see `infra/README.md`.

## Local database

A local run must not touch a real database: in Development the app refuses to start unless the database is on this
machine (`LocalDev:AllowRemoteDatabase=true` overrides it). Use a local copy of the Azure **test** database:

```bash
docker compose up -d                                      # SQL Edge (the SQL Server engine, native on Apple silicon)
SOURCE_DB='<test database connection string>' tools/copy-test-db.sh
```

The script exports the test database (read only) to `~/.direnistakip/test-copy.bacpac`, imports it into the local
server and points the app's user-secrets at it; run it again to refresh the copy. The copy holds real data: it stays in
your home folder, never in the repository. Needs `sqlpackage` (`dotnet tool install -g microsoft.sqlpackage`).

In Development the app talks to a pipeline running on this machine (`http://localhost:7071`, no key; see the pipeline's
README, "Local development"). To use the Azure test pipeline instead, set `Pipeline:BaseUrl` and `Pipeline:FunctionKey`
in user-secrets.

## New front end (RS.Web)

Vue 3 + TypeScript + Vite + [Naive UI](https://www.naiveui.com). Served by RS.MVC at `/app` (the site root redirects there); the old Razor pages were removed in R12c and live on only in the `legacy` branch. Needs Node 22.12+ (the current LTS is fine).

```bash
cd RS.Web
npm install            # once, and after package changes
npm run dev            # http://localhost:5173/app/ with hot reload; /api goes to RS.MVC on http://localhost:56455
npm run build          # type check + build into RS.MVC/wwwroot/rsweb (not committed)
```

For `npm run dev`, start RS.MVC as well (`dotnet run --project RS.MVC`, profile "RS.MVC" listens on port 56455). Another port: `RS_API_URL=http://localhost:5000 npm run dev`.

`RS.MVC/wwwroot/app` holds the old pages' scripts; the new app's build goes to `wwwroot/rsweb` and is served at `/app`.
