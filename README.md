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
