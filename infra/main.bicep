// Test environment for the redesigned app (R1): a Windows web app on the free App Service tier,
// an Azure SQL database on the free offer holding a copy of the production data, and a budget alert.
// Deployed by .github/workflows/infra-test.yml into the resource group from infra/bootstrap.
// Production stays on SmarterASP and is not touched.

param location string = resourceGroup().location
param environmentName string = 'test'

@description('SQL administrator login; the app connects with it too (test environment only).')
param sqlAdminLogin string = 'rsadmin'

@secure()
@description('SQL administrator password. Kept out of the repository (GitHub secret SQL_ADMIN_PASSWORD).')
param sqlAdminPassword string

@secure()
@description('Signs and verifies the app\'s login tokens (AppSettings:Secret). GitHub secret JWT_SECRET.')
param jwtSecret string

@secure()
@description('Password of the app\'s "admin" login (AppSettings:AdminPass). GitHub secret ADMIN_PASSWORD.')
param adminPassword string

@secure()
@description('Password of the app\'s "user" login (AppSettings:UserPass). GitHub secret USER_PASSWORD.')
param userPassword string

@description('News pipeline Function App root, e.g. https://func-xyz.azurewebsites.net. Empty leaves the pipeline off.')
param pipelineBaseUrl string = ''

@secure()
@description('Function key for the pipeline API. GitHub secret PIPELINE_FUNCTION_KEY.')
param pipelineFunctionKey string = ''

@description('First month served by the pipeline ("yyyy-MM"); empty = pipeline off.')
param pipelineCutoverMonth string = ''

@description('Monthly budget amount in the billing currency.')
param monthlyBudget int = 10

@description('First day of the budget\'s first month (yyyy-MM-01). Keep fixed across deployments.')
param budgetStartDate string

@description('Address for budget alerts. Empty skips the budget.')
param budgetContactEmail string = ''

var suffix = uniqueString(resourceGroup().id)
var databaseName = 'rsdb'

// ---------- Database: Azure SQL, free offer (one per subscription; pauses when the monthly free amount is used) ----------

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: 'sql-direnistakip-${environmentName}-${suffix}'
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// Lets Azure services (the web app) reach the server. Your own IP is added by hand for the data import (R2).
resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource database 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'GP_S_Gen5_2'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 2
  }
  properties: {
    useFreeLimit: true
    freeLimitExhaustionBehavior: 'AutoPause'
    autoPauseDelay: 60
    minCapacity: json('0.5')
    maxSizeBytes: 34359738368 // 32 GB, the free offer's limit
  }
}

// ---------- Web app: Windows, .NET 9, free tier ----------

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: 'asp-direnistakip-app-${environmentName}'
  location: location
  kind: 'app'
  sku: {
    name: 'F1'
    tier: 'Free'
  }
}

// Wait up to a minute for the paused database to resume.
var connectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${databaseName};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;'

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: 'app-direnistakip-${environmentName}-${suffix}'
  location: location
  kind: 'app'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      metadata: [
        { name: 'CURRENT_STACK', value: 'dotnet' }
      ]
      use32BitWorkerProcess: true // the free tier runs 32-bit only
      alwaysOn: false // not available on the free tier; the app sleeps when idle
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      // Settings here override appsettings.json (environment variables win), so the test app never
      // falls back to the production values committed in that file.
      connectionStrings: [
        { name: 'RSConnectionString', connectionString: connectionString, type: 'SQLAzure' }
      ]
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: 'Test' }
        { name: 'AppSettings__Secret', value: jwtSecret }
        { name: 'AppSettings__AdminPass', value: adminPassword }
        { name: 'AppSettings__UserPass', value: userPassword }
        { name: 'Pipeline__BaseUrl', value: pipelineBaseUrl }
        { name: 'Pipeline__FunctionKey', value: pipelineFunctionKey }
        { name: 'Pipeline__CutoverMonth', value: pipelineCutoverMonth }
      ]
    }
  }
}

// App log messages (Warning and up) to the file system, so they show in the portal's Log stream. Kept 3 days.
resource webAppLogs 'Microsoft.Web/sites/config@2023-12-01' = {
  parent: webApp
  name: 'logs'
  properties: {
    applicationLogs: {
      fileSystem: { level: 'Warning' }
    }
    httpLogs: {
      fileSystem: { enabled: false, retentionInDays: 3, retentionInMb: 35 }
    }
    detailedErrorMessages: { enabled: false }
    failedRequestsTracing: { enabled: false }
  }
}

// ---------- Budget alert ----------

resource budget 'Microsoft.Consumption/budgets@2023-11-01' = if (!empty(budgetContactEmail)) {
  name: 'budget-app-${environmentName}'
  properties: {
    category: 'Cost'
    amount: monthlyBudget
    timeGrain: 'Monthly'
    timePeriod: { startDate: budgetStartDate }
    notifications: {
      actual80: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 80
        thresholdType: 'Actual'
        contactEmails: [budgetContactEmail]
      }
      forecast100: {
        enabled: true
        operator: 'GreaterThan'
        threshold: 100
        thresholdType: 'Forecasted'
        contactEmails: [budgetContactEmail]
      }
    }
  }
}

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output sqlServerName string = sqlServer.name
output sqlServerHost string = sqlServer.properties.fullyQualifiedDomainName
output databaseName string = database.name
