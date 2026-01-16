# Rollback Procedures

This document describes the procedures for rolling back deployments in case of issues.

## Quick Reference

| Scenario | Method | Time to Complete | Impact |
|----------|--------|------------------|--------|
| Dev environment issue | Re-deploy previous commit | 5-10 minutes | Low |
| Staging environment issue | Slot swap or re-deploy | 10-15 minutes | Medium |
| Production critical issue | Slot swap (recommended) | 2-5 minutes | High |
| Production non-critical issue | Re-deploy previous version | 10-15 minutes | Medium |

## Rollback Methods

### Method 1: Azure Deployment Slot Swap (Fastest - Recommended for Production)

This method swaps the staging slot with the production slot, effectively rolling back to the previous version.

**Prerequisites:**
- Azure Web App configured with deployment slots
- Previous version running in staging slot

**Steps:**

1. **Via Azure Portal:**
   ```
   1. Navigate to Azure Portal
   2. Go to your Web App resource
   3. Select "Deployment slots" from the left menu
   4. Click "Swap" button
   5. Select source: "production", target: "staging"
   6. Review configuration changes
   7. Click "Swap" to execute
   ```

2. **Via Azure CLI:**
   ```bash
   # Login to Azure
   az login
   
   # Swap slots
   az webapp deployment slot swap \
     --resource-group ProductCatalogApp-RG \
     --name productcatalog-prod \
     --slot staging \
     --target-slot production
   ```

3. **Via GitHub Actions:**
   - Trigger the rollback workflow manually
   - The workflow will initiate the slot swap automatically

**Time:** 2-5 minutes  
**Downtime:** < 30 seconds (during swap)

### Method 2: Re-deploy Previous Version

This method re-deploys a previous version of the application from GitHub.

**Steps:**

1. **Identify the last known good version:**
   ```bash
   # List recent production tags
   git tag -l "production-*" --sort=-version:refname | head -5
   
   # Or list recent commits
   git log --oneline -10
   ```

2. **Via GitHub Actions (Recommended):**
   ```
   1. Go to Actions tab in GitHub
   2. Select "CD" workflow
   3. Click "Run workflow"
   4. Select the branch/tag with the working version
   5. Choose the environment (dev/staging/production)
   6. Click "Run workflow"
   ```

3. **Via Azure CLI:**
   ```bash
   # Checkout the previous version
   git checkout <commit-hash-or-tag>
   
   # Create deployment package
   # (Follow build steps from CD workflow)
   
   # Deploy to Azure
   az webapp deployment source config-zip \
     --resource-group ProductCatalogApp-RG \
     --name productcatalog-prod \
     --src deployment-package.zip
   ```

**Time:** 10-15 minutes  
**Downtime:** 2-5 minutes (during deployment)

### Method 3: Restore from Backup

This method restores the application from an Azure backup.

**Prerequisites:**
- Azure Web App backups configured
- Recent backup available

**Steps:**

1. **Via Azure Portal:**
   ```
   1. Navigate to Azure Portal
   2. Go to your Web App resource
   3. Select "Backups" from the left menu
   4. Select the backup to restore
   5. Click "Restore"
   6. Choose restore options (overwrite or new app)
   7. Confirm and restore
   ```

2. **Via Azure CLI:**
   ```bash
   # List available backups
   az webapp config backup list \
     --resource-group ProductCatalogApp-RG \
     --webapp-name productcatalog-prod
   
   # Restore from backup
   az webapp config backup restore \
     --resource-group ProductCatalogApp-RG \
     --webapp-name productcatalog-prod \
     --backup-name <backup-name>
   ```

**Time:** 15-30 minutes  
**Downtime:** 5-15 minutes (depending on backup size)

## Rollback Decision Tree

```
┌─────────────────────────────────────┐
│    Issue Detected in Production     │
└──────────────┬──────────────────────┘
               │
               v
┌──────────────────────────────────────┐
│  Is it a critical/breaking issue?    │
└──────┬───────────────────────┬───────┘
       │ Yes                   │ No
       v                       v
┌─────────────────┐   ┌──────────────────────┐
│ Use Method 1:   │   │ Monitor and fix in   │
│ Slot Swap       │   │ next deployment, or  │
│ (Fastest)       │   │ Use Method 2 if      │
└─────────────────┘   │ needed               │
                      └──────────────────────┘
```

## Environment-Specific Procedures

### Development Environment

**When to Rollback:**
- Never required for dev (just fix forward)
- Use for testing rollback procedures

**Procedure:**
1. Re-deploy from main branch
2. Or manually trigger CD workflow with specific commit

### Staging Environment

**When to Rollback:**
- Failed acceptance tests
- Integration issues discovered
- Performance degradation

**Procedure:**
1. Identify last stable deployment
2. Re-run CD workflow with previous version
3. Verify fixes before attempting production deployment again

### Production Environment

**When to Rollback:**
- Critical bugs affecting users
- Security vulnerabilities
- Severe performance degradation
- Data corruption issues

**Procedure:**
1. **Immediate action (< 5 minutes):**
   - Assess severity
   - Notify team via incident channel
   - Execute Method 1 (Slot Swap) if available

2. **Communication:**
   - Update status page
   - Notify stakeholders
   - Document the incident

3. **Post-rollback:**
   - Verify application health
   - Monitor error rates and performance
   - Create incident report

## Pre-Rollback Checklist

Before initiating a rollback:

- [ ] Confirm the issue severity warrants a rollback
- [ ] Identify the last known good version
- [ ] Notify the team (via Slack/Teams/Email)
- [ ] Update status page (if applicable)
- [ ] Document the issue and decision to rollback
- [ ] Prepare communication for stakeholders
- [ ] Have the right team members available

## Post-Rollback Actions

After completing a rollback:

1. **Verify Application Health:**
   - Check application logs
   - Monitor error rates
   - Test critical user flows
   - Verify database integrity

2. **Communication:**
   - Update status page with resolution
   - Send all-clear notification
   - Post incident summary

3. **Root Cause Analysis:**
   - Schedule post-mortem meeting
   - Document what went wrong
   - Identify prevention measures
   - Update deployment procedures if needed

4. **Prevention:**
   - Add tests to catch the issue
   - Improve monitoring/alerting
   - Update deployment checklist
   - Review and improve CI/CD pipeline

## Monitoring After Rollback

Key metrics to monitor after a rollback:

- **Application Health:**
  - HTTP status codes (should be mostly 2xx)
  - Response times (should be within baseline)
  - Error rates (should return to normal)

- **Infrastructure:**
  - CPU usage
  - Memory usage
  - Network traffic

- **Business Metrics:**
  - User sign-ups
  - Transaction completion rates
  - Key feature usage

## Automated Rollback

Consider implementing automated rollback based on:
- Error rate thresholds
- Response time degradation
- Failed health checks
- Synthetic monitoring failures

Example health check configuration:
```yaml
# In CD workflow
- name: Health check with auto-rollback
  shell: pwsh
  run: |
    $maxRetries = 3
    $retryCount = 0
    $success = $false
    
    while ($retryCount -lt $maxRetries -and -not $success) {
      try {
        $response = Invoke-WebRequest -Uri "https://productcatalog.azurewebsites.net/health" -UseBasicParsing
        if ($response.StatusCode -eq 200) {
          $success = $true
          Write-Host "Health check passed"
        }
      } catch {
        $retryCount++
        Write-Warning "Health check failed, attempt $retryCount of $maxRetries"
        Start-Sleep -Seconds 30
      }
    }
    
    if (-not $success) {
      Write-Error "Health check failed after $maxRetries attempts, initiating rollback"
      # Trigger rollback workflow
      exit 1
    }
```

## Emergency Contacts

In case of deployment issues:

- **DevOps Lead:** [Email/Phone]
- **Development Lead:** [Email/Phone]
- **On-Call Engineer:** [Rotation schedule/PagerDuty]
- **Azure Support:** [Support plan details]

## Additional Resources

- [Azure Web App Deployment Slots](https://docs.microsoft.com/en-us/azure/app-service/deploy-staging-slots)
- [Azure Web App Backups](https://docs.microsoft.com/en-us/azure/app-service/manage-backup)
- [GitHub Actions Workflows](https://docs.github.com/en/actions/using-workflows)
- [Incident Response Runbook](./incident-response.md) (if available)
