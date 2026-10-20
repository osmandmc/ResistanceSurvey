# Azure test environment

A test copy of the app for the redesign (tasks R1–R3). Production stays on SmarterASP and is not touched: it runs the old app from the `legacy` branch.

| Resource | Tier | Cost |
|---|---|---|
| Resource group `rg-direnistakip-app-test` (swedencentral) | — | — |
| App Service plan + web app, Windows, .NET 9 | F1 (free): sleeps when idle, 60 CPU minutes a day | Free |
| Azure SQL server + database `rsdb` | Free offer (one per subscription): 32 GB, 100,000 vCore-seconds a month, pauses when used up | Free |
| Budget alert | $10/month, email at 80% actual and 100% forecast | — |

Deleting the resource group removes everything.

**Secrets live only in Azure** (web app settings) and GitHub secrets, never in this repository. The web app's settings override `appsettings.json`, so the test app never uses the production values committed there.

## One-time setup

### 1. Bootstrap (subscription Owner, Azure Cloud Shell)

Open [Cloud Shell](https://shell.azure.com) (Bash) and run:

```bash
git clone https://github.com/osmandmc/ResistanceSurvey.git
cd ResistanceSurvey
az deployment sub create --location swedencentral --template-file infra/bootstrap/main.bicep \
  --query properties.outputs
```

It creates the resource group, lets this repository's GitHub deploy jobs sign in as the existing deploy identity (`id-github-deploy`, from the pipeline's bootstrap), and gives that identity Contributor on the new resource group only. Safe to re-run. Note the four output values.

### 2. GitHub settings (Settings → Environments → `test`)

Create an environment named **`test`** and add these as its **environment** variables and secrets. A `test` secret overrides a repository secret of the same name for the test deploy only; `ADMIN_PASSWORD` and `USER_PASSWORD` may be left to the repository secrets to share production's logins:

| Kind | Name | Value |
|---|---|---|
| Variable | `AZURE_CLIENT_ID` `AZURE_TENANT_ID` `AZURE_SUBSCRIPTION_ID` `AZURE_RESOURCE_GROUP` | The bootstrap outputs |
| Variable | `BUDGET_EMAIL` | Where budget alerts go |
| Variable | `PIPELINE_BASE_URL` | The pipeline's Function App URL, e.g. `https://func-….azurewebsites.net` |
| Secret | `SQL_ADMIN_PASSWORD` | New strong password, e.g. `openssl rand -base64 24` |
| Secret | `ADMIN_PASSWORD` `USER_PASSWORD` | Login passwords for `admin` and `user` on the test app |
| Secret | `PIPELINE_FUNCTION_KEY` | Portal → pipeline Function App → App keys → `default` |

### 3. Deploy the resources

Actions → **infra-test** → Run workflow (branch `master`). Later runs happen on every push to `master` that changes `infra/`.

The deployment's outputs (Actions log or portal → resource group → Deployments) give the web app URL and the SQL server host.

If the free SQL offer turns out to be used already, the deployment fails on the database; switch to the Basic tier (about $5/month) instead.

## Copying the data (R2)

Covered in R2: export the production database to a `.bacpac` (read-only), add your IP to the SQL server's firewall, and import into `rsdb`.
