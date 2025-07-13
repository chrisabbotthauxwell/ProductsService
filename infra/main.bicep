param location string = resourceGroup().location
param environmentName string = 'aca-env'
param containerAppName string = 'productsservice'
param containerRegistryName string = 'cr${uniqueString(resourceGroup().id)}'
param logAnalyticsName string = 'log-${uniqueString(resourceGroup().id)}'
param appInsightsName string = 'appi-${uniqueString(resourceGroup().id)}'
param portalDashboardName string = 'dash-${uniqueString(resourceGroup().id)}'
param daprComponentName string = 'pubsub'
param serviceBusConnectionStringSecretName string = 'servicebus-connection'
param serviceBusNamespaceName string = 'sb-${uniqueString(resourceGroup().id)}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: containerRegistryName
  location: location
  sku: {
    name: 'Basic'
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource portalDashboard 'Microsoft.Portal/dashboards@2020-09-01-preview' = {
  name: portalDashboardName
  location: location
  properties: {
    lenses: []
    metadata: {
      model: {
        timeRange: {
          value: {
            relative: {
              duration: 24
              timeUnit: 'Hour'
            }
          }
        }
      }
    }
  }
}

resource acaEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: environmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${containerAppName}-identity'
  location: location
}

resource acrPullRoleAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid(resourceGroup().id, containerRegistryName, containerAppName, 'acrpull')
  scope: containerRegistry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d') // AcrPull
    principalId: containerAppIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

// Reference the existing RootManageSharedAccessKey authorization rule
resource serviceBusAuthRule 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2022-10-01-preview' existing = {
  parent: serviceBusNamespace
  name: 'RootManageSharedAccessKey'
}

var serviceBusConnectionString = serviceBusAuthRule.listKeys().primaryConnectionString

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
  tags: {
    'azd-service-name': containerAppName
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerAppIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: acaEnv.id
    configuration: {
      secrets: [
        {
          name: serviceBusConnectionStringSecretName
          value: serviceBusConnectionString
        }
      ]
      registries: [
        {
          server: containerRegistry.properties.loginServer
          identity: containerAppIdentity.id
        }
      ]
      dapr: {
        enabled: true
        appId: containerAppName
        appPort: 8080
      }
    }
    template: {
      containers: [
        {
          image: '${containerRegistry.properties.loginServer}/${containerAppName}:latest'
          //image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
          name: containerAppName
          resources: {
            cpu: json('0.5')
            memory: '1.0Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
  dependsOn: [
    containerAppIdentity
    acrPullRoleAssignment
  ]
}


resource daprComponent 'Microsoft.App/managedEnvironments/daprComponents@2025-02-02-preview' = {
  name: daprComponentName
  parent: acaEnv
  properties: {
    componentType: 'pubsub.azure.servicebus'
    version: 'v1'
    metadata: [
      {
        name: 'connectionString'
        value: serviceBusConnectionString
      }
    ]
    scopes: [containerAppName]
  }
  dependsOn: [
    containerApp
  ]
}

// Pub/Sub topics from README.md
var pubsubTopics = [
  'stock-available'
  'order-placed'
  'order-fulfilled'
  'order-backordered'
]

// Create Service Bus topics for Dapr pub/sub
resource serviceBusTopics 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = [for topicName in pubsubTopics: {
  parent: serviceBusNamespace
  name: topicName
  properties: {
    enablePartitioning: true
  }
}]

// Create Service Bus subscriptions for each topic (one per app, e.g. productsservice)
resource serviceBusSubscriptions 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = [for (topicName, i) in pubsubTopics: {
  parent: serviceBusTopics[i]
  name: '${containerAppName}-sub'
  properties: {
    maxDeliveryCount: 10
    lockDuration: 'PT5M'
  }
}]
