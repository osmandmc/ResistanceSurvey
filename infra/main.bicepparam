using 'main.bicep'

param environmentName = 'test'

param monthlyBudget = 10
param budgetStartDate = '2026-10-01'

// Kept out of the repository; the infra-test workflow passes them from GitHub secrets and variables.
param budgetContactEmail = readEnvironmentVariable('BUDGET_EMAIL', '')
param sqlAdminPassword = readEnvironmentVariable('SQL_ADMIN_PASSWORD')
param adminPassword = readEnvironmentVariable('ADMIN_PASSWORD')
param pipelineBaseUrl = readEnvironmentVariable('PIPELINE_BASE_URL', '')
param pipelineFunctionKey = readEnvironmentVariable('PIPELINE_FUNCTION_KEY', '')
