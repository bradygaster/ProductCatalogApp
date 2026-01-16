@description('Name of the App Service Plan')
param appServicePlanName string

@description('Location for the App Service Plan')
param location string = resourceGroup().location

@description('SKU for the App Service Plan')
@allowed([
  'F1'  // Free tier
  'D1'  // Shared tier
  'B1'  // Basic tier
  'B2'
  'B3'
  'S1'  // Standard tier
  'S2'
  'S3'
  'P1v2' // Premium V2
  'P2v2'
  'P3v2'
  'P1v3' // Premium V3
  'P2v3'
  'P3v3'
])
param sku string = 'B1'

@description('Tags to apply to the resource')
param tags object = {}

@description('Kind of App Service Plan (Windows/Linux)')
@allowed([
  'Windows'
  'Linux'
])
param kind string = 'Windows'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  tags: tags
  sku: {
    name: sku
  }
  kind: kind
  properties: {
    reserved: kind == 'Linux'
  }
}

@description('Resource ID of the App Service Plan')
output appServicePlanId string = appServicePlan.id

@description('Name of the App Service Plan')
output appServicePlanName string = appServicePlan.name
