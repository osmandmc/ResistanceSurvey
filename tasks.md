# Tasks — direnistakip app (ResistanceSurvey)

Implemented one at a time, each after approval. **You** = needs your action or access; otherwise Claude implements and you review. Pipeline work is tracked separately in the pipeline repo's `tasks.md`.

Status: ☐ open · ◐ in progress · ☑ done

## Decisions

- **Stack.** Backend stays ASP.NET Core 9 + EF Core 9 + SQL Server (`RS.MVC`, `RS.EF`, `RS.COMMON`). New screens are a separate front-end project, **`RS.Web`**: Vue 3 + TypeScript + Vite + PrimeVue 4 (theme set to the mockup's colours and IBM Plex Sans). Its build goes to `RS.MVC/wwwroot/app` and is served by the same site: one login, one deploy. CI builds it; bundles are not committed.
- **Old pages stay until replaced.** Razor + Semantic UI + jQuery pages and the current Vue case form keep working; each new screen replaces one. When the last is replaced, Semantic UI, jQuery and the webpack build are removed.
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
| ◐ R3 | Deploy workflow | GitHub Actions: push to `redesign` builds and deploys to the test app; secrets only in Azure | Current app runs on the test URL | Claude |

## Phase B — New screens

| # | Task | Notes |
|---|---|---|
| ☐ R4 | `RS.Web` project + layout | Vite + Vue 3 + TS + PrimeVue, theme, top menu, routes, login reuse; CI build; old pages linked from the menu until replaced |
| ☐ R5 | Gelen Kutusu (Inbox) | Open articles from the pipeline, processing count, “İlgisiz” sends `Dismissed`. “Taslak hazır” tab empty until AI drafts exist |
| ☐ R6 | İnceleme (Review) | Article beside the full case form (all fields of today's form). New resistance / add protest to existing / link only. Save: case + thin `News` row (`ArticleId`) + link in one transaction, then `Digested` |
| ☐ R7 | Haber Arşivi (Archive) | All articles with status and timeline; needs title search in the pipeline API |
| ☐ R8 | Vakalar (Cases) | Today's list plus linked articles per case |
| ☐ R9 | Yükleme (Upload) | The B3 upload on its own page, with the result summary |

## Later

| # | Task | Waits for |
|---|---|---|
| ☐ R10 | Durum Paneli (Dashboard) | Pipeline B6 (`GET /metrics`) |
| ☐ R11 | AI pre-fill, evidence, flags in Review | Pipeline phase 3 (drafts) |
| ☐ R12 | Retire old pages, Semantic UI, jQuery, webpack | R5–R9 |
| ☐ R13 | Secrets out of `appsettings*.json` and rotated; login passwords out of code | Deferred by you (2026-10-18); do before production uses the redesign |
| ☐ R14 | Production move | Redesign proven on the test app; decide hosting (SmarterASP or Azure, D-013) |
