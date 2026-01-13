# Rollback Procedures

This document describes the procedures for rolling back deployments in case of issues or failures.

## Overview

The CI/CD pipeline supports multiple rollback strategies depending on the severity and nature of the issue:

1. **Slot Swap Rollback** (Production only - Fastest)
2. **Previous Version Redeployment** (All environments)
3. **Manual Rollback** (Emergency situations)

## Rollback Strategies

### 1. Slot Swap Rollback (Production)

The production deployment uses Azure deployment slots with a blue-green deployment strategy. This allows for instant rollback.

**When to use:**
- Issues discovered immediately after production deployment
- Application crashes or critical bugs in production
- Performance degradation detected

**Steps:**

1. Verify the issue is with the new deployment:
   ```bash
   # Check current production slot
   az webapp show --name productcatalog-prod \
     --resource-group <resource-group> \
     --query "slotSwapStatus"
   ```

2. Swap back to the previous version:
   ```bash
   az webapp deployment slot swap \
     --name productcatalog-prod \
     --resource-group <resource-group> \
     --slot blue \
     --target-slot production
   ```

3. Verify the rollback:
   ```bash
   # Test the production URL
   curl https://productcatalog.azurewebsites.net/health
   ```

**Time to rollback:** ~30 seconds

### 2. Previous Version Redeployment

Redeploy a previous successful build from GitHub Actions artifacts.

**When to use:**
- Issues discovered after slot swap window has passed
- Staging or dev environment issues
- Need to roll back multiple versions

**Steps:**

1. Navigate to **Actions** tab in GitHub repository

2. Find the last successful deployment workflow run before the problematic deployment

3. Locate the workflow run and note the deployment package artifact

4. Trigger a manual deployment:
   - Go to **Actions** > **CD** workflow
   - Click **Run workflow**
   - Select the target environment
   - The workflow will use the latest successful build

5. Alternatively, download and redeploy manually:
   ```bash
   # Download the artifact from the successful run
   gh run download <run-id> --name deployment-package
   
   # Deploy using Azure CLI
   az webapp deployment source config-zip \
     --name productcatalog-<env> \
     --resource-group <resource-group> \
     --src ./deployment-package.zip
   ```

**Time to rollback:** 5-10 minutes

### 3. Git Revert and Redeploy

Revert the problematic commit(s) and trigger a new deployment.

**When to use:**
- Issues identified in code that need to be permanently reverted
- Multiple commits need to be rolled back
- Clean git history is important

**Steps:**

1. Identify the problematic commit(s):
   ```bash
   git log --oneline -10
   ```

2. Create a revert commit:
   ```bash
   # Revert a single commit
   git revert <commit-hash>
   
   # Revert multiple commits
   git revert <oldest-commit>..<newest-commit>
   ```

3. Push to trigger CI/CD:
   ```bash
   git push origin main
   ```

4. Monitor the deployment in GitHub Actions

**Time to rollback:** 10-20 minutes (includes full build and test cycle)

### 4. Manual Emergency Rollback

For critical situations where automated rollback fails.

**When to use:**
- Automated rollback procedures fail
- Infrastructure or platform issues
- Security incidents requiring immediate action

**Steps:**

1. **Stop the application** (if necessary):
   ```bash
   az webapp stop --name productcatalog-<env> \
     --resource-group <resource-group>
   ```

2. **Access Azure Portal:**
   - Navigate to the App Service
   - Go to **Deployment** > **Deployment slots** (for production)
   - Or **Deployment Center** for direct deployments

3. **Rollback options:**
   - Use the **Swap** button to swap slots
   - Or use **Deployment History** to redeploy a previous version
   - Or restore from a **Backup** if configured

4. **Verify and restart:**
   ```bash
   az webapp start --name productcatalog-<env> \
     --resource-group <resource-group>
   ```

5. **Notify the team** via appropriate channels

**Time to rollback:** 5-15 minutes (depending on issue complexity)

## Rollback Decision Matrix

| Scenario | Environment | Recommended Strategy | Estimated Time |
|----------|-------------|---------------------|----------------|
| Critical production bug | Production | Slot Swap Rollback | 30 seconds |
| Failed smoke tests | Staging | Previous Version Redeployment | 5-10 minutes |
| Database migration issue | All | Manual Emergency Rollback + DB restore | 15-30 minutes |
| Performance degradation | Production | Slot Swap Rollback | 30 seconds |
| Security vulnerability | All | Git Revert and Redeploy | 10-20 minutes |
| Configuration issue | Dev/Staging | Previous Version Redeployment | 5-10 minutes |

## Post-Rollback Procedures

After successfully rolling back a deployment:

1. **Create an incident report:**
   - Document the issue that triggered the rollback
   - Record the rollback method used
   - Note any data loss or side effects
   - Estimate impact on users

2. **Investigate root cause:**
   - Review logs and monitoring data
   - Identify what went wrong
   - Determine why the issue wasn't caught in testing

3. **Fix the issue:**
   - Create a hotfix branch if urgent
   - Implement proper fix with tests
   - Ensure fix is validated in dev/staging before production

4. **Update procedures:**
   - Document any new rollback scenarios
   - Update runbooks if needed
   - Share learnings with the team

## Database Rollback Considerations

If the deployment included database migrations:

1. **Check if migration is reversible:**
   ```bash
   # Check migration history
   # This depends on your ORM or migration tool
   ```

2. **Roll back database changes:**
   - If using Entity Framework: `Update-Database -Migration <previous-migration>`
   - If using manual scripts: Run rollback SQL scripts
   - If using Azure SQL: Restore from point-in-time backup

3. **Verify data integrity** after database rollback

**Important:** Always test database rollback procedures in non-production environments first.

## Monitoring During Rollback

Monitor the following during and after rollback:

- **Application logs** in Azure App Service
- **Application Insights** for errors and performance
- **Azure Monitor** for infrastructure metrics
- **User reports** through support channels

## Testing Rollback Procedures

Regularly test rollback procedures to ensure they work:

1. **Quarterly rollback drills** in staging environment
2. **Document any issues** encountered during drills
3. **Update procedures** based on drill outcomes
4. **Train team members** on rollback procedures

## Emergency Contacts

**DevOps Team:**
- Primary: [devops-team@company.com]
- Secondary: [devops-oncall@company.com]

**Azure Support:**
- Support Portal: https://portal.azure.com/#blade/Microsoft_Azure_Support/HelpAndSupportBlade
- Phone: [Azure Support Phone Number]

**Escalation:**
- Engineering Manager: [manager@company.com]
- CTO: [cto@company.com]

## Prevention

To minimize the need for rollbacks:

1. **Comprehensive testing** in CI pipeline
2. **Gradual rollouts** using feature flags
3. **Monitoring and alerting** to catch issues early
4. **Canary deployments** for high-risk changes
5. **Automated smoke tests** after deployment
6. **Blue-green deployments** for zero-downtime releases

## References

- [Azure App Service Deployment Documentation](https://docs.microsoft.com/azure/app-service/deploy-best-practices)
- [GitHub Actions Documentation](https://docs.github.com/actions)
- [Azure Deployment Slots](https://docs.microsoft.com/azure/app-service/deploy-staging-slots)
