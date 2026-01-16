using '../main.bicep'

// Production environment parameters
param environmentName = 'prod'
param resourceBaseName = 'productcatalog'
param location = 'eastus'

// Use Premium tier for production with better performance and SLA
param appServicePlanSku = 'P1v3'
param storageAccountSku = 'Standard_GRS'
param keyVaultSku = 'premium'

// Longer retention for production (compliance/audit requirements)
param logAnalyticsRetentionInDays = 90

// Enable RBAC for Key Vault (recommended for production)
param enableKeyVaultRbacAuthorization = true

// Production tags
param tags = {
  environment: 'prod'
  application: 'ProductCatalog'
  managedBy: 'bicep'
  costCenter: 'production'
  criticality: 'high'
}
