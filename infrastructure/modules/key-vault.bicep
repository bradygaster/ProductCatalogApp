@description('Name of the Key Vault')
param keyVaultName string

@description('Location for the Key Vault')
param location string = resourceGroup().location

@description('Tags to apply to the resource')
param tags object = {}

@description('Azure AD Tenant ID')
param tenantId string = tenant().tenantId

@description('SKU for the Key Vault')
@allowed([
  'standard'
  'premium'
])
param sku string = 'standard'

@description('Enable soft delete')
param enableSoftDelete bool = true

@description('Soft delete retention in days')
@minValue(7)
@maxValue(90)
param softDeleteRetentionInDays int = 90

@description('Enable RBAC authorization')
param enableRbacAuthorization bool = true

@description('Enable public network access')
param publicNetworkAccess string = 'Enabled'

@description('Object IDs to grant access to Key Vault (for managed identities)')
param accessPolicies array = []

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    tenantId: tenantId
    sku: {
      family: 'A'
      name: sku
    }
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: softDeleteRetentionInDays
    enableRbacAuthorization: enableRbacAuthorization
    publicNetworkAccess: publicNetworkAccess
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
    accessPolicies: [for policy in accessPolicies: {
      tenantId: tenantId
      objectId: policy.objectId
      permissions: policy.permissions
    }]
  }
}

@description('Key Vault ID')
output keyVaultId string = keyVault.id

@description('Key Vault name')
output keyVaultName string = keyVault.name

@description('Key Vault URI')
output keyVaultUri string = keyVault.properties.vaultUri
