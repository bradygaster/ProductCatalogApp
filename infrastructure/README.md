# Product Catalog Application - Infrastructure as Code

This directory contains Bicep templates for deploying the Product Catalog Application to Azure.

## Overview

The infrastructure deploys a complete Azure environment for hosting the Product Catalog .NET Framework 4.8.1 application with the following resources:

### Azure Resources

- **App Service Plan** - Hosts the web application
- **App Service** - Runs the Product Catalog ASP.NET MVC application
- **Storage Account** - Provides Azure Queue Storage (replaces MSMQ)
- **Key Vault** - Stores secrets and connection strings securely
- **Application Insights** - Provides application monitoring and diagnostics
- **Log Analytics Workspace** - Centralized logging and analytics

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Resource Group                        │
│                                                          │
│  ┌────────────────┐      ┌──────────────────┐          │
│  │  App Service   │──────│  App Service     │          │
│  │  Plan          │      │  (Web App)       │          │
│  └────────────────┘      └──────────────────┘          │
│                                 │                        │
│                                 │                        │
│  ┌────────────────┐      ┌──────────────────┐          │
│  │  Storage       │      │  Application     │          │
│  │  Account       │      │  Insights        │          │
│  │  (Queues)      │      └──────────────────┘          │
│  └────────────────┘               │                     │
│         │                          │                     │
│         │                  ┌──────────────────┐         │
│         │                  │  Log Analytics   │         │
│         │                  │  Workspace       │         │
│         │                  └──────────────────┘         │
│         │                                                │
│  ┌────────────────┐                                     │
│  │  Key Vault     │                                     │
│  │  (Secrets)     │                                     │
│  └────────────────┘                                     │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

## Directory Structure

```
infrastructure/
├── main.bicep                      # Main orchestration template
├── modules/                        # Reusable Bicep modules
│   ├── app-service-plan.bicep     # App Service Plan configuration
│   ├── app-service.bicep          # App Service configuration
│   ├── storage-account.bicep      # Storage Account with Queue
│   ├── key-vault.bicep            # Key Vault configuration
│   ├── app-insights.bicep         # Application Insights
│   └── log-analytics.bicep        # Log Analytics Workspace
└── parameters/                     # Environment-specific parameters
    ├── dev.bicepparam             # Development environment
    ├── staging.bicepparam         # Staging environment
    └── prod.bicepparam            # Production environment
```

## Prerequisites

- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed
- Azure subscription with appropriate permissions
- Resource Group created in Azure

## Deployment

### 1. Validate Templates

```bash
# Validate main template
az bicep build --file infrastructure/main.bicep

# Validate all module templates
az bicep build --file infrastructure/modules/app-service-plan.bicep
az bicep build --file infrastructure/modules/app-service.bicep
az bicep build --file infrastructure/modules/storage-account.bicep
az bicep build --file infrastructure/modules/key-vault.bicep
az bicep build --file infrastructure/modules/app-insights.bicep
az bicep build --file infrastructure/modules/log-analytics.bicep
```

### 2. Login to Azure

```bash
az login
az account set --subscription "<your-subscription-id>"
```

### 3. Create Resource Group

```bash
# For development
az group create --name rg-productcatalog-dev --location eastus

# For staging
az group create --name rg-productcatalog-staging --location eastus

# For production
az group create --name rg-productcatalog-prod --location eastus
```

### 4. Run What-If Deployment (Preview Changes)

```bash
# For development
az deployment group what-if \
  --resource-group rg-productcatalog-dev \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/parameters/dev.bicepparam

# For staging
az deployment group what-if \
  --resource-group rg-productcatalog-staging \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/parameters/staging.bicepparam

# For production
az deployment group what-if \
  --resource-group rg-productcatalog-prod \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/parameters/prod.bicepparam
```

### 5. Deploy Infrastructure

```bash
# For development
az deployment group create \
  --resource-group rg-productcatalog-dev \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/parameters/dev.bicepparam \
  --name productcatalog-dev-deployment

# For staging
az deployment group create \
  --resource-group rg-productcatalog-staging \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/parameters/staging.bicepparam \
  --name productcatalog-staging-deployment

# For production
az deployment group create \
  --resource-group rg-productcatalog-prod \
  --template-file infrastructure/main.bicep \
  --parameters infrastructure/parameters/prod.bicepparam \
  --name productcatalog-prod-deployment
```

## Environment Configurations

### Development
- **App Service Plan**: B1 (Basic)
- **Storage**: Standard_LRS (Locally Redundant)
- **Key Vault**: Standard
- **Log Retention**: 30 days
- **Cost**: ~$55/month

### Staging
- **App Service Plan**: S1 (Standard)
- **Storage**: Standard_GRS (Geo-Redundant)
- **Key Vault**: Standard
- **Log Retention**: 60 days
- **Cost**: ~$75/month

### Production
- **App Service Plan**: P1v3 (Premium V3)
- **Storage**: Standard_GRS (Geo-Redundant)
- **Key Vault**: Premium (HSM-backed)
- **Log Retention**: 90 days
- **Cost**: ~$200/month

## Security Features

- **HTTPS Only**: All App Services enforce HTTPS
- **Managed Identity**: App Service uses system-assigned managed identity
- **Key Vault**: Secrets stored securely in Azure Key Vault
- **TLS 1.2**: Minimum TLS version enforced
- **Soft Delete**: Key Vault has soft delete enabled (90 days)
- **RBAC**: Role-Based Access Control for Key Vault
- **Private Storage**: Blob public access disabled

## Post-Deployment Steps

### 1. Configure Key Vault Secrets

```bash
# Get Key Vault name from deployment outputs
KV_NAME=$(az deployment group show \
  --resource-group rg-productcatalog-dev \
  --name productcatalog-dev-deployment \
  --query properties.outputs.keyVaultName.value -o tsv)

# Add secrets (example)
az keyvault secret set --vault-name $KV_NAME --name "DatabaseConnectionString" --value "your-connection-string"
```

### 2. Grant Key Vault Access to App Service

If using RBAC (recommended):

```bash
# Get App Service principal ID
PRINCIPAL_ID=$(az deployment group show \
  --resource-group rg-productcatalog-dev \
  --name productcatalog-dev-deployment \
  --query properties.outputs.appServicePrincipalId.value -o tsv)

# Grant Key Vault Secrets User role
az role assignment create \
  --assignee $PRINCIPAL_ID \
  --role "Key Vault Secrets User" \
  --scope $(az keyvault show --name $KV_NAME --query id -o tsv)
```

### 3. Deploy Application Code

```bash
# Get App Service name
APP_NAME=$(az deployment group show \
  --resource-group rg-productcatalog-dev \
  --name productcatalog-dev-deployment \
  --query properties.outputs.appServiceName.value -o tsv)

# Deploy using Azure CLI
az webapp deployment source config-zip \
  --resource-group rg-productcatalog-dev \
  --name $APP_NAME \
  --src path/to/your/app.zip
```

## Monitoring and Diagnostics

### Application Insights

Application Insights is automatically configured and connected to the App Service. View telemetry in:
- Azure Portal > Application Insights
- Live Metrics Stream for real-time monitoring
- Transaction search for detailed request traces

### Log Analytics

All logs are centralized in Log Analytics Workspace. Query logs using Kusto Query Language (KQL):

```kusto
// Recent errors
AppServiceConsoleLogs
| where TimeGenerated > ago(1h)
| where ResultDescription contains "error"
| order by TimeGenerated desc

// Performance metrics
AppServiceHTTPLogs
| where TimeGenerated > ago(1h)
| summarize avg(TimeTaken), max(TimeTaken) by bin(TimeGenerated, 5m)
```

## Cleanup

To delete all resources:

```bash
# For development
az group delete --name rg-productcatalog-dev --yes --no-wait

# For staging
az group delete --name rg-productcatalog-staging --yes --no-wait

# For production
az group delete --name rg-productcatalog-prod --yes --no-wait
```

## Notes on MSMQ Migration

The original application uses MSMQ (Microsoft Message Queuing) for order processing. In Azure, this is replaced with **Azure Storage Queues**:

- MSMQ queue `.\Private$\ProductCatalogOrders` → Azure Storage Queue `productcatalogorders`
- Connection string provided via App Settings: `StorageConnectionString`
- Application code needs to be updated to use Azure Storage Queue SDK instead of `System.Messaging`

## Troubleshooting

### Bicep Build Errors

```bash
# Check Bicep version
az bicep version

# Upgrade Bicep
az bicep upgrade
```

### Deployment Errors

```bash
# View deployment logs
az deployment group show \
  --resource-group rg-productcatalog-dev \
  --name productcatalog-dev-deployment

# View deployment operations
az deployment operation group list \
  --resource-group rg-productcatalog-dev \
  --name productcatalog-dev-deployment
```

### App Service Issues

```bash
# View App Service logs
az webapp log tail \
  --resource-group rg-productcatalog-dev \
  --name <app-service-name>
```

## Additional Resources

- [Azure Bicep Documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure Storage Queues Documentation](https://docs.microsoft.com/en-us/azure/storage/queues/)
- [Application Insights Documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
