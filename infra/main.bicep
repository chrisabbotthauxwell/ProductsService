@minLength(1)
@maxLength(64)
param environmentName string
param location string = resourceGroup().location
param tags object = {}

var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var serviceName = 'productsservice'

// Log Analytics workspace for monitoring
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'log-${resourceToken}'
  location: location
  tags: tags
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

// Application Insights for telemetry
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-${resourceToken}'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// Azure Dashboard for monitoring
resource dashboard 'Microsoft.Portal/dashboards@2020-09-01-preview' = {
  name: 'dash-${resourceToken}'
  location: location
  tags: tags
  properties: {
    lenses: []
  }
}

// Container Registry
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: 'cr${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// Container Apps Environment with Dapr
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: 'cae-${resourceToken}'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    daprAIInstrumentationKey: applicationInsights.properties.InstrumentationKey
    infrastructureResourceGroup: 'rg-${environmentName}-infra'
  }
}

// Service Bus for Dapr pub/sub
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'sb-${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

// Wait for Service Bus to be ready
resource serviceBusAuthRule 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2022-10-01-preview' existing = {
  parent: serviceBusNamespace
  name: 'RootManageSharedAccessKey'
}

// Get the connection string after Service Bus is ready
var serviceBusConnectionString = serviceBusAuthRule.listKeys().primaryConnectionString

// Container App with Dapr component
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: serviceName
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      dapr: {
        enabled: true
        appId: serviceName
        appPort: 8080
      }
      ingress: {
        external: true
        targetPort: 8080
      }
      registries: [
        {
          server: containerRegistry.properties.loginServer
          username: containerRegistry.listCredentials().username
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistry.listCredentials().passwords[0].value
        }
        {
          name: 'servicebus-connection-string'
          value: serviceBusConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: serviceName
          image: '${containerRegistry.properties.loginServer}/${serviceName}:latest'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsights.properties.ConnectionString
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
  dependsOn: [
    serviceBusNamespace
    serviceBusAuthRule
  ]
}

// Dapr Component for Service Bus
resource daprComponent 'Microsoft.App/managedEnvironments/daprComponents@2023-05-01' = {
  name: '${containerAppsEnvironment.name}/pubsub'
  properties: {
    componentType: 'pubsub.azure.servicebus'
    version: 'v1'
    metadata: [
      {
        name: 'connectionString'
        secretRef: 'servicebus-connection-string'
      }
    ]
    scopes: [
      containerApp.name
    ]
  }
  dependsOn: [
    serviceBusNamespace
    containerApp
  ]
}

output CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.properties.loginServer
output CONTAINER_REGISTRY_NAME string = containerRegistry.name
output APPLICATION_INSIGHTS_CONNECTION_STRING string = applicationInsights.properties.ConnectionString
output SERVICE_BUS_ENDPOINT string = serviceBusNamespace.properties.serviceBusEndpoint
output API_URI string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
