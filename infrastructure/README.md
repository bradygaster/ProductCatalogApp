# Azure Infrastructure for Product Catalog App

This directory contains Azure Bicep templates for deploying the infrastructure required by the Product Catalog application.

## Service Bus Resources

### service-bus.bicep

Creates the following Azure resources:
- **Service Bus Namespace**: A Standard tier Service Bus namespace
- **Service Bus Queue**: A queue named `productcatalogorders` for order processing
- **Authorization Rules**: Separate Send and Listen policies for security

### Deployment

#### Prerequisites
- Azure CLI installed
- Azure subscription
- Resource group created

#### Deploy using Azure CLI

```bash
# Login to Azure
az login

# Create a resource group (if not exists)
az group create --name rg-productcatalog --location eastus

# Deploy the Service Bus infrastructure
az deployment group create \
  --resource-group rg-productcatalog \
  --template-file infrastructure/service-bus.bicep \
  --parameters serviceBusSku=Standard

# Get the connection string from outputs
az deployment group show \
  --resource-group rg-productcatalog \
  --name service-bus \
  --query properties.outputs.serviceBusConnectionString.value \
  --output tsv
```

#### Deploy using Azure Portal
1. Navigate to Azure Portal
2. Go to "Deploy a custom template"
3. Select "Build your own template in the editor"
4. Copy and paste the content of `service-bus.bicep`
5. Fill in the parameters and deploy

### Configuration

After deployment, update the application configuration:

1. **Get the connection string** from the deployment outputs
2. **Store in Azure Key Vault** (recommended for production):
   ```bash
   az keyvault secret set \
     --vault-name your-keyvault-name \
     --name ServiceBusConnectionString \
     --value "your-connection-string"
   ```
3. **Update Web.config** with the connection string or Key Vault reference

### Parameters

| Parameter | Description | Default | Allowed Values |
|-----------|-------------|---------|----------------|
| `serviceBusNamespaceName` | Name of the Service Bus namespace | `sb-productcatalog-{uniqueString}` | - |
| `queueName` | Name of the queue | `productcatalogorders` | - |
| `location` | Azure region | Resource group location | All Azure regions |
| `serviceBusSku` | Service Bus tier | `Standard` | Basic, Standard, Premium |

### Service Bus Tiers

- **Basic**: Basic messaging with queues, $0.05 per million operations
- **Standard**: Includes topics/subscriptions, auto-scaling, $10/month base
- **Premium**: Dedicated resources, VNet integration, high throughput

### Queue Configuration

The queue is configured with:
- **Lock Duration**: 5 minutes
- **Max Size**: 1 GB
- **TTL**: 14 days
- **Max Delivery Count**: 10
- **Dead Letter Queue**: Enabled

### Security

- Minimum TLS version: 1.2
- Separate authorization rules for Send and Listen operations
- Connection strings should be stored in Azure Key Vault
- Consider using Managed Identity for authentication in production

### Cost Estimation

**Standard Tier**:
- Base: ~$10/month
- Operations: $0.05 per million operations
- Messages: Free up to certain limits

**Typical small application**: $10-20/month

### Monitoring

Enable diagnostics and monitoring:
```bash
az monitor diagnostic-settings create \
  --resource /subscriptions/{subscription-id}/resourceGroups/{rg}/providers/Microsoft.ServiceBus/namespaces/{namespace} \
  --name servicebus-diagnostics \
  --logs '[{"category": "OperationalLogs", "enabled": true}]' \
  --metrics '[{"category": "AllMetrics", "enabled": true}]' \
  --workspace /subscriptions/{subscription-id}/resourceGroups/{rg}/providers/Microsoft.OperationalInsights/workspaces/{workspace}
```

### Cleanup

To delete all resources:
```bash
az group delete --name rg-productcatalog --yes --no-wait
```
