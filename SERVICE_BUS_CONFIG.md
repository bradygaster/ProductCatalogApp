# Azure Service Bus Configuration Guide

## Quick Start

### 1. Deploy Infrastructure

```bash
# Login to Azure
az login

# Create resource group
az group create --name rg-productcatalog --location eastus

# Deploy Service Bus
az deployment group create \
  --resource-group rg-productcatalog \
  --template-file infrastructure/service-bus.bicep
  
# Get connection string
az deployment group show \
  --resource-group rg-productcatalog \
  --name service-bus \
  --query properties.outputs.serviceBusConnectionString.value \
  --output tsv
```

### 2. Configure Application

Update `Web.config` with your connection string:

```xml
<appSettings>
  <add key="ServiceBus:ConnectionString" value="Endpoint=sb://YOUR-NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR-KEY" />
  <add key="ServiceBus:QueueName" value="productcatalogorders" />
</appSettings>
```

### 3. Production Setup (Recommended)

#### Option A: Azure Key Vault

```bash
# Create Key Vault
az keyvault create \
  --name kv-productcatalog \
  --resource-group rg-productcatalog \
  --location eastus

# Store connection string
az keyvault secret set \
  --vault-name kv-productcatalog \
  --name ServiceBusConnectionString \
  --value "YOUR-CONNECTION-STRING"

# Grant app access
az keyvault set-policy \
  --name kv-productcatalog \
  --object-id YOUR-APP-OBJECT-ID \
  --secret-permissions get list
```

Then use Key Vault reference in Web.config:
```xml
<add key="ServiceBus:ConnectionString" value="@Microsoft.KeyVault(SecretUri=https://kv-productcatalog.vault.azure.net/secrets/ServiceBusConnectionString/)" />
```

#### Option B: Managed Identity (Best Practice)

Enable Managed Identity on your App Service:
```bash
az webapp identity assign \
  --name your-webapp \
  --resource-group rg-productcatalog
```

Grant access to Service Bus:
```bash
az role assignment create \
  --assignee YOUR-MANAGED-IDENTITY-ID \
  --role "Azure Service Bus Data Sender" \
  --scope /subscriptions/YOUR-SUB/resourceGroups/rg-productcatalog/providers/Microsoft.ServiceBus/namespaces/YOUR-NAMESPACE
```

Update code to use Managed Identity:
```csharp
// In OrderQueueService.cs
var credential = new DefaultAzureCredential();
_client = new ServiceBusClient("YOUR-NAMESPACE.servicebus.windows.net", credential);
```

## Configuration Options

### Development
- Use connection string from Azure Portal
- Store in Web.config locally (don't commit!)
- Use local Service Bus emulator if available

### Staging
- Store connection string in Azure Key Vault
- Use Key Vault references in App Configuration
- Enable diagnostic logging

### Production
- Use Managed Identity (passwordless)
- Store secrets in Azure Key Vault
- Enable all diagnostics and monitoring
- Set up alerts for queue depth and errors

## Connection String Format

```
Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=[keyname];SharedAccessKey=[key]
```

### Components
- **Endpoint**: Service Bus namespace URL
- **SharedAccessKeyName**: Name of the access policy (e.g., RootManageSharedAccessKey)
- **SharedAccessKey**: The access key from Azure Portal

## Troubleshooting

### Connection Issues
1. Verify connection string is correct
2. Check firewall rules (Service Bus allows all by default)
3. Verify queue name matches exactly
4. Check access policy has Send/Listen permissions

### Message Issues
1. Check message size (max 256KB for Standard, 1MB for Premium)
2. Verify JSON serialization is working
3. Check dead letter queue for failed messages
4. Review ServiceBus diagnostics logs

## Security Best Practices

✅ **DO:**
- Use Managed Identity in production
- Store connection strings in Key Vault
- Use separate Send and Listen policies
- Enable diagnostic logging
- Rotate keys regularly
- Use TLS 1.2+

❌ **DON'T:**
- Commit connection strings to source control
- Use RootManageSharedAccessKey in production code
- Disable encryption
- Share connection strings between environments
- Log connection strings

## Cost Optimization

- Use **Standard tier** for most applications (~$10/month)
- Use **Basic tier** for development/test (~$0.05/million ops)
- Use **Premium tier** only if you need:
  - Dedicated resources
  - VNet integration
  - Large message sizes (>1MB)
  - Geo-disaster recovery

## Monitoring Queries

### Azure Portal Metrics
- Active Messages
- Incoming Messages/sec
- Outgoing Messages/sec
- Dead Letter Messages
- Server Errors
- User Errors

### Log Analytics Queries

```kusto
// Failed message processing
ServiceBusOperationalLogs
| where Status == "Failed"
| summarize count() by bin(TimeGenerated, 5m), OperationName
```

## Support

- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Troubleshooting Guide](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-messaging-exceptions)
- [Best Practices](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-performance-improvements)
