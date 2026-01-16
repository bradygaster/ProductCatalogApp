@description('Name of the Storage Account')
param storageAccountName string

@description('Location for the Storage Account')
param location string = resourceGroup().location

@description('Storage Account SKU')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
  'Premium_ZRS'
])
param sku string = 'Standard_LRS'

@description('Tags to apply to the resource')
param tags object = {}

@description('Enable queue service')
param enableQueueService bool = true

@description('Default queue name for orders')
param defaultQueueName string = 'orders'

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2022-09-01' = if (enableQueueService) {
  parent: storageAccount
  name: 'default'
  properties: {}
}

resource orderQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2022-09-01' = if (enableQueueService) {
  parent: queueService
  name: defaultQueueName
  properties: {
    metadata: {}
  }
}

@description('Storage Account ID')
output storageAccountId string = storageAccount.id

@description('Storage Account name')
output storageAccountName string = storageAccount.name

@description('Storage Account primary connection string')
@secure()
output connectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

@description('Storage Account primary key')
@secure()
output primaryKey string = storageAccount.listKeys().keys[0].value
