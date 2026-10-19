# Tasks — direnistakip app (ResistanceSurvey)

Implemented one at a time, each after approval. **You** = needs your action or access; otherwise Claude implements and you review. Pipeline work is tracked separately in the pipeline repo's `tasks.md`.

Status: ☐ open · ◐ in progress · ☑ done

## Decisions

- **Stack.** Backend stays ASP.NET Core 9 + EF Core 9 + SQL Server (`RS.MVC`, `RS.EF`, `RS.COMMON`). New screens are a separate front-end project, **`RS.Web`**: Vue 3 + TypeScript + Vite + **Naive UI** (MIT; theme set to the mockup's colours and IBM Plex Sans). PrimeVue was the first choice, but version 5 (2026) moved to a commercial licence with a key; Naive UI has no conditions. Its build goes to `RS.MVC/wwwroot/rsweb` and is served by the same site at `/app` (`wwwroot/app` holds the old pages' scripts): one login, one deploy. CI builds it; bundles are not committed.
- **Old pages stay until replaced.** Razor + Semantic UI + jQuery pages and the current Vue case form keep working; each new screen replaces one. When the last is replaced, Semantic UI, jQuery and the webpack build are removed.
- **Sign-in for the new app is an HttpOnly cookie** (`/api/auth/*`), not a token in `localStorage`; the backend also accepts the old bearer token until the old screens are retired.
- **Pipeline access goes through the backend.** The browser never sees the pipeline's function key; `RS.MVC` calls the pipeline API and exposes its own JSON endpoints.
- **Target screens:** see the mockup (Gelen Kutusu, İnceleme, Haber Arşivi, Durum Paneli, Vakalar, Yükleme): https://claude.ai/artifact/DhTuKR4eCcn2gdnBQjtUq8
- **Test environment in Azure, production untouched.** The redesign is tried on a test app in Azure with a copy of the database. Production stays on SmarterASP until the redesign is proven.

## Pipeline integration (moved from the pipeline's task list)

| # | Task | Notes |
|---|---|---|
| ☑ B2 | `News.ArticleId` + cut-over setting | EF migration: nullable `ArticleId`, unique where set. `Pipeline:CutoverMonth`. Existing rows stay `NULL` (no backfill) |
| ☑ B3 | Excel upload → pipeline | For pipeline sources (`Pipeline:Sources`) from the cut-over month: `POST /api/articles`; otherwise the local import as before |
| — B5 | Inbox from the pipeline | Replaced by R5 (Inbox) and R6 (Review) below |

## Phase A — Test environment in Azure

| # | Task | Deliverable | Done when | Who |
|---|---|---|---|---|
| ☑ R1 | Azure test environment | `infra/` (Bicep): resource group, App Service (F1), Azure SQL (free offer), app settings for secrets, budget alert; trust for this repo on the GitHub deploy identity | Resources exist; budget alert set | Claude writes, **you** run the one-time setup |
| ☑ R2 | Database copy | Export of the live database (read-only) imported into Azure SQL. SmarterASP's `rsuser` user cannot exist in Azure SQL: export with `/p:VerifyExtraction=False`, then remove `rsuser` from the `.bacpac` before importing | Test app reads the copied data | **You** export, Claude guides |
| ☑ R3 | Deploy workflow | GitHub Actions: push to `redesign` builds and deploys to the test app; secrets only in Azure | Current app runs on the test URL | Claude |

## Phase B — New screens

| # | Task | Notes |
|---|---|---|
| ◐ R4 | `RS.Web` project + layout | In increments: ☑ **R4a** cookie sign-in (`/api/auth/login`, `/logout`, `/me`; bearer still accepted) · ☑ **R4b** `RS.Web` skeleton served at `/app` · ☑ **R4d** deploy builds the front end · ◐ **R4c** login screen, top menu, empty pages, “Eski ekranlar” link (English paths: `/app/login`, `/inbox`, `/archive`, `/cases`, `/status`, `/upload`) |
| ◐ R5 | Gelen Kutusu (Inbox) | ☑ **R5a** backend: `GET /api/inbox` (open, parsed articles per month/source; articles already handled in the app are hidden and their outcome re-sent), `POST /api/inbox/{id}/dismiss` (thin `News` row, then `Dismissed`; optional reason); `RS.Tests` project · ◐ **R5b** the Inbox page (month/source filters, “Elle incelenmeli” list, “İlgisiz” with optional reason, load more; “Aç” opens the original until R6). “Taslak hazır” tab empty until AI drafts exist; processing count waits for pipeline B6 |
| ◐ R6 | İnceleme (Review) | Article beside the full case form (all fields of today's form). New resistance / add protest to existing / link only. Save: case + thin `News` row (`ArticleId`) + link in one transaction, then `Digested`. In increments: ☑ **R6a** backend (`GET/POST /api/review/{articleId}`, `GET /api/review/resistances`, required fields checked on the server) · ☐ **R6b** screen layout · ☐ **R6c** case part of the form · ☐ **R6d** protest part · ☐ **R6e** saving end to end |
| ☐ R7 | Haber Arşivi (Archive) | All articles with status and timeline; needs title search in the pipeline API |
| ☐ R8 | Vakalar (Cases) | Today's list plus linked articles per case |
| ☐ R9 | Yükleme (Upload) | The B3 upload on its own page, with the result summary |

## Refactoring (from the code review, 2026-10-19)

| # | Task | Notes |
|---|---|---|
| ☑ F1 | Small fixes | Login on the four data actions that had none (`Resistance/News`, `Resistance/Export`, `Corporation/IsTradeUnion`, `CheckTradeUnion`); `Enums.cs` re-saved as UTF-8 (result labels were garbled); `appsettings.Development.json` name; removed `Startup.cs`, `RS.Tests.Api` and commented-out code |
| ◐ F2 | **CQRS-lite instead of the `…Application` classes** | One handler per use case (`CreateCase`, `AddProtest`, `LinkArticle`, `DismissArticle`, `GetInbox`, `SearchCases` …): queries project straight from EF, commands validate and write. No MediatR (commercial licence since v13): plain handlers, injected directly. ☑ Part 1: the new screens' code is in `RS.MVC/Features/` (Inbox: `GetInbox`, `DismissArticle`; Review: `GetReviewArticle`, `SearchCases`, `CreateCaseFromArticle`, `AddProtestFromArticle`, `LinkArticleToCase`; shared: `ArticleNewsService`, `OutcomeSender`, `ArticleCaseLinker`). ☐ Part 2: move the old `ResistanceApplication` (604 lines) use case by use case |
| ☑ F3 | `ArticleNews` as an injected service | Today a static helper; becomes part of the F2 handlers. Source names → a small `SourceCatalog` |
| ☐ F4 | Per-person accounts | Instead of two shared logins with plain-text passwords in config: users table, hashed passwords, who-did-what. Needed before production uses the redesign |
| ☐ F5 | Request models with ids only | 18 request models bind EF entities (over-posting; `{id: -1}` creates lookup values). New APIs already use ids |
| ☐ F6 | Global error handling and logging | Exception handler middleware; no bare 500s |
| ☐ F7 | Build and test on pull requests | Today only the deploy workflows build |
| ☐ F8 | Smaller clean-ups | Remove the unused .NET Framework package `Microsoft.Web.Deployment`; async EF calls where code is touched; Excel export out of the controller; one generic lookup admin instead of five near-identical controllers; named constants for intervention type 7 and corporation type 1; decide whether to switch on the configured tr-TR request localization |

## Later

| # | Task | Waits for |
|---|---|---|
| ☐ R10 | Durum Paneli (Dashboard) | Pipeline B6 (`GET /metrics`) |
| ☐ R11 | AI pre-fill, evidence, flags in Review | Pipeline phase 3 (drafts) |
| ☐ R12 | Retire old pages, Semantic UI, jQuery, webpack | R5–R9 |
| ☐ R13 | Secrets out of `appsettings*.json` and rotated; login passwords out of code | Deferred by you (2026-10-18); do before production uses the redesign |
| ◐ R15 | Login and page protection | Found on the test app (2026-10-19): the login form is `/` (POST `/Home/Login`); auth was JWT bearer only and the old pages render without login. New app: cookie sign-in (R4a) and login required on every screen (R4c). Old pages: protected or retired with R12 |
| ☐ R14 | Production move | Redesign proven on the test app; decide hosting (SmarterASP or Azure, D-013) |
