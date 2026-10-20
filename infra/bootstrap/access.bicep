// Contributor on the test environment's resource group only. The app's resources assign no roles,
// so the deploy identity gets no role-assignment rights here.

param principalId string

var contributor = 'b24988ac-6180-42a0-ab88-20f7382dd24c'

resource contributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(resourceGroup().id, principalId, contributor)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', contributor)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}
