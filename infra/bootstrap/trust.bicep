// Lets this repository's GitHub deploy jobs (environment "test") sign in as the existing deploy identity.

param identityName string
param githubRepo string
param githubEnvironment string

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: identityName
}

resource federated 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: identity
  name: 'github-app-env-${githubEnvironment}'
  properties: {
    issuer: 'https://token.actions.githubusercontent.com'
    subject: 'repo:${githubRepo}:environment:${githubEnvironment}'
    audiences: ['api://AzureADTokenExchange']
  }
}

output principalId string = identity.properties.principalId
output clientId string = identity.properties.clientId
