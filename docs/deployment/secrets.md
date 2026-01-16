# GitHub Secrets Configuration

This document describes the required GitHub secrets for the CI/CD pipeline.

## Required Secrets

The following secrets must be configured in your GitHub repository settings (Settings → Secrets and variables → Actions).

### Azure Credentials

#### AZURE_CREDENTIALS

Azure Service Principal credentials in JSON format for authenticating with Azure.

**How to create:**

```bash
# Create a service principal with contributor role
az ad sp create-for-rbac \
  --name "ProductCatalogApp-GitHub-Actions" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
  --sdk-auth
```

The output should look like:

```json
{
  "clientId": "<GUID>",
  "clientSecret": "<STRING>",
  "subscriptionId": "<GUID>",
  "tenantId": "<GUID>",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```

Copy the entire JSON output and save it as the `AZURE_CREDENTIALS` secret.

#### AZURE_SUBSCRIPTION_ID

Your Azure subscription ID.

**How to find:**

```bash
az account show --query id -o tsv
```

Or find it in the Azure Portal under "Subscriptions".

#### AZURE_RESOURCE_GROUP

The name of the Azure Resource Group where your resources are deployed.

Example: `ProductCatalogApp-RG`

### Azure Web App Names

Configure separate web app names for each environment:

#### AZURE_WEBAPP_NAME_DEV

The name of your Azure Web App for the development environment.

Example: `productcatalog-dev`

#### AZURE_WEBAPP_NAME_STAGING

The name of your Azure Web App for the staging environment.

Example: `productcatalog-staging`

#### AZURE_WEBAPP_NAME_PROD

The name of your Azure Web App for the production environment.

Example: `productcatalog-prod`

## Environment-Specific Configuration

### GitHub Environments

In addition to secrets, configure the following environments in GitHub (Settings → Environments):

1. **dev**
   - No protection rules (auto-deploy)
   - Environment URL: `https://dev-productcatalog.azurewebsites.net`

2. **staging**
   - Required reviewers: 1-2 team members
   - Wait timer: Optional (e.g., 5 minutes)
   - Environment URL: `https://staging-productcatalog.azurewebsites.net`

3. **production**
   - Required reviewers: 2+ senior team members
   - Wait timer: Recommended (e.g., 30 minutes)
   - Deployment branch rule: `main` only
   - Environment URL: `https://productcatalog.azurewebsites.net`

## Setting Up Secrets

### Via GitHub UI

1. Navigate to your repository on GitHub
2. Go to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Enter the secret name and value
5. Click **Add secret**

### Via GitHub CLI

```bash
# Set a secret using GitHub CLI
gh secret set AZURE_CREDENTIALS < azure-credentials.json
gh secret set AZURE_SUBSCRIPTION_ID --body "your-subscription-id"
gh secret set AZURE_RESOURCE_GROUP --body "your-resource-group"
gh secret set AZURE_WEBAPP_NAME_DEV --body "productcatalog-dev"
gh secret set AZURE_WEBAPP_NAME_STAGING --body "productcatalog-staging"
gh secret set AZURE_WEBAPP_NAME_PROD --body "productcatalog-prod"
```

## Security Best Practices

1. **Least Privilege**: Grant the service principal only the permissions it needs
2. **Rotation**: Rotate service principal credentials regularly (every 90 days recommended)
3. **Monitoring**: Enable Azure Activity Log to monitor service principal usage
4. **Scope**: Limit the service principal scope to specific resource groups
5. **Audit**: Regularly audit who has access to GitHub secrets

## Troubleshooting

### Authentication Failures

If you encounter authentication errors:

1. Verify the service principal credentials are correct
2. Check that the service principal has not expired
3. Ensure the service principal has appropriate permissions on the resource group
4. Verify the subscription ID matches your Azure subscription

### Missing Secrets

If workflows fail due to missing secrets:

1. Check that all required secrets are configured
2. Verify secret names match exactly (they are case-sensitive)
3. Ensure secrets are available in the correct scope (repository vs. environment)

## Additional Resources

- [Azure Service Principal Documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal)
- [GitHub Actions Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)
- [Azure Web Apps Deployment](https://docs.microsoft.com/en-us/azure/app-service/deploy-github-actions)
