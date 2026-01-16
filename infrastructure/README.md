# Infrastructure as Code (IaC)

This directory contains Bicep templates for deploying Azure infrastructure resources.

## Files

### service-bus.bicep

Deploys Azure Service Bus infrastructure for the Product Catalog application.

**Resources Created:**
- Service Bus Namespace
- Service Bus Queue (product-orders)
- Key Vault secrets (connection string and queue name)

**Parameters:**
- `location` - Azure region (defaults to resource group location)
- `serviceBusNamespaceName` - Name of Service Bus namespace (auto-generated with unique suffix)
- `queueName` - Name of the queue (default: "product-orders")
- `serviceBusSku` - Pricing tier: Basic, Standard, or Premium (default: Standard)
- `keyVaultName` - Name of existing Key Vault to store secrets (required)

**Usage:**

```bash
# Deploy with default settings
az deployment group create \
  --resource-group rg-productcatalog \
  --template-file service-bus.bicep \
  --parameters keyVaultName=kv-productcatalog-xyz

# Deploy with custom parameters
az deployment group create \
  --resource-group rg-productcatalog \
  --template-file service-bus.bicep \
  --parameters \
    serviceBusNamespaceName=sb-myapp \
    queueName=orders \
    serviceBusSku=Premium \
    keyVaultName=kv-myapp
```

**Outputs:**
- `serviceBusNamespaceId` - Resource ID of the namespace
- `serviceBusNamespaceName` - Name of the namespace
- `serviceBusQueueName` - Name of the queue
- `serviceBusEndpoint` - Service Bus endpoint URL

## Prerequisites

- Azure CLI installed
- Authenticated to Azure (`az login`)
- Resource group created
- Key Vault created (for storing secrets)

## Best Practices

1. **Use separate resource groups** for dev, staging, and production
2. **Enable diagnostic logs** for monitoring and troubleshooting
3. **Use Premium tier** for production workloads requiring high throughput
4. **Configure alerts** for queue depth and dead-letter messages
5. **Use Managed Identity** instead of connection strings when possible
