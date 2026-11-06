# Decisions — direnistakip app (ResistanceSurvey)

Lasting choices about how the app is built. Open tasks are issues in the private repo
[direnistakip-tasks](https://github.com/osmandmc/direnistakip-tasks/issues). The old task list with the notes of
finished tasks was removed on 2026-11-06; its last version is
[`tasks.md` at 831c0f1](https://github.com/osmandmc/ResistanceSurvey/blob/831c0f1/tasks.md).

- **Stack.** Backend stays ASP.NET Core 9 + EF Core 9 + SQL Server (`RS.MVC`, `RS.EF`, `RS.COMMON`). New screens are a separate front-end project, **`RS.Web`**: Vue 3 + TypeScript + Vite + **Naive UI** (MIT; theme set to the mockup's colours and IBM Plex Sans). PrimeVue was the first choice, but version 5 (2026) moved to a commercial licence with a key; Naive UI has no conditions. Its build goes to `RS.MVC/wwwroot/rsweb` and is served by the same site at `/app` (`wwwroot/app` holds the old pages' scripts): one login, one deploy. CI builds it; bundles are not committed.
- **Old pages stay until replaced.** Razor + Semantic UI + jQuery pages and the current Vue case form keep working; each new screen replaces one. When the last is replaced, Semantic UI, jQuery and the webpack build are removed.
- **Sign-in for the new app is an HttpOnly cookie** (`/api/auth/*`), not a token in `localStorage`; the backend also accepts the old bearer token until the old screens are retired.
- **Pipeline access goes through the backend.** The browser never sees the pipeline's function key; `RS.MVC` calls the pipeline API and exposes its own JSON endpoints.
- **Target screens:** see the mockup (Gelen Kutusu, İnceleme, Haber Arşivi, Durum Paneli, Vakalar, Yükleme): https://claude.ai/artifact/DhTuKR4eCcn2gdnBQjtUq8
- **Test environment in Azure, production untouched.** The redesign is tried on a test app in Azure with a copy of the database. Production stays on SmarterASP until the redesign is proven.
- **Branches.** `master` holds the redesign and deploys to the test environment (`deploy-test`, `infra-test`). The old app lives on `legacy`, which deploys to production on SmarterASP (`main.yml` there). New work goes to `master` through short branches and pull requests.
