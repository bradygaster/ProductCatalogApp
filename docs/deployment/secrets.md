# GitHub Secrets Configuration

This document describes the GitHub secrets required for the CI/CD pipeline to function properly.

## Required Secrets

### AZURE_CREDENTIALS

Azure Service Principal credentials in JSON format.

**Format:**
```json
{
  "clientId": "<client-id>",
  "clientSecret": "<client-secret>",
  "subscriptionId": "<subscription-id>",
  "tenantId": "<tenant-id>"
}
```

**How to create:**

1. Create a service principal with contributor access:
   ```bash
   az ad sp create-for-rbac \
     --name "ProductCatalogApp-GitHub-Actions" \
     --role contributor \
     --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} \
     --json-auth
   ```

2. Copy the JSON output and save it as the `AZURE_CREDENTIALS` secret in GitHub.

### AZURE_SUBSCRIPTION_ID

Your Azure subscription ID.

**Format:** `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`

**How to find:**
```bash
az account show --query id --output tsv
```

### AZURE_RESOURCE_GROUP

The name of the Azure Resource Group where the application resources are deployed.

**Format:** `productcatalog-rg`

**How to find:**
```bash
az group list --query "[].name" --output table
```

## Setting Up Secrets in GitHub

### Repository Secrets

1. Navigate to your repository on GitHub
2. Go to **Settings** > **Secrets and variables** > **Actions**
3. Click **New repository secret**
4. Add each secret with its name and value
5. Click **Add secret**

### Environment Secrets

For environment-specific secrets (dev, staging, production):

1. Navigate to your repository on GitHub
2. Go to **Settings** > **Environments**
3. Select or create an environment (dev, staging, production)
4. Add environment-specific secrets if needed
5. Configure protection rules:
   - **Staging:** Require at least 1 reviewer
   - **Production:** Require at least 2 reviewers, enable wait timer (15 minutes)

## Optional Secrets

### Database Connection Strings

If your application requires database connections, add these as environment variables:

- `DB_CONNECTION_STRING_DEV`
- `DB_CONNECTION_STRING_STAGING`
- `DB_CONNECTION_STRING_PROD`

### Application Insights

For monitoring and telemetry:

- `APPINSIGHTS_INSTRUMENTATIONKEY`

### Custom Application Settings

Any application-specific secrets should be added following the naming convention:

- `APP_SETTING_{NAME}_{ENVIRONMENT}`

## Security Best Practices

1. **Never commit secrets** to the repository
2. **Rotate credentials** regularly (every 90 days recommended)
3. **Use least privilege** when creating service principals
4. **Audit secret access** regularly through GitHub audit logs
5. **Use environment secrets** for environment-specific values
6. **Enable secret scanning** in repository settings
7. **Review and revoke** unused service principals

## Verifying Configuration

After setting up secrets, verify the configuration:

1. Go to **Actions** tab in your repository
2. Manually trigger the CD workflow with `workflow_dispatch`
3. Select the `dev` environment
4. Monitor the deployment logs for any authentication or configuration errors

## Troubleshooting

### Authentication Failures

If you encounter authentication errors:

1. Verify the service principal credentials are correct
2. Check that the service principal has the necessary permissions
3. Ensure the subscription ID and resource group name are accurate
4. Verify the service principal hasn't expired

### Missing Secrets

If a workflow fails due to missing secrets:

1. Check the workflow logs for the specific secret name
2. Verify the secret is configured at the correct level (repository or environment)
3. Ensure the secret name matches exactly (case-sensitive)

## Support

For issues with secrets configuration, contact your DevOps team or create an issue in the repository.
