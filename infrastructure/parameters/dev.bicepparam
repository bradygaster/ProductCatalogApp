using './main.bicep'

// Development environment parameters
param environmentName = 'dev'
param resourceBaseName = 'productcatalog'
param location = 'eastus'

// Use Free/Basic tier for development to minimize costs
param appServicePlanSku = 'B1'
param storageAccountSku = 'Standard_LRS'
param keyVaultSku = 'standard'

// Shorter retention for development
param logAnalyticsRetentionInDays = 30

// Enable RBAC for Key Vault (recommended)
param enableKeyVaultRbacAuthorization = true

// Development tags
param tags = {
  environment: 'dev'
  application: 'ProductCatalog'
  managedBy: 'bicep'
  costCenter: 'development'
}
