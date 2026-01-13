# GitHub Secrets Configuration

This document describes the required GitHub secrets for the CI/CD pipeline.

## Required Secrets

### Azure Authentication

#### AZURE_CREDENTIALS
**Required for:** CD workflow  
**Description:** Azure service principal credentials for deployment authentication  
**Format:** JSON object

```json
{
  "clientId": "<service-principal-client-id>",
  "clientSecret": "<service-principal-secret>",
  "subscriptionId": "<azure-subscription-id>",
  "tenantId": "<azure-tenant-id>"
}
```

**How to create:**
1. Create an Azure service principal:
   ```bash
   az ad sp create-for-rbac --name "ProductCatalogApp-GitHub-Actions" \
     --role contributor \
     --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
     --sdk-auth
   ```
2. Copy the JSON output
3. Add to GitHub: Repository Settings → Secrets and variables → Actions → New repository secret
4. Name: `AZURE_CREDENTIALS`
5. Paste the JSON output as the value

#### AZURE_SUBSCRIPTION_ID
**Required for:** CD workflow  
**Description:** Azure subscription ID where resources are deployed  
**Format:** UUID (e.g., `12345678-1234-1234-1234-123456789012`)

**How to find:**
```bash
az account show --query id --output tsv
```

#### AZURE_RESOURCE_GROUP
**Required for:** CD workflow  
**Description:** Name of the Azure resource group containing the web apps  
**Format:** String (e.g., `rg-productcatalog-prod`)

**How to find:**
```bash
az group list --query "[].name" --output table
```

### Azure Web App Names

#### AZURE_WEBAPP_NAME_DEV
**Required for:** CD workflow (dev deployment)  
**Description:** Name of the Azure Web App for development environment  
**Format:** String (e.g., `webapp-productcatalog-dev`)

#### AZURE_WEBAPP_NAME_STAGING
**Required for:** CD workflow (staging deployment)  
**Description:** Name of the Azure Web App for staging environment  
**Format:** String (e.g., `webapp-productcatalog-staging`)

#### AZURE_WEBAPP_NAME_PROD
**Required for:** CD workflow (production deployment)  
**Description:** Name of the Azure Web App for production environment  
**Format:** String (e.g., `webapp-productcatalog-prod`)

## Setting Up Secrets

### Via GitHub Web Interface
1. Navigate to your repository on GitHub
2. Go to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Enter the secret name and value
5. Click **Add secret**

### Via GitHub CLI
```bash
gh secret set AZURE_CREDENTIALS < credentials.json
gh secret set AZURE_SUBSCRIPTION_ID --body "your-subscription-id"
gh secret set AZURE_RESOURCE_GROUP --body "your-resource-group"
gh secret set AZURE_WEBAPP_NAME_DEV --body "your-webapp-dev"
gh secret set AZURE_WEBAPP_NAME_STAGING --body "your-webapp-staging"
gh secret set AZURE_WEBAPP_NAME_PROD --body "your-webapp-prod"
```

## Environment Configuration

### GitHub Environments
Configure the following environments in GitHub:
- **dev**: No protection rules, auto-deploys on merge to main
- **staging**: Requires manual approval
- **production**: Requires manual approval

**To configure:**
1. Go to **Settings** → **Environments**
2. Click **New environment**
3. Enter environment name (dev, staging, or production)
4. For staging and production:
   - Enable **Required reviewers**
   - Add team members who can approve deployments
5. Click **Save protection rules**

## Security Best Practices

1. **Rotate credentials regularly**: Update service principal secrets every 90 days
2. **Use least privilege**: Grant only necessary permissions to service principals
3. **Monitor secret usage**: Review GitHub Actions logs for unauthorized access attempts
4. **Separate service principals**: Use different service principals for each environment
5. **Enable secret scanning**: GitHub will automatically scan for exposed secrets in commits
6. **Use environment secrets**: For environment-specific values, use environment secrets instead of repository secrets

## Troubleshooting

### Authentication Failures
- Verify service principal has Contributor role on resource group
- Check if service principal credentials have expired
- Ensure subscription ID matches the resource group subscription

### Deployment Failures
- Verify Web App names are correct and exist in Azure
- Check resource group name is accurate
- Ensure Web App is running and not stopped

### Secret Not Found Errors
- Confirm secret names match exactly (case-sensitive)
- Verify secrets are set at repository level, not organization level
- Check if secrets are set in the correct environment

## Additional Resources

- [Azure Service Principal Documentation](https://docs.microsoft.com/azure/active-role-based-access-control/create-service-principal-portal)
- [GitHub Secrets Documentation](https://docs.github.com/actions/security-guides/encrypted-secrets)
- [Azure Web Apps Deployment](https://docs.microsoft.com/azure/app-service/deploy-github-actions)
