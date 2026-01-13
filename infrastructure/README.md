# Infrastructure as Code - Azure Service Bus

This directory contains Bicep templates for deploying Azure Service Bus resources for the Product Catalog application.

## Files

- `service-bus.bicep` - Main template for Service Bus namespace and queue

## Prerequisites

- Azure CLI installed
- Azure subscription
- Resource group created

## Quick Start

### 1. Login to Azure
```bash
az login
az account set --subscription "your-subscription-id"
```

### 2. Create Resource Group (if not exists)
```bash
az group create \
  --name product-catalog-rg \
  --location eastus
```

### 3. Deploy Service Bus Infrastructure
```bash
az deployment group create \
  --resource-group product-catalog-rg \
  --template-file service-bus.bicep \
  --parameters serviceBusNamespaceName=product-catalog-sb-$(date +%s)
```

Note: Service Bus namespace names must be globally unique. The command above appends a timestamp to ensure uniqueness.

### 4. Get Connection String
```bash
az servicebus namespace authorization-rule keys list \
  --resource-group product-catalog-rg \
  --namespace-name product-catalog-sb-TIMESTAMP \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv
```

Copy this connection string to your application configuration files.

## Parameters

### Required
- `serviceBusNamespaceName` - Name of the Service Bus namespace (must be globally unique)

### Optional
- `location` - Azure region (default: resource group location)
- `queueName` - Name of the queue (default: "product-catalog-orders")
- `skuName` - Service Bus tier: Basic, Standard, or Premium (default: "Standard")

## Example with Parameters

```bash
az deployment group create \
  --resource-group product-catalog-rg \
  --template-file service-bus.bicep \
  --parameters \
    serviceBusNamespaceName=mycompany-catalog-sb \
    queueName=orders \
    skuName=Standard \
    location=westus2
```

## Outputs

The deployment outputs the following values:
- `serviceBusNamespaceId` - Resource ID of the Service Bus namespace
- `serviceBusNamespaceName` - Name of the Service Bus namespace
- `queueName` - Name of the created queue
- `primaryConnectionString` - Primary connection string (use this in your application)

## Queue Configuration

The queue is configured with the following settings:
- **Lock Duration**: 5 minutes
- **Max Size**: 1 GB
- **Message TTL**: 14 days
- **Max Delivery Count**: 10 (messages move to dead letter queue after 10 failed attempts)
- **Dead Lettering**: Enabled for expired messages
- **Batched Operations**: Enabled for better performance

## SKU Comparison

| Feature | Basic | Standard | Premium |
|---------|-------|----------|---------|
| Max message size | 256 KB | 256 KB | 1 MB |
| Topics/Subscriptions | No | Yes | Yes |
| Message TTL | 14 days | 14 days | 90 days |
| Partitioning | No | Yes | Yes |
| Throughput | Low | Medium | High (dedicated) |
| Pricing | Lowest | Medium | Highest |

## Security Best Practices

### 1. Use Managed Identity (Recommended)
Instead of connection strings, use Azure Managed Identity:

```bicep
// Add to your Bicep template
resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  // ...
  identity: {
    type: 'SystemAssigned'
  }
}

// Grant permission to Service Bus
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBusNamespace
  name: guid(serviceBusNamespace.id, webApp.id, 'ServiceBusSender')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39') // Azure Service Bus Data Sender
    principalId: webApp.identity.principalId
  }
}
```

### 2. Store Connection Strings in Key Vault
```bash
# Create Key Vault
az keyvault create \
  --name product-catalog-kv \
  --resource-group product-catalog-rg \
  --location eastus

# Store connection string
az keyvault secret set \
  --vault-name product-catalog-kv \
  --name ServiceBusConnectionString \
  --value "Endpoint=sb://..."
```

### 3. Use Virtual Network Service Endpoints
Add to your Bicep template:
```bicep
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  // ...
  properties: {
    publicNetworkAccess: 'Disabled'
    // Add network rules
  }
}
```

## Monitoring

### Enable Diagnostic Settings
```bash
az monitor diagnostic-settings create \
  --name servicebus-diagnostics \
  --resource /subscriptions/{sub}/resourceGroups/product-catalog-rg/providers/Microsoft.ServiceBus/namespaces/product-catalog-sb \
  --workspace /subscriptions/{sub}/resourceGroups/product-catalog-rg/providers/Microsoft.OperationalInsights/workspaces/my-workspace \
  --logs '[{"category":"OperationalLogs","enabled":true}]' \
  --metrics '[{"category":"AllMetrics","enabled":true}]'
```

### Set Up Alerts
```bash
# Alert for high queue length
az monitor metrics alert create \
  --name high-queue-length \
  --resource-group product-catalog-rg \
  --scopes /subscriptions/{sub}/resourceGroups/product-catalog-rg/providers/Microsoft.ServiceBus/namespaces/product-catalog-sb \
  --condition "avg ActiveMessages > 100" \
  --description "Alert when queue has more than 100 messages"
```

## Clean Up

To delete all resources:
```bash
az group delete --name product-catalog-rg --yes --no-wait
```

## Additional Resources

- [Bicep Documentation](https://docs.microsoft.com/azure/azure-resource-manager/bicep/)
- [Service Bus Bicep Reference](https://docs.microsoft.com/azure/templates/microsoft.servicebus/namespaces)
- [Azure CLI Service Bus Commands](https://docs.microsoft.com/cli/azure/servicebus)
