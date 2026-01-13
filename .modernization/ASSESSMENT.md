# Modernization Assessment Report
**Product Catalog Application**

---

## Executive Summary

**Assessment Date:** January 13, 2026  
**Repository:** bradygaster/ProductCatalogApp  
**Current Framework:** .NET Framework 4.8.1  
**Target Framework:** .NET 10  
**Target Platform:** Azure Container Apps  
**Complexity Score:** 7/10 (Moderate-to-High)  
**Estimated Duration:** 6-8 weeks

This assessment evaluates the Product Catalog Application for modernization from .NET Framework 4.8.1 to .NET 10 with deployment to Azure Container Apps. The application is a three-tier product catalog and ordering system using ASP.NET MVC, WCF services, and MSMQ messaging.

### Key Findings

✅ **Strengths:**
- Small, well-structured codebase
- Clear separation of concerns (MVC, Service Layer, Message Processor)
- Modern UI dependencies (Bootstrap 5, jQuery 3.7)
- Simple data access pattern (in-memory repository)

⚠️ **Challenges:**
- Heavy reliance on Windows-specific technologies (WCF, MSMQ)
- Session-based state management incompatible with containers
- Legacy project format and configuration
- IIS-specific hosting assumptions

---

## Current State Analysis

### Framework & Platform

| Component | Current State | Notes |
|-----------|--------------|-------|
| **Framework** | .NET Framework 4.8.1 | Last version of .NET Framework |
| **UI Framework** | ASP.NET MVC 5.2.9 | Legacy web framework |
| **Platform** | Windows | Requires Windows Server for IIS |
| **Hosting** | IIS (Internet Information Services) | Windows-specific web server |

### Project Structure

The solution consists of three projects:

1. **ProductCatalog** (ASP.NET MVC Web Application)
   - Main web application with product browsing and cart functionality
   - Uses WCF client to communicate with ProductServiceLibrary
   - Implements MSMQ integration for order submission
   - Session-based shopping cart storage

2. **ProductServiceLibrary** (WCF Service Library)
   - Exposes product catalog operations via WCF
   - In-memory product repository
   - SOAP-based service contracts

3. **OrderProcessor** (Console Application)
   - Background order processing daemon
   - Consumes orders from MSMQ queue
   - Simulates order fulfillment workflow

### Dependencies Analysis

#### NuGet Packages

| Package | Version | Status | Migration Path |
|---------|---------|--------|----------------|
| Microsoft.AspNet.Mvc | 5.2.9 | ⚠️ Legacy | Migrate to Microsoft.AspNetCore.Mvc |
| Microsoft.AspNet.Razor | 3.2.9 | ⚠️ Legacy | Built into ASP.NET Core |
| Microsoft.AspNet.WebPages | 3.2.9 | ⚠️ Legacy | Built into ASP.NET Core |
| Newtonsoft.Json | 13.0.3 | ✅ Compatible | Consider System.Text.Json |
| bootstrap | 5.2.3 | ✅ Compatible | No changes needed |
| jQuery | 3.7.0 | ✅ Compatible | No changes needed |

#### System Dependencies

| Assembly | Purpose | Status | Alternative |
|----------|---------|--------|-------------|
| System.Web | ASP.NET infrastructure | ❌ Not available | ASP.NET Core equivalents |
| System.ServiceModel | WCF communication | ❌ Not available | REST API, gRPC, or CoreWCF |
| System.Messaging | MSMQ operations | ❌ Not available | Azure Service Bus, RabbitMQ |

---

## Legacy Patterns Identified

### 1. WCF Service Communication
**Impact:** 🔴 High  
**Location:** ProductServiceLibrary, ProductCatalog/Connected Services

**Description:**  
The application uses Windows Communication Foundation (WCF) for service-to-service communication. The ProductCatalog web app communicates with ProductServiceLibrary via WCF SOAP endpoints.

**Issues:**
- WCF is not supported in .NET Core/.NET 5+
- SOAP overhead compared to modern protocols
- Complex configuration and tooling
- Tight coupling between services

**Modernization Strategy:**
```
Option 1: Migrate to REST API
  - Create ASP.NET Core Web API project
  - Implement RESTful endpoints
  - Use HTTP clients for communication
  - Recommended for simplicity

Option 2: Migrate to gRPC
  - Use gRPC for high-performance communication
  - Protocol buffers for serialization
  - Better for microservices architecture

Option 3: Use CoreWCF (Bridge Solution)
  - Port to CoreWCF library
  - Maintain SOAP contracts temporarily
  - Use as intermediate step
```

### 2. MSMQ Message Queuing
**Impact:** 🔴 High  
**Location:** OrderProcessor/Program.cs, ProductCatalog/Services/OrderQueueService.cs

**Description:**  
The application uses Microsoft Message Queuing (MSMQ) for asynchronous order processing. When customers submit orders, they're serialized to XML and placed in a private MSMQ queue.

**Issues:**
- MSMQ is Windows-specific and not available on Linux
- Cannot run in Linux containers
- Not supported in Azure Container Apps
- Local infrastructure dependency

**Modernization Strategy:**
```
Recommended: Azure Service Bus
  - Fully managed message broker
  - Drop-in replacement for MSMQ
  - Supports queues and topics
  - Native Azure integration
  - Enterprise messaging features

Alternative: Azure Storage Queues
  - Simpler, lower cost option
  - Good for basic queueing needs
  - Less features than Service Bus

Migration Approach:
  1. Create Azure Service Bus namespace
  2. Implement dual-write during transition
  3. Migrate OrderProcessor to consume from both
  4. Cutover and decommission MSMQ
```

### 3. Session State Management
**Impact:** 🟡 Medium  
**Location:** ProductCatalog/Controllers/HomeController.cs (Cart storage)

**Description:**  
Shopping cart data is stored in ASP.NET Session state (in-memory). This creates statefulness that's incompatible with containerized, scalable deployments.

**Issues:**
- In-memory state doesn't persist across container restarts
- Prevents horizontal scaling (sticky sessions required)
- Lost on application pool recycle
- Not suitable for cloud-native applications

**Modernization Strategy:**
```
Option 1: Distributed Cache (Recommended)
  - Use Azure Cache for Redis
  - Session state persists across instances
  - Enables true stateless containers
  - Minimal code changes

Option 2: Client-Side State
  - Store cart in browser (localStorage, cookies)
  - Reduces server memory usage
  - More complex synchronization

Implementation:
  - Add Microsoft.Extensions.Caching.StackExchangeRedis
  - Configure distributed session state
  - Update session configuration in Startup.cs
```

### 4. System.Web Dependencies
**Impact:** 🔴 High  
**Location:** Throughout ProductCatalog project

**Description:**  
The application heavily relies on System.Web assemblies which are part of the legacy ASP.NET stack and not available in .NET Core/.NET 5+.

**Issues:**
- System.Web.Mvc, System.Web.Optimization not available
- Configuration model incompatible (Web.config vs appsettings.json)
- HttpContext differences
- Request/Response pipeline changes

**Modernization Strategy:**
```
ASP.NET Core Migration:
  1. Convert to ASP.NET Core MVC
  2. Replace Web.config with appsettings.json
  3. Update HttpContext usage
  4. Migrate authentication/authorization
  5. Port bundling/minification to new system
  6. Update view compilation
```

### 5. Legacy Project Format
**Impact:** 🟡 Medium  
**Location:** All .csproj files

**Description:**  
Projects use the old-style .csproj format with verbose XML, PackageReference via packages.config, and manual file inclusion.

**Issues:**
- Verbose and hard to maintain
- No implicit file globbing
- Separate packages.config file
- Not optimized for modern tooling

**Modernization Strategy:**
```
Convert to SDK-Style Projects:
  - Use dotnet migrate or manual conversion
  - Move to PackageReference format
  - Simplify .csproj structure
  - Enable modern features (implicit usings, nullable reference types)
```

### 6. IIS-Specific Configuration
**Impact:** 🟡 Medium  
**Location:** Web.config, ProductCatalog.csproj

**Description:**  
Application configuration and deployment are tightly coupled to IIS hosting model.

**Issues:**
- Web.config not used in ASP.NET Core
- IIS-specific modules and handlers
- Windows-specific paths and settings
- Not portable to containers

**Modernization Strategy:**
```
Migrate to Kestrel:
  - ASP.NET Core uses Kestrel web server
  - Configuration via appsettings.json
  - Environment-based configuration
  - Container-ready by design
```

---

## Complexity Analysis

### Overall Complexity Score: 7/10

**Scale:**
- 1-3: Low complexity, straightforward migration
- 4-6: Medium complexity, some challenges
- 7-8: High complexity, significant refactoring required
- 9-10: Very high complexity, consider rewrite

### Factor Breakdown

| Factor | Score | Weight | Rationale |
|--------|-------|--------|-----------|
| **Architecture Migration** | 8/10 | High | WCF → API and MSMQ → Service Bus require significant refactoring |
| **Framework Migration** | 7/10 | High | ASP.NET MVC → Core MVC well-documented but substantial |
| **State Management** | 6/10 | Medium | Session state migration requires distributed cache setup |
| **Dependencies** | 6/10 | Medium | Most packages have equivalents; System.Web requires replacement |
| **Codebase Size** | 4/10 | Low | Small codebase (~15 source files) is manageable |
| **Containerization** | 8/10 | High | Windows dependencies make containerization challenging |

### Complexity Justification

The **7/10 complexity score** reflects:

**High Complexity Factors:**
- No direct WCF equivalent in modern .NET (requires architecture change)
- MSMQ is Windows-specific (requires messaging platform migration)
- Multiple legacy patterns requiring simultaneous modernization

**Mitigating Factors:**
- Small, well-organized codebase
- Clear separation of concerns
- Simple business logic
- No complex data access layer
- Good documentation (MSMQ_SETUP.md)

**Comparison:**
- More complex than simple .NET Framework console app migration (3/10)
- Less complex than large enterprise app with Entity Framework + Windows Services (9/10)
- Similar to typical .NET Framework web app with external dependencies (6-8/10)

---

## Migration Recommendations

### Target Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure Container Apps                       │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────────┐        ┌──────────────────────┐  │
│  │  ProductCatalog.Web  │        │   OrderProcessor     │  │
│  │   (ASP.NET Core)     │───────▶│   (Worker Service)   │  │
│  │      .NET 10         │        │      .NET 10         │  │
│  └──────────┬───────────┘        └──────────┬───────────┘  │
│             │                                │               │
│             │                                │               │
│  ┌──────────▼───────────┐        ┌──────────▼───────────┐  │
│  │  ProductService.API  │        │  Azure Service Bus   │  │
│  │   (REST/gRPC API)    │        │   (Message Queue)    │  │
│  │      .NET 10         │        └──────────────────────┘  │
│  └──────────────────────┘                                   │
│                                                               │
│  ┌──────────────────────────────────────────────────────┐  │
│  │        Azure Cache for Redis (Session State)         │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### Recommended Migration Approach

**Strategy:** Incremental migration with parallel run capability

**Rationale:**
- Minimizes risk by allowing gradual cutover
- Enables testing at each phase
- Maintains production system during migration
- Allows rollback if issues arise

### Migration Phases

#### Phase 1: Project Modernization
**Duration:** 1 week  
**Effort:** Low  
**Priority:** High

**Tasks:**
- Convert .csproj files to SDK-style format
- Upgrade to .NET 10 (initially targeting Windows)
- Update NuGet packages to .NET 10 compatible versions
- Migrate packages.config to PackageReference
- Enable nullable reference types and modern C# features

**Deliverables:**
- All projects compile on .NET 10
- Updated project files
- Verified local build

#### Phase 2: Service Layer Refactoring
**Duration:** 1-2 weeks  
**Effort:** Medium  
**Priority:** High

**Tasks:**
- Create new ProductService.Api project (ASP.NET Core Web API)
- Implement REST endpoints for product operations
- Port business logic from WCF service
- Add health checks and logging
- Implement API versioning
- Create integration tests

**Deliverables:**
- REST API with full CRUD operations
- OpenAPI/Swagger documentation
- Unit and integration tests
- Local Docker container for API

#### Phase 3: Web Application Migration
**Duration:** 2 weeks  
**Effort:** Medium  
**Priority:** High

**Tasks:**
- Create new ProductCatalog.Web project (ASP.NET Core MVC)
- Port controllers, models, and views
- Replace WCF client with HTTP client
- Migrate Web.config to appsettings.json
- Update bundling and minification
- Implement dependency injection
- Port custom middleware/filters

**Deliverables:**
- Functional ASP.NET Core MVC application
- Migrated views and controllers
- Configuration files
- Local testing environment

#### Phase 4: Messaging Modernization
**Duration:** 1 week  
**Effort:** Medium  
**Priority:** High

**Tasks:**
- Provision Azure Service Bus namespace
- Create order processing queue
- Implement Service Bus sender in web app
- Update OrderProcessor to consume from Service Bus
- Implement error handling and dead-letter processing
- Add monitoring and alerting

**Deliverables:**
- Azure Service Bus infrastructure
- Updated sender/receiver code
- Monitoring dashboard
- Migration runbook

#### Phase 5: State Management Migration
**Duration:** 3-4 days  
**Effort:** Low  
**Priority:** Medium

**Tasks:**
- Provision Azure Cache for Redis
- Configure distributed session state
- Update session configuration in web app
- Test session persistence across restarts
- Implement session serialization for complex types

**Deliverables:**
- Redis cache instance
- Updated session configuration
- Verified cart persistence

#### Phase 6: Containerization
**Duration:** 3-4 days  
**Effort:** Low  
**Priority:** Medium

**Tasks:**
- Create Dockerfiles for each application
- Build and test Linux containers locally
- Optimize image size (multi-stage builds)
- Configure container environment variables
- Set up health checks and readiness probes
- Create docker-compose for local development

**Deliverables:**
- Dockerfiles for all projects
- Local container testing environment
- Container registry setup (Azure Container Registry)

#### Phase 7: Infrastructure as Code & Deployment
**Duration:** 1 week  
**Effort:** Low  
**Priority:** Medium

**Tasks:**
- Create Azure Container Apps environment
- Implement Bicep/Terraform templates
- Configure ingress and scaling rules
- Set up Azure AD authentication
- Implement CI/CD pipelines
- Configure monitoring (Application Insights)
- Conduct load testing

**Deliverables:**
- Infrastructure as Code templates
- Deployed Azure Container Apps
- CI/CD pipeline
- Monitoring and alerting
- Load test results

---

## Risks and Mitigation Strategies

### High-Severity Risks

#### Risk 1: Breaking Changes in Service Contracts
**Severity:** 🔴 High  
**Impact:** Potential downtime if old clients can't communicate with new API

**Mitigation:**
- Implement API versioning from the start (e.g., /api/v1/, /api/v2/)
- Run parallel WCF and REST endpoints during transition period
- Use feature flags to gradually migrate traffic
- Maintain backward compatibility in data contracts
- Extensive integration testing before cutover

#### Risk 2: MSMQ Message Loss During Migration
**Severity:** 🔴 High  
**Impact:** Lost orders during transition period

**Mitigation:**
- Implement dual-write pattern: send to both MSMQ and Azure Service Bus
- Monitor both queues during transition
- Implement idempotency in OrderProcessor to handle duplicates
- Drain MSMQ completely before decommissioning
- Keep MSMQ infrastructure for 1-2 weeks post-migration as backup

### Medium-Severity Risks

#### Risk 3: Session State Data Loss
**Severity:** 🟡 Medium  
**Impact:** Shopping carts lost during cutover, poor user experience

**Mitigation:**
- Schedule migration during low-traffic period
- Implement session migration utilities
- Display maintenance notice to active users
- Consider cart persistence to database as backup
- Provide customer support contact during migration

#### Risk 4: Performance Changes
**Severity:** 🟡 Medium  
**Impact:** Different performance characteristics may affect user experience

**Mitigation:**
- Conduct thorough load testing before migration
- Benchmark MSMQ vs Azure Service Bus performance
- Optimize Azure Service Bus configuration (prefetch, batching)
- Monitor performance metrics closely post-migration
- Have rollback plan ready

### Low-Severity Risks

#### Risk 5: Windows-Specific Dependencies
**Severity:** 🟢 Low  
**Impact:** Unexpected dependencies on Windows features

**Mitigation:**
- Comprehensive testing on Linux containers
- Review all file path usage (Windows vs Unix)
- Check for Windows-specific APIs
- Test on Azure Container Apps early and often

---

## Estimated Timeline

### Total Duration: 6-8 Weeks

```
Week 1: Planning & Setup
├─ Assessment review and approval
├─ Azure infrastructure provisioning
└─ Development environment setup

Weeks 2-3: Core Migration
├─ Phase 1: Project modernization (Week 2)
├─ Phase 2: Service layer refactoring (Weeks 2-3)
└─ Phase 3: Web app migration begins (Week 3)

Weeks 4-5: Application Migration
├─ Phase 3: Web app migration continues
├─ Phase 4: Messaging modernization
└─ Phase 5: State management migration

Week 6: Containerization & Testing
├─ Phase 6: Containerization
├─ Integration testing
└─ User acceptance testing (UAT)

Weeks 7-8: Deployment & Validation
├─ Phase 7: Infrastructure & CI/CD
├─ Production deployment
├─ Post-deployment validation
└─ Documentation and handoff
```

### Resource Requirements

**Team Composition:**
- 1-2 Full-Stack Developers (.NET expertise)
- 1 Azure/DevOps Engineer (part-time)
- 1 QA Engineer (during testing phases)

**Azure Resources:**
- Azure Container Apps Environment
- Azure Service Bus Namespace (Standard tier)
- Azure Cache for Redis (Basic or Standard tier)
- Azure Container Registry
- Application Insights

**Assumptions:**
- Developers have .NET Core/Modern .NET experience
- Access to Azure subscription with appropriate permissions
- Stakeholders available for reviews and UAT
- No major feature additions during migration period
- Existing test coverage or acceptance criteria available

---

## Success Criteria

### Technical Metrics

- ✅ All applications running on .NET 10
- ✅ Zero Windows-specific dependencies
- ✅ All services containerized and running in Azure Container Apps
- ✅ MSMQ fully replaced with Azure Service Bus
- ✅ Session state persisted in Redis
- ✅ Health checks passing for all services
- ✅ API documentation complete and accurate

### Performance Metrics

- ✅ Page load times within 10% of baseline
- ✅ API response times < 200ms (p95)
- ✅ Message processing latency < 5 seconds (p95)
- ✅ Zero message loss during migration
- ✅ 99.9% uptime during cutover

### Business Metrics

- ✅ Zero lost orders during migration
- ✅ No increase in customer support tickets
- ✅ All features working as before
- ✅ Improved deployment frequency (CI/CD enabled)
- ✅ Reduced infrastructure costs (vs Windows VMs)

---

## Next Steps

### Immediate Actions (Week 1)

1. **Stakeholder Review**
   - Present assessment findings to stakeholders
   - Get approval for migration approach
   - Align on timeline and resources

2. **Azure Infrastructure Setup**
   - Provision development Azure Container Apps environment
   - Create Service Bus namespace
   - Set up Redis cache instance
   - Configure Container Registry

3. **Development Environment**
   - Set up migration branch in Git
   - Install .NET 10 SDK
   - Configure Docker Desktop
   - Set up local Azure Service Bus emulator

4. **Documentation**
   - Create detailed migration runbook
   - Document rollback procedures
   - Prepare communication plan

### Week 2 Actions

5. **Begin Phase 1: Project Modernization**
   - Convert projects to SDK-style
   - Upgrade to .NET 10
   - Validate builds

6. **CI/CD Planning**
   - Design GitHub Actions workflows
   - Set up staging environment
   - Configure deployment gates

### Communication Plan

- **Weekly Status Updates:** Project team and stakeholders
- **Pre-Migration Notice:** 2 weeks before cutover (all users)
- **Maintenance Window:** Schedule and notify users
- **Post-Migration Summary:** Success metrics and lessons learned

---

## Appendix

### A. Useful Resources

**Microsoft Documentation:**
- [Migrate from ASP.NET MVC to ASP.NET Core MVC](https://docs.microsoft.com/aspnet/core/migration/mvc)
- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Azure Container Apps Overview](https://docs.microsoft.com/azure/container-apps/)
- [Distributed caching in ASP.NET Core](https://docs.microsoft.com/aspnet/core/performance/caching/distributed)

**Migration Tools:**
- .NET Upgrade Assistant (CLI tool)
- CoreWCF (WCF bridge for .NET Core)
- Azure Migrate

### B. Key Files to Migrate

**Configuration:**
- Web.config → appsettings.json, appsettings.{Environment}.json
- App.config (OrderProcessor) → appsettings.json

**Projects:**
- ProductCatalog.csproj → ProductCatalog.Web.csproj
- ProductServiceLibrary.csproj → ProductService.Api.csproj
- OrderProcessor → OrderProcessor.Worker.csproj

**Code:**
- Controllers (minor changes for ASP.NET Core)
- Views (.cshtml files mostly compatible)
- Models (add data annotations if needed)
- Services (major refactor for WCF → API, MSMQ → Service Bus)

### C. Glossary

- **WCF:** Windows Communication Foundation - legacy SOAP-based service framework
- **MSMQ:** Microsoft Message Queuing - Windows messaging system
- **gRPC:** Modern RPC framework using HTTP/2 and Protocol Buffers
- **SDK-Style Projects:** Modern .csproj format introduced with .NET Core
- **Kestrel:** Cross-platform web server for ASP.NET Core
- **Azure Container Apps:** Serverless container hosting platform

---

**Assessment Completed By:** GitHub Copilot Modernization Agent  
**Date:** January 13, 2026  
**Version:** 1.0
