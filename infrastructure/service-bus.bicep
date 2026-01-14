// Azure Service Bus Namespace and Queue for Product Catalog App
@description('The name of the Service Bus namespace')
param serviceBusNamespaceName string = 'sb-productcatalog-${uniqueString(resourceGroup().id)}'

@description('The name of the Service Bus queue')
param queueName string = 'productcatalogorders'

@description('Location for all resources')
param location string = resourceGroup().location

@description('The messaging tier for Service Bus namespace')
@allowed([
  'Basic'
  'Standard'
  'Premium'
])
param serviceBusSku string = 'Standard'

@description('Tags to apply to all resources')
param tags object = {
  environment: 'production'
  application: 'ProductCatalog'
  modernization: 'transform-msmq-to-servicebus'
}

// Service Bus Namespace
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusNamespaceName
  location: location
  tags: tags
  sku: {
    name: serviceBusSku
    tier: serviceBusSku
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
  }
}

// Service Bus Queue
resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    lockDuration: 'PT5M'
    maxSizeInMegabytes: 1024
    requiresDuplicateDetection: false
    requiresSession: false
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
    maxDeliveryCount: 10
    enableBatchedOperations: true
    enablePartitioning: false
    enableExpress: false
  }
}

// Authorization Rule for Send permissions
resource sendAuthRule 'Microsoft.ServiceBus/namespaces/queues/authorizationRules@2022-10-01-preview' = {
  parent: serviceBusQueue
  name: 'SendPolicy'
  properties: {
    rights: [
      'Send'
    ]
  }
}

// Authorization Rule for Listen permissions
resource listenAuthRule 'Microsoft.ServiceBus/namespaces/queues/authorizationRules@2022-10-01-preview' = {
  parent: serviceBusQueue
  name: 'ListenPolicy'
  properties: {
    rights: [
      'Listen'
    ]
  }
}

// Outputs
@description('The name of the Service Bus namespace')
output serviceBusNamespaceName string = serviceBusNamespace.name

@description('The name of the Service Bus queue')
output queueName string = serviceBusQueue.name

@description('The connection string for the Service Bus namespace')
output serviceBusConnectionString string = listKeys(resourceId('Microsoft.ServiceBus/namespaces/authorizationRules', serviceBusNamespace.name, 'RootManageSharedAccessKey'), serviceBusNamespace.apiVersion).primaryConnectionString

@description('The endpoint for the Service Bus namespace')
output serviceBusEndpoint string = serviceBusNamespace.properties.serviceBusEndpoint

@description('The connection string for Send policy')
output sendConnectionString string = listKeys(sendAuthRule.id, sendAuthRule.apiVersion).primaryConnectionString

@description('The connection string for Listen policy')
output listenConnectionString string = listKeys(listenAuthRule.id, listenAuthRule.apiVersion).primaryConnectionString
