@description('Name of the App Service')
param appServiceName string

@description('Location for the App Service')
param location string = resourceGroup().location

@description('App Service Plan ID')
param appServicePlanId string

@description('Application Insights Connection String')
param appInsightsConnectionString string = ''

@description('Application Insights Instrumentation Key')
param appInsightsInstrumentationKey string = ''

@description('Storage Account Connection String for queue operations')
param storageConnectionString string = ''

@description('Key Vault URI for secrets')
param keyVaultUri string = ''

@description('Tags to apply to the resource')
param tags object = {}

@description('Enable managed identity')
param enableManagedIdentity bool = true

@description('.NET Framework version')
param netFrameworkVersion string = 'v4.8'

@description('Always On setting')
param alwaysOn bool = true

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceName
  location: location
  tags: tags
  kind: 'app'
  identity: enableManagedIdentity ? {
    type: 'SystemAssigned'
  } : null
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: netFrameworkVersion
      alwaysOn: alwaysOn
      minTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      appSettings: [
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsightsConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'recommended'
        }
        {
          name: 'StorageConnectionString'
          value: storageConnectionString
        }
        {
          name: 'KeyVaultUri'
          value: keyVaultUri
        }
      ]
      connectionStrings: []
    }
  }
}

@description('App Service default hostname')
output defaultHostName string = appService.properties.defaultHostName

@description('App Service name')
output appServiceName string = appService.name

@description('App Service principal ID (managed identity)')
output principalId string = enableManagedIdentity ? appService.identity.principalId : ''

@description('App Service resource ID')
output appServiceId string = appService.id
