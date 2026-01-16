@description('Name of the Application Insights instance')
param appInsightsName string

@description('Location for Application Insights')
param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object = {}

@description('Log Analytics Workspace ID')
param workspaceId string

@description('Application type')
@allowed([
  'web'
  'other'
])
param applicationType string = 'web'

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: applicationType
  properties: {
    Application_Type: applicationType
    WorkspaceResourceId: workspaceId
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('Application Insights Instrumentation Key')
output instrumentationKey string = appInsights.properties.InstrumentationKey

@description('Application Insights Connection String')
output connectionString string = appInsights.properties.ConnectionString

@description('Application Insights ID')
output appInsightsId string = appInsights.id

@description('Application Insights name')
output appInsightsName string = appInsights.name
