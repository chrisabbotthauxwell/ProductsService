@description('The name of the Service Bus namespace')
param name string

@description('The location for the Service Bus namespace')
param location string

@description('Tags to apply to the Service Bus namespace')
param tags object = {}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2021-11-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {}
}

// Create topics for your events
var topics = [
  'stock-available'
  'order-placed'
  'order-fulfilled'
  'order-backordered'
]

resource serviceBusTopics 'Microsoft.ServiceBus/namespaces/topics@2021-11-01' = [for topic in topics: {
  parent: serviceBusNamespace
  name: topic
  properties: {
    maxSizeInMegabytes: 1024
  }
}]

resource sendListenRule 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2021-11-01' = {
  parent: serviceBusNamespace
  name: 'DaprRule'
  properties: {
    rights: [
      'Send'
      'Listen'
      'Manage'
    ]
  }
}

output namespaceName string = serviceBusNamespace.name
output connectionString string = listKeys(sendListenRule.id, sendListenRule.apiVersion).primaryConnectionString
output id string = serviceBusNamespace.id
