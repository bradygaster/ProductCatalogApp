# .NET 10 & Azure Container Apps Modernization Assessment

**Assessment Date:** 2026-01-13  
**Target Platform:** .NET 10 with Azure Container Apps  
**Status:** ✅ Complete

---

## Executive Summary

This repository contains a three-tier e-commerce application built on .NET Framework 4.8.1 using legacy Windows-specific technologies (ASP.NET MVC 5, WCF, MSMQ). Modernization to .NET 10 and Azure Container Apps is **feasible but requires significant refactoring** due to dependencies on Windows-only components.

**Complexity Score: 7/10** (High)

---

## Current Architecture

### Projects Analyzed

| Project | Type | Framework | Description |
|---------|------|-----------|-------------|
| **ProductCatalog** | ASP.NET MVC 5 Web App | .NET Framework 4.8.1 | E-commerce frontend with shopping cart |
| **ProductServiceLibrary** | WCF Service Library | .NET Framework 4.8.1 | Product catalog service |
| **OrderProcessor** | Console Application | .NET Framework 4.8.1 | Background order processor |

### Technology Stack

#### Frontend (ProductCatalog)
- **Framework:** ASP.NET MVC 5.2.9 (legacy)
- **View Engine:** Razor
- **UI Libraries:** Bootstrap 5.x, jQuery 3.7.0
- **Messaging:** System.Messaging (MSMQ)
- **Service Communication:** WCF SOAP client

#### Service Layer (ProductServiceLibrary)
- **Framework:** WCF (Windows Communication Foundation)
- **Protocols:** SOAP/XML
- **Data Access:** In-memory repository with hardcoded data
- **Serialization:** DataContract/XmlSerializer

#### Backend Processing (OrderProcessor)
- **Type:** Console application
- **Messaging:** MSMQ (Microsoft Message Queuing)
- **Processing Model:** Synchronous polling

---

## Detected Legacy Patterns

### 🔴 Critical Blockers (Windows-Only)

1. **MSMQ (Microsoft Message Queuing)**
   - Location: `ProductCatalog/Services/OrderQueueService.cs`, `OrderProcessor/Program.cs`
   - Impact: Windows-only, not available in Linux containers
   - Files: 2 files, ~150 lines of code
   - **Migration Required:** Replace with Azure Service Bus or Azure Storage Queues

2. **WCF (Windows Communication Foundation)**
   - Location: `ProductServiceLibrary/` (entire project)
   - Impact: Legacy, verbose, not supported in .NET Core/.NET 5+
   - Files: 6 files, ~800 lines
   - **Migration Required:** Convert to ASP.NET Core Web API (REST) or gRPC

3. **System.Messaging**
   - Usage: Order queue operations
   - **Migration Required:** Use Azure SDK for Service Bus/Storage

### 🟡 Major Modernization Needs

4. **ASP.NET MVC 5 → ASP.NET Core MVC**
   - Location: `ProductCatalog/` web application
   - Impact: Different project structure, middleware pipeline, dependency injection
   - Files: ~25 files (controllers, views, startup)
   - **Effort:** High - requires project template conversion

5. **Old-style .csproj Format**
   - Impact: All three projects use verbose XML format
   - **Migration:** Convert to SDK-style project format

6. **Session State Management**
   - Location: `ProductCatalog/Controllers/HomeController.cs`
   - Current: In-memory session (not container-friendly)
   - **Migration Required:** Use distributed cache (Redis, Azure Cache)

7. **Configuration Management**
   - Current: `Web.config`, `App.config` (XML-based)
   - **Migration Required:** `appsettings.json`, environment variables, Azure Key Vault

### 🟢 Minor Updates Required

8. **NuGet Packages**
   - Many outdated packages: jQuery Validate, Modernizr, etc.
   - **Action:** Update to latest compatible versions

9. **Static Content Serving**
   - Current: IIS-based serving of Scripts/, Content/ folders
   - **Action:** Use ASP.NET Core static files middleware

---

## Complexity Breakdown

| Category | Complexity (1-10) | Justification |
|----------|-------------------|---------------|
| **WCF Replacement** | 8 | Requires complete service redesign, API contracts, client updates |
| **MSMQ to Azure Service Bus** | 7 | Message patterns differ, reliability/retry logic needed |
| **ASP.NET MVC Migration** | 6 | Moderate - routing, filters, and DI changes |
| **Session State** | 5 | Needs distributed cache, session serialization |
| **Configuration** | 3 | Straightforward mapping to appsettings.json |
| **UI/Frontend** | 2 | Minimal changes - mostly static files |

**Overall Complexity: 7/10**

---

## Azure Container Apps Considerations

### ✅ Well-Suited For

1. **ProductCatalog (Web App)**
   - HTTP ingress with public endpoint
   - Scalable web frontend
   - Good fit for Container Apps with ingress enabled

2. **OrderProcessor (Background Worker)**
   - After MSMQ→Service Bus migration
   - Perfect for Container Apps background processing (scale-to-zero)
   - Event-driven scaling based on queue depth

3. **New API Service (Replaces WCF)**
   - RESTful API in Container Apps
   - Internal ingress for service-to-service communication

### ⚠️ Challenges

1. **No Windows Containers:** Must migrate Windows-specific dependencies
2. **Stateless Required:** Session state must use external cache (Azure Cache for Redis)
3. **MSMQ Not Available:** Must use Azure Service Bus or Storage Queues
4. **WCF Not Supported:** Must replace with REST API or gRPC

---

## Recommended Migration Path

### Phase 1: Foundation (Estimated: 2-3 weeks)

**Goal:** Modernize to .NET 10 while maintaining functionality

1. **Convert Projects to SDK-Style**
   - Use `dotnet new` or `upgrade-assistant` tool
   - Migrate to .NET 10

2. **Replace WCF with ASP.NET Core Web API**
   - Create new Web API project (`ProductCatalogApi`)
   - Migrate `IProductService` interface to REST controllers
   - Implement JSON serialization (System.Text.Json)
   - Update `ProductCatalog` to use HttpClient instead of WCF proxy

3. **Migrate ASP.NET MVC to ASP.NET Core MVC**
   - Create new ASP.NET Core MVC project
   - Port controllers, views, and models
   - Replace `Web.config` with `appsettings.json`
   - Implement `Program.cs` and dependency injection

4. **Update Configuration**
   - Migrate AppSettings to `appsettings.json`
   - Use Azure Key Vault for secrets
   - Implement Options pattern

### Phase 2: Cloud-Native (Estimated: 2-3 weeks)

**Goal:** Replace Windows-specific components with Azure services

5. **Replace MSMQ with Azure Service Bus**
   - Provision Azure Service Bus namespace
   - Create queue: `productcatalog-orders`
   - Update `OrderQueueService` to use `Azure.Messaging.ServiceBus`
   - Update `OrderProcessor` to consume from Service Bus
   - Implement retry policies and dead-letter handling

6. **Implement Distributed Session State**
   - Provision Azure Cache for Redis
   - Configure session state provider
   - Update `Startup.cs`/`Program.cs` configuration

7. **Data Layer Enhancement**
   - Replace in-memory `ProductRepository` with real data store
   - Options: Azure SQL Database, Cosmos DB, or PostgreSQL (Flexible Server)

### Phase 3: Containerization (Estimated: 1-2 weeks)

**Goal:** Containerize and deploy to Azure Container Apps

8. **Create Dockerfiles**
   - `ProductCatalog/Dockerfile` - Web frontend
   - `ProductCatalogApi/Dockerfile` - REST API
   - `OrderProcessor/Dockerfile` - Background worker

9. **Multi-stage Builds**
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
   FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
   ```

10. **Azure Container Apps Deployment**
    - Create Container Apps Environment
    - Deploy three Container Apps:
      - `productcatalog-web` (HTTP ingress, external)
      - `productcatalog-api` (HTTP ingress, internal)
      - `order-processor` (no ingress, Service Bus scale rule)

11. **Configure Scaling**
    - Web: HTTP concurrent requests (10-50 replicas)
    - API: HTTP concurrent requests (5-30 replicas)
    - Processor: Service Bus queue length (0-10 replicas, scale-to-zero)

### Phase 4: Production Readiness (Estimated: 1 week)

12. **Observability**
    - Enable Application Insights
    - Configure structured logging (Serilog)
    - Add health checks

13. **Security**
    - Implement managed identity for Azure services
    - Use Azure Key Vault for secrets
    - Enable HTTPS/TLS

14. **CI/CD**
    - GitHub Actions for build and deployment
    - Container registry (Azure Container Registry)

---

## Estimated Effort

| Phase | Duration | Risk |
|-------|----------|------|
| Phase 1: Foundation | 2-3 weeks | Medium |
| Phase 2: Cloud-Native | 2-3 weeks | High |
| Phase 3: Containerization | 1-2 weeks | Low |
| Phase 4: Production Readiness | 1 week | Low |
| **Total** | **6-9 weeks** | **Medium-High** |

---

## Key Risks & Mitigation

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| WCF migration complexity | High | High | Use OpenAPI/Swagger, comprehensive testing |
| Session state loss during migration | Medium | Medium | Implement distributed cache early, test thoroughly |
| MSMQ message loss | High | Low | Use transactional sends, implement retry logic |
| Performance degradation | Medium | Medium | Load testing, performance benchmarking |
| Breaking changes in .NET 10 | Low | Low | Use `upgrade-assistant`, follow migration guides |

---

## Immediate Next Steps

1. ✅ **Complete Assessment** (This document)
2. 🔄 **Proof of Concept**
   - Convert one small service/controller to .NET 10
   - Test WCF→REST conversion
   - Validate MSMQ→Service Bus pattern
3. 📋 **Detailed Sprint Planning**
   - Break down phases into user stories
   - Assign story points
4. 🛠️ **Setup Development Environment**
   - .NET 10 SDK
   - Azure subscription
   - Container Apps environment

---

## Alternative Approaches

### Option A: Lift and Shift (Not Recommended)
- Deploy to Azure App Service with Windows plan
- Keep WCF, MSMQ as-is
- ❌ **Downsides:** High costs, no container benefits, tech debt remains

### Option B: Hybrid Migration
- Phase 1: API + Web to .NET 10 + Linux containers
- Phase 2: Keep OrderProcessor on Windows VM with MSMQ
- ⚠️ **Downsides:** Operational complexity, split architecture

### Option C: Full Rewrite (Overkill)
- Completely new architecture, microservices
- ❌ **Downsides:** Excessive effort for application size

**Recommendation: Follow the phased approach (Phases 1-4)**

---

## Conclusion

Modernizing this application to .NET 10 and Azure Container Apps is **achievable with significant but manageable effort**. The main challenges are replacing Windows-specific technologies (WCF, MSMQ) with cloud-native alternatives. The resulting architecture will be more scalable, cost-effective, and maintainable.

**Recommendation:** Proceed with the phased migration plan, starting with a proof of concept for the highest-risk items (WCF→REST, MSMQ→Service Bus).
