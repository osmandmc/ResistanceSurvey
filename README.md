# direnistakip (ResistanceSurvey)

ASP.NET Core 9 app for recording labour resistances and protests from news articles.

## Secrets

No secrets are stored in this repository. The app refuses to start without these settings:

| Setting | What |
|---|---|
| `ConnectionStrings:RSConnectionString` | SQL Server connection string |
| `AppSettings:Secret` | Signs login tokens (any long random string) |
| `AppSettings:AdminPass` | Password of the `admin` user |
| `AppSettings:UserPass` | Password of the `user` user |

**Locally**, keep them in .NET user-secrets (stored in your home folder, outside the repository). From the repository root:

```bash
dotnet user-secrets --project RS.MVC set "ConnectionStrings:RSConnectionString" "<connection string>"
dotnet user-secrets --project RS.MVC set "AppSettings:Secret" "$(openssl rand -base64 48)"
dotnet user-secrets --project RS.MVC set "AppSettings:AdminPass" "<password>"
dotnet user-secrets --project RS.MVC set "AppSettings:UserPass" "<password>"
```

User-secrets are read when the app runs in the Development environment, and by the EF tools (`dotnet ef … --project RS.EF --startup-project RS.EF`). Environment variables work too (`ConnectionStrings__RSConnectionString`, `AppSettings__Secret`, …).

**Production** (SmarterASP): the deploy workflow (`.github/workflows/main.yml`) writes them into the published `appsettings.json` from the GitHub secrets `RS_CONNECTION_STRING`, `JWT_SECRET`, `ADMIN_PASSWORD` and `USER_PASSWORD`, and fails if any is missing.

**Test environment** (Azure): see `infra/README.md`.

## New front end (RS.Web)

Vue 3 + TypeScript + Vite + [Naive UI](https://www.naiveui.com). Served by RS.MVC at `/app`; the old pages stay where they are until replaced. Needs Node 22.12+ (the current LTS is fine).

```bash
cd RS.Web
npm install            # once, and after package changes
npm run dev            # http://localhost:5173/app/ with hot reload; /api goes to RS.MVC on http://localhost:56455
npm run build          # type check + build into RS.MVC/wwwroot/rsweb (not committed)
```

For `npm run dev`, start RS.MVC as well (`dotnet run --project RS.MVC`, profile "RS.MVC" listens on port 56455). Another port: `RS_API_URL=http://localhost:5000 npm run dev`.

`RS.MVC/wwwroot/app` holds the old pages' scripts; the new app's build goes to `wwwroot/rsweb` and is served at `/app`.
