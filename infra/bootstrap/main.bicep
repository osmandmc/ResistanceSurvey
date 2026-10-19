// One-time bootstrap (R1): the test environment's resource group, and permission for this repository's
// GitHub Actions to deploy into it. Run once by an Owner of the subscription, e.g. from Azure Cloud Shell:
//   az deployment sub create --location swedencentral --template-file infra/bootstrap/main.bicep
// Safe to re-run. The resources themselves come from infra/main.bicep via the infra-test workflow.
//
// Reuses the GitHub deploy identity created by the pipeline repository's bootstrap (id-github-deploy):
// adds a trust entry for this repository and gives it Contributor on the new resource group only.

targetScope = 'subscription'

param location string = 'swedencentral'
param environmentName string = 'test'
param resourceGroupName string = 'rg-direnistakip-app-${environmentName}'

@description('Resource group and name of the existing GitHub deploy identity (from the pipeline bootstrap).')
param identityResourceGroupName string = 'rg-direnistakip-prod'
param identityName string = 'id-github-deploy'

// GitHub's immutable OIDC subject format: owner@ownerId/repo@repoId (IDs are public, not secrets).
// The exact value appears in an AADSTS700213 login error if it ever differs.
@description('GitHub repository allowed to sign in, as it appears in the OIDC subject claim.')
param githubRepo string = 'osmandmc@20256505/ResistanceSurvey@145420398'

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: {
    project: 'direnistakip-app'
    environment: environmentName
  }
}

module trust 'trust.bicep' = {
  scope: resourceGroup(identityResourceGroupName)
  name: 'app-${environmentName}-github-trust'
  params: {
    identityName: identityName
    githubRepo: githubRepo
    githubEnvironment: environmentName
  }
}

module access 'access.bicep' = {
  scope: rg
  name: 'app-${environmentName}-deploy-access'
  params: {
    principalId: trust.outputs.principalId
  }
}

// Values for the app repository's GitHub variables.
output AZURE_CLIENT_ID string = trust.outputs.clientId
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_SUBSCRIPTION_ID string = subscription().subscriptionId
output AZURE_RESOURCE_GROUP string = rg.name
