@description('The name of the Container Apps environment')
param containerAppsEnvironmentName string

@description('The Service Bus connection string')
@secure()
param serviceBusConnectionString string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppsEnvironmentName
}

resource daprPubSubComponent 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  parent: containerAppsEnvironment
  name: 'pubsub'
  properties: {
    componentType: 'pubsub.azure.servicebus.topics'
    version: 'v1'
    metadata: [
      {
        name: 'connectionString'
        value: serviceBusConnectionString
      }
    ]
    scopes: [
      'productsservice'
    ]
  }
}

output name string = daprPubSubComponent.name
