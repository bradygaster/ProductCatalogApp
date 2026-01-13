# Deployment Rollback Procedures

This document outlines the procedures for rolling back deployments in case of issues or failures.

## Overview

Rollbacks allow you to quickly revert to a previous stable version of the application when issues are detected in production. This guide covers multiple rollback strategies and when to use each.

## Quick Rollback Checklist

- [ ] Identify the issue and confirm rollback is necessary
- [ ] Notify team members about the rollback
- [ ] Choose appropriate rollback method
- [ ] Execute rollback procedure
- [ ] Verify application is functioning correctly
- [ ] Document the incident and root cause
- [ ] Plan and implement fix for the underlying issue

## Rollback Methods

### Method 1: Azure Portal Slot Swap Rollback (Recommended for Production)

**Best for:** Quick rollbacks in production with minimal downtime  
**Prerequisites:** Deployment slots configured in Azure App Service

#### Steps:
1. **Access Azure Portal**
   - Navigate to https://portal.azure.com
   - Select the Azure subscription
   - Navigate to the App Service resource

2. **Swap Slots**
   - Go to **Deployment** → **Deployment slots**
   - Click **Swap**
   - Select source slot (e.g., production) and target slot (e.g., staging)
   - Review settings that will be swapped
   - Click **Swap** to execute

3. **Verify Rollback**
   - Access the application URL
   - Verify the previous version is running
   - Check application logs for errors
   - Monitor application insights for anomalies

**Time to complete:** ~2-5 minutes  
**Downtime:** Minimal (typically < 30 seconds)

### Method 2: Redeploy Previous GitHub Release

**Best for:** Environments without deployment slots, or when specific version restoration is needed  
**Prerequisites:** GitHub Actions CD workflow configured

#### Steps:
1. **Identify Previous Stable Release**
   ```bash
   # List recent releases
   gh release list --limit 10
   
   # Or view in GitHub UI
   # Repository → Releases → Find stable release
   ```

2. **Trigger Manual Deployment**
   - Navigate to GitHub Actions tab
   - Select the **CD** workflow
   - Click **Run workflow**
   - Select target environment (staging or production)
   - Workflow will redeploy the latest code from main branch

3. **Alternative: Revert Code and Redeploy**
   ```bash
   # Find the commit of the last stable release
   git log --oneline
   
   # Create a revert commit
   git revert <bad-commit-sha>
   
   # Push to main branch (triggers automatic deployment to dev)
   git push origin main
   
   # For staging/production, use workflow dispatch
   ```

4. **Verify Rollback**
   - Monitor GitHub Actions workflow execution
   - Check deployment logs for errors
   - Verify application functionality
   - Review application metrics

**Time to complete:** ~10-20 minutes (depending on build and deployment time)  
**Downtime:** Variable (5-15 minutes typical)

### Method 3: Azure CLI Rollback

**Best for:** Automated rollback scripts or when portal access is limited  
**Prerequisites:** Azure CLI installed and authenticated

#### Steps:
1. **List Previous Deployments**
   ```bash
   # Show deployment history
   az webapp deployment list \
     --name <webapp-name> \
     --resource-group <resource-group> \
     --query "[].{id:id, status:status, author:author, timestamp:receivedTime}" \
     --output table
   ```

2. **Redeploy Specific Version**
   ```bash
   # Redeploy a previous successful deployment
   az webapp deployment source show \
     --name <webapp-name> \
     --resource-group <resource-group> \
     --deployment-id <deployment-id>
   
   # If using deployment slots, swap
   az webapp deployment slot swap \
     --name <webapp-name> \
     --resource-group <resource-group> \
     --slot <source-slot> \
     --target-slot production
   ```

3. **Verify Rollback**
   ```bash
   # Check application is running
   az webapp show \
     --name <webapp-name> \
     --resource-group <resource-group> \
     --query "state" \
     --output tsv
   
   # View recent logs
   az webapp log tail \
     --name <webapp-name> \
     --resource-group <resource-group>
   ```

**Time to complete:** ~5-10 minutes  
**Downtime:** Minimal if using slots, variable otherwise

### Method 4: Manual Package Redeployment

**Best for:** Emergency situations when other methods fail  
**Prerequisites:** Access to previous deployment packages

#### Steps:
1. **Locate Previous Package**
   - Download from GitHub Actions artifacts (available for 30 days)
   - Or rebuild from specific Git commit/tag

2. **Download Artifact**
   ```bash
   # Using GitHub CLI
   gh run list --workflow=cd.yml --limit 10
   gh run download <run-id> --name deployment-package
   ```

3. **Deploy via Azure Portal**
   - Navigate to App Service in Azure Portal
   - Go to **Deployment** → **Deployment Center**
   - Select **Manual Deployment (Push)**
   - Upload the deployment-package.zip
   - Click **Deploy**

4. **Deploy via Azure CLI**
   ```bash
   az webapp deployment source config-zip \
     --name <webapp-name> \
     --resource-group <resource-group> \
     --src deployment-package.zip
   ```

**Time to complete:** ~10-15 minutes  
**Downtime:** 5-10 minutes typical

## Environment-Specific Procedures

### Development Environment
- **Strategy:** Redeploy from main branch
- **Approval Required:** No
- **Impact:** Low (development only)
- **Process:** Automatic rollback via CI/CD rerun

### Staging Environment
- **Strategy:** Slot swap or redeploy
- **Approval Required:** Yes (team lead)
- **Impact:** Medium (affects testing)
- **Process:** Manual trigger via GitHub Actions

### Production Environment
- **Strategy:** Slot swap (primary), redeploy (backup)
- **Approval Required:** Yes (technical lead + product owner)
- **Impact:** High (affects customers)
- **Process:** Follow incident response procedures

## Post-Rollback Actions

### Immediate Actions (0-30 minutes)
1. **Verify Service Health**
   - Check application is responding
   - Verify key functionality
   - Monitor error rates and performance metrics
   - Review Application Insights dashboards

2. **Communication**
   - Update status page if applicable
   - Notify stakeholders of rollback completion
   - Post incident update in team channels

### Short-term Actions (1-4 hours)
1. **Incident Documentation**
   - Document timeline of events
   - Record rollback method used
   - Note any issues encountered during rollback
   - Capture relevant logs and metrics

2. **Root Cause Analysis (Initial)**
   - Review deployment logs
   - Analyze error messages
   - Identify potential causes
   - Assign investigation owner

### Long-term Actions (1-3 days)
1. **Complete Root Cause Analysis**
   - Conduct thorough investigation
   - Identify root cause of the issue
   - Document findings
   - Create action items

2. **Implement Fixes**
   - Develop fix for identified issues
   - Add tests to prevent regression
   - Review changes with team
   - Plan for redeployment

3. **Process Improvement**
   - Update deployment procedures if needed
   - Improve monitoring and alerting
   - Enhance testing procedures
   - Update runbooks and documentation

## Rollback Decision Matrix

| Severity | Symptoms | Recommended Action | Method |
|----------|----------|-------------------|---------|
| P0 - Critical | Application down, data loss risk, security breach | Immediate rollback | Slot swap (fastest) |
| P1 - High | Major feature broken, high error rate, performance degradation | Rollback within 30 min | Slot swap or redeploy |
| P2 - Medium | Minor feature broken, isolated issues | Investigate, prepare rollback | Assess and decide |
| P3 - Low | Cosmetic issues, non-critical bugs | Fix forward | Deploy hotfix |

## Monitoring and Detection

### Key Metrics to Monitor Post-Deployment
- **Application Health**: HTTP response codes, uptime
- **Performance**: Response times, throughput
- **Errors**: Error rates, exception counts
- **Business Metrics**: Conversion rates, user actions

### Alerting Thresholds
- Error rate > 5% sustained for 5 minutes
- Response time > 3 seconds 95th percentile
- Availability < 99.5% in 10-minute window
- Critical business transaction failures

### Monitoring Tools
- Azure Application Insights
- Azure Monitor
- GitHub Actions logs
- Application logs in Azure Log Analytics

## Testing Rollback Procedures

Regular testing ensures rollback procedures work when needed:

### Quarterly Rollback Drills
1. Schedule rollback drill
2. Notify team in advance
3. Execute rollback in staging environment
4. Time the procedure
5. Document issues encountered
6. Update procedures as needed

### Validation Steps
- [ ] Test slot swap in staging
- [ ] Verify artifact download process
- [ ] Practice Azure CLI commands
- [ ] Confirm access permissions
- [ ] Review and update documentation

## Emergency Contacts

### Escalation Path
1. **First Responder**: On-call engineer
2. **Technical Lead**: [Name/Contact]
3. **DevOps Lead**: [Name/Contact]
4. **CTO/VP Engineering**: [Name/Contact]

### Communication Channels
- **Incident Channel**: #incidents (Slack/Teams)
- **Status Updates**: #engineering-status
- **Stakeholder Updates**: Email to leadership

## Additional Resources

- [Azure App Service Deployment Slots](https://docs.microsoft.com/azure/app-service/deploy-staging-slots)
- [GitHub Actions Artifacts](https://docs.github.com/actions/using-workflows/storing-workflow-data-as-artifacts)
- [Incident Response Plan](../incident-response.md) (if exists)
- [Azure CLI Reference](https://docs.microsoft.com/cli/azure/webapp/deployment)

## Revision History

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-01-13 | 1.0 | Initial rollback procedures | GitHub Copilot |
