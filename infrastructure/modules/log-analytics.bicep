@description('Name of the Log Analytics Workspace')
param workspaceName string

@description('Location for the Log Analytics Workspace')
param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object = {}

@description('Workspace data retention in days')
@minValue(30)
@maxValue(730)
param retentionInDays int = 30

@description('Workspace SKU')
@allowed([
  'PerGB2018'
  'Free'
  'Standalone'
  'PerNode'
  'Standard'
  'Premium'
])
param sku string = 'PerGB2018'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: workspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: sku
    }
    retentionInDays: retentionInDays
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('Log Analytics Workspace ID')
output workspaceId string = logAnalyticsWorkspace.id

@description('Log Analytics Workspace name')
output workspaceName string = logAnalyticsWorkspace.name

@description('Log Analytics Workspace Customer ID')
output customerId string = logAnalyticsWorkspace.properties.customerId
