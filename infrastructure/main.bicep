targetScope = 'resourceGroup'

@description('The environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environmentName string = 'dev'

@description('The Azure region for all resources')
param location string = resourceGroup().location

@description('Base name for all resources')
param resourceBaseName string = 'productcatalog'

@description('App Service Plan SKU')
@allowed([
  'F1'
  'D1'
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P1v2'
  'P2v2'
  'P3v2'
  'P1v3'
  'P2v3'
  'P3v3'
])
param appServicePlanSku string = 'B1'

@description('Storage Account SKU')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
  'Premium_ZRS'
])
param storageAccountSku string = 'Standard_LRS'

@description('Key Vault SKU')
@allowed([
  'standard'
  'premium'
])
param keyVaultSku string = 'standard'

@description('Log Analytics retention in days')
@minValue(30)
@maxValue(730)
param logAnalyticsRetentionInDays int = 30

@description('Enable Key Vault RBAC authorization')
param enableKeyVaultRbacAuthorization bool = true

@description('Tags to apply to all resources')
param tags object = {
  environment: environmentName
  application: 'ProductCatalog'
  managedBy: 'bicep'
}

// Generate unique resource names
var uniqueSuffix = uniqueString(resourceGroup().id, resourceBaseName)
var appServicePlanName = '${resourceBaseName}-plan-${environmentName}'
var appServiceName = '${resourceBaseName}-app-${environmentName}-${uniqueSuffix}'
var storageAccountName = toLower('${resourceBaseName}st${environmentName}${take(uniqueSuffix, 8)}')
var keyVaultName = '${resourceBaseName}-kv-${environmentName}-${take(uniqueSuffix, 4)}'
var appInsightsName = '${resourceBaseName}-ai-${environmentName}'
var logAnalyticsName = '${resourceBaseName}-log-${environmentName}'

// Module: Log Analytics Workspace
module logAnalytics 'modules/log-analytics.bicep' = {
  name: 'deploy-log-analytics'
  params: {
    workspaceName: logAnalyticsName
    location: location
    tags: tags
    retentionInDays: logAnalyticsRetentionInDays
    sku: 'PerGB2018'
  }
}

// Module: Application Insights
module appInsights 'modules/app-insights.bicep' = {
  name: 'deploy-app-insights'
  params: {
    appInsightsName: appInsightsName
    location: location
    tags: tags
    workspaceId: logAnalytics.outputs.workspaceId
    applicationType: 'web'
  }
}

// Module: Storage Account (replaces MSMQ for queue operations)
module storageAccount 'modules/storage-account.bicep' = {
  name: 'deploy-storage-account'
  params: {
    storageAccountName: storageAccountName
    location: location
    tags: tags
    sku: storageAccountSku
    enableQueueService: true
    defaultQueueName: 'productcatalogorders'
  }
}

// Module: App Service Plan
module appServicePlan 'modules/app-service-plan.bicep' = {
  name: 'deploy-app-service-plan'
  params: {
    appServicePlanName: appServicePlanName
    location: location
    tags: tags
    sku: appServicePlanSku
    kind: 'Windows'
  }
}

// Module: Key Vault
module keyVault 'modules/key-vault.bicep' = {
  name: 'deploy-key-vault'
  params: {
    keyVaultName: keyVaultName
    location: location
    tags: tags
    sku: keyVaultSku
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enableRbacAuthorization: enableKeyVaultRbacAuthorization
    publicNetworkAccess: 'Enabled'
    accessPolicies: []
  }
}

// Module: App Service (ProductCatalog web application)
module appService 'modules/app-service.bicep' = {
  name: 'deploy-app-service'
  params: {
    appServiceName: appServiceName
    location: location
    tags: tags
    appServicePlanId: appServicePlan.outputs.appServicePlanId
    appInsightsConnectionString: appInsights.outputs.connectionString
    appInsightsInstrumentationKey: appInsights.outputs.instrumentationKey
    storageConnectionString: storageAccount.outputs.connectionString
    keyVaultUri: keyVault.outputs.keyVaultUri
    enableManagedIdentity: true
    netFrameworkVersion: 'v4.8'
    alwaysOn: appServicePlanSku != 'F1' && appServicePlanSku != 'D1'
  }
}

// Grant App Service managed identity access to Key Vault secrets
resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2022-07-01' = if (!enableKeyVaultRbacAuthorization) {
  name: '${keyVaultName}/add'
  properties: {
    accessPolicies: [
      {
        tenantId: tenant().tenantId
        objectId: appService.outputs.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}

// Outputs
@description('App Service default hostname')
output appServiceUrl string = 'https://${appService.outputs.defaultHostName}'

@description('App Service name')
output appServiceName string = appService.outputs.appServiceName

@description('Application Insights Instrumentation Key')
output appInsightsInstrumentationKey string = appInsights.outputs.instrumentationKey

@description('Storage Account name')
output storageAccountName string = storageAccount.outputs.storageAccountName

@description('Key Vault URI')
output keyVaultUri string = keyVault.outputs.keyVaultUri

@description('Key Vault name')
output keyVaultName string = keyVault.outputs.keyVaultName

@description('App Service Managed Identity Principal ID')
output appServicePrincipalId string = appService.outputs.principalId

@description('Resource Group location')
output resourceGroupLocation string = location

@description('Environment name')
output environment string = environmentName
