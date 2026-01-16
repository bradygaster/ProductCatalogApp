using './main.bicep'

// Staging environment parameters
param environmentName = 'staging'
param resourceBaseName = 'productcatalog'
param location = 'eastus'

// Use Standard tier for staging to match production-like environment
param appServicePlanSku = 'S1'
param storageAccountSku = 'Standard_GRS'
param keyVaultSku = 'standard'

// Moderate retention for staging
param logAnalyticsRetentionInDays = 60

// Enable RBAC for Key Vault (recommended)
param enableKeyVaultRbacAuthorization = true

// Staging tags
param tags = {
  environment: 'staging'
  application: 'ProductCatalog'
  managedBy: 'bicep'
  costCenter: 'staging'
}
