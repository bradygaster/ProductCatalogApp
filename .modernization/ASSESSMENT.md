# ProductCatalogApp Modernization Assessment

**Assessment Date:** 2026-01-13  
**Target Framework:** .NET 10  
**Target Platform:** Azure Container Apps  
**Overall Complexity Score:** 7/10

## Executive Summary

The ProductCatalogApp is a legacy .NET Framework 4.8.1 application consisting of three components:
- **ProductCatalog**: ASP.NET MVC 5 web application
- **ProductServiceLibrary**: WCF service library
- **OrderProcessor**: Console application for order processing

To modernize this application to .NET 10 and deploy it to Azure Container Apps, significant architectural changes are required. The migration complexity is **moderate to high** (7/10) due to the use of legacy technologies like WCF and MSMQ that are not supported in modern .NET.

## Current Architecture Analysis

### 1. ProductCatalog (Web Application)
**Framework:** .NET Framework 4.8.1 with ASP.NET MVC 5.2.9

**Current Technologies:**
- ASP.NET MVC 5 with Razor views
- WCF Service Reference for product data
- MSMQ for order queue management
- In-memory session state for shopping cart
- Bootstrap 5.2.3 for UI
- jQuery 3.7.0 for client-side interactions

**Legacy Patterns:**
- ✗ Old-style `.csproj` format (not SDK-style)
- ✗ `packages.config` for NuGet dependencies
- ✗ `Web.config` for configuration
- ✗ WCF client proxies
- ✗ System.Messaging (MSMQ) for message queuing
- ✗ Session-based state management

**Migration Complexity:** 8/10

### 2. ProductServiceLibrary (WCF Service)
**Framework:** .NET Framework 4.8.1 with WCF

**Current Technologies:**
- Windows Communication Foundation (WCF)
- ServiceContract/OperationContract patterns
- In-memory product repository

**Legacy Patterns:**
- ✗ WCF service contracts (not supported in .NET Core/.NET 5+)
- ✗ App.config for service configuration
- ✗ Old-style `.csproj` format

**Migration Complexity:** 7/10

### 3. OrderProcessor (Console Application)
**Framework:** .NET Framework 4.8.1

**Current Technologies:**
- System.Messaging (MSMQ)
- Thread-based message processing
- Console output for order processing

**Legacy Patterns:**
- ✗ MSMQ for message queuing
- ✗ App.config for configuration
- ✗ Manual thread management

**Migration Complexity:** 5/10

## Key Migration Challenges

### 1. ASP.NET MVC to ASP.NET Core MVC ⚠️ HIGH IMPACT
**Challenge:** ASP.NET MVC 5 and ASP.NET Core MVC are fundamentally different frameworks with different APIs, middleware pipelines, and dependency injection systems.

**Required Changes:**
- Rewrite `Global.asax` to `Program.cs` with middleware pipeline
- Convert controllers to use ASP.NET Core MVC patterns
- Update Razor views syntax where needed
- Migrate routing configuration
- Replace `Web.config` settings with `appsettings.json`
- Update dependency injection registration

**Estimated Effort:** 5-8 days

### 2. WCF to Modern Service Communication ⚠️ HIGH IMPACT
**Challenge:** WCF is not supported in .NET Core/.NET 5+. The WCF service needs to be replaced with a modern approach.

**Options:**
1. **gRPC** (Recommended for performance): High-performance RPC framework
2. **REST API** (Recommended for simplicity): HTTP-based API with JSON
3. **CoreWCF**: Community project for WCF compatibility (lower priority)

**Required Changes:**
- Convert `IProductService` WCF contract to modern API
- Rewrite service implementation
- Replace WCF client proxy with HTTP client or gRPC client
- Update all service calls in ProductCatalog

**Estimated Effort:** 3-5 days

### 3. MSMQ to Azure Service Bus ⚠️ HIGH IMPACT
**Challenge:** MSMQ is Windows-specific and not available in Linux containers or Azure Container Apps.

**Solution:** Azure Service Bus (cloud-native message queue)

**Required Changes:**
- Replace `System.Messaging` with `Azure.Messaging.ServiceBus`
- Update `OrderQueueService` to use Service Bus SDK
- Update `OrderProcessor` to consume from Service Bus
- Provision Azure Service Bus namespace and queue
- Update configuration for Service Bus connection strings

**Estimated Effort:** 2-4 days

### 4. Session State to Distributed Cache ⚠️ MEDIUM IMPACT
**Challenge:** In-memory session state won't work across multiple container instances.

**Solution:** Azure Redis Cache for distributed sessions

**Required Changes:**
- Add Redis session provider
- Configure session state to use Redis
- Update cart management to work with distributed sessions
- Provision Azure Redis Cache instance

**Estimated Effort:** 1-2 days

### 5. Configuration Management ⚠️ MEDIUM IMPACT
**Challenge:** Web.config and App.config need to be replaced.

**Solution:** appsettings.json with IConfiguration

**Required Changes:**
- Convert all settings to `appsettings.json`
- Update code to use `IConfiguration`
- Set up environment-specific configuration files
- Configure Azure Container Apps environment variables

**Estimated Effort:** 1-2 days

### 6. Containerization ⚠️ MEDIUM IMPACT
**Challenge:** Applications need to be containerized for Azure Container Apps.

**Required Changes:**
- Create Dockerfile for ProductCatalog web app
- Create Dockerfile for ProductServiceLibrary API
- Create Dockerfile for OrderProcessor worker
- Add docker-compose.yml for local development
- Configure health checks for containers
- Set up Azure Container Apps configuration

**Estimated Effort:** 2-3 days

## Recommended Migration Strategy

### Phase 1: Foundation (3-5 days)
**Goal:** Prepare projects for .NET 10 migration

**Tasks:**
1. Convert all `.csproj` files to SDK-style format
2. Upgrade projects to .NET 10
3. Replace `packages.config` with `PackageReference`
4. Update NuGet packages to .NET 10 compatible versions
5. Convert configuration files to `appsettings.json`

**Success Criteria:**
- All projects compile on .NET 10
- No packages.config files remain
- Configuration loading works with IConfiguration

### Phase 2: Service Layer Modernization (3-5 days)
**Goal:** Replace WCF with modern API

**Tasks:**
1. Create new ASP.NET Core Web API project for ProductService
2. Implement REST API endpoints matching WCF service contract
3. Migrate ProductRepository and data models
4. Add API versioning and documentation (Swagger/OpenAPI)
5. Update ProductCatalog to use HTTP client instead of WCF proxy
6. Test all service endpoints

**Success Criteria:**
- All WCF operations available as REST endpoints
- ProductCatalog successfully calls new API
- No WCF dependencies remain

### Phase 3: Web Application Migration (5-8 days)
**Goal:** Migrate ASP.NET MVC to ASP.NET Core MVC

**Tasks:**
1. Create new ASP.NET Core MVC project
2. Migrate models, view models, and data classes
3. Migrate controllers and action methods
4. Migrate Razor views and layouts
5. Set up middleware pipeline (routing, static files, etc.)
6. Configure dependency injection
7. Replace session state with distributed cache (Redis)
8. Migrate bundling and minification
9. Update client-side assets (jQuery, Bootstrap)
10. Test all pages and functionality

**Success Criteria:**
- All pages render correctly
- All user interactions work
- Shopping cart persists across requests
- Session state works with Redis

### Phase 4: Messaging Modernization (2-4 days)
**Goal:** Replace MSMQ with Azure Service Bus

**Tasks:**
1. Provision Azure Service Bus namespace and queue
2. Create new OrderQueueService using Service Bus SDK
3. Update ProductCatalog to send orders to Service Bus
4. Migrate OrderProcessor to consume from Service Bus
5. Implement retry policies and error handling
6. Test message flow end-to-end

**Success Criteria:**
- Orders successfully sent to Service Bus
- OrderProcessor receives and processes orders
- No MSMQ dependencies remain
- Error handling and retries work

### Phase 5: Containerization & Deployment (2-3 days)
**Goal:** Deploy to Azure Container Apps

**Tasks:**
1. Create Dockerfile for ProductCatalog web app
2. Create Dockerfile for ProductService API
3. Create Dockerfile for OrderProcessor worker
4. Create docker-compose.yml for local testing
5. Build and test containers locally
6. Create Azure Container Apps environment
7. Create Container Apps for each service
8. Configure ingress, scaling, and secrets
9. Set up monitoring and logging
10. Deploy and test in Azure

**Success Criteria:**
- All containers build successfully
- Applications run locally in Docker
- Applications deployed to Azure Container Apps
- All services communicate correctly in Azure
- Scaling and health checks work

## Project Structure Changes

### Before (Current)
```
ProductCatalogApp/
├── ProductCatalog/              # ASP.NET MVC 5 Web App
│   ├── App_Start/
│   ├── Controllers/
│   ├── Models/
│   ├── Views/
│   ├── Web.config
│   └── packages.config
├── ProductServiceLibrary/       # WCF Service
│   ├── IProductService.cs
│   ├── ProductService.cs
│   └── App.config
└── OrderProcessor/              # Console App
    ├── Program.cs
    └── App.config
```

### After (Modernized)
```
ProductCatalogApp/
├── src/
│   ├── ProductCatalog.Web/              # ASP.NET Core MVC
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Views/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   ├── ProductCatalog.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   └── ProductCatalog.OrderProcessor/   # Worker Service
│       ├── Worker.cs
│       ├── Program.cs
│       ├── appsettings.json
│       └── Dockerfile
├── docker-compose.yml
└── .modernization/
    ├── assessment.json
    └── ASSESSMENT.md
```

## Dependencies Update

### Web Application Dependencies
| Package | Current | Target | Notes |
|---------|---------|--------|-------|
| ASP.NET MVC | 5.2.9 | → ASP.NET Core MVC 10.0 | Framework change |
| Newtonsoft.Json | 13.0.3 | → System.Text.Json | Built-in .NET 10 |
| Bootstrap | 5.2.3 | → 5.3.x | Minor update |
| jQuery | 3.7.0 | → 3.7.x | Keep current |

### New Dependencies Needed
- `Microsoft.AspNetCore.App` (metapackage)
- `Azure.Messaging.ServiceBus` (for Service Bus)
- `Microsoft.Extensions.Caching.StackExchangeRedis` (for Redis)
- `Swashbuckle.AspNetCore` (for API documentation)

## Risk Assessment

| Risk | Severity | Likelihood | Mitigation |
|------|----------|------------|------------|
| Breaking changes in ASP.NET Core | High | High | Thorough testing, phased rollout |
| Data loss during queue migration | High | Medium | Parallel run, message backup |
| Session state issues | Medium | Medium | Extensive testing of cart functionality |
| Performance regression | Medium | Low | Load testing, performance monitoring |
| Deployment issues | Low | Low | Staging environment, gradual rollout |

## Estimated Effort

**Total Estimated Duration:** 15-25 business days

**Breakdown:**
- Foundation: 3-5 days
- Service Layer Modernization: 3-5 days
- Web Application Migration: 5-8 days
- Messaging Modernization: 2-4 days
- Containerization & Deployment: 2-3 days

**Team Size:** 1-2 developers

## Success Criteria

- ✅ All applications running on .NET 10
- ✅ No .NET Framework dependencies
- ✅ WCF replaced with REST API
- ✅ MSMQ replaced with Azure Service Bus
- ✅ Session state using distributed cache
- ✅ All applications containerized
- ✅ Successfully deployed to Azure Container Apps
- ✅ All functionality working as before
- ✅ Horizontal scaling working
- ✅ Health checks and monitoring configured

## Benefits of Modernization

1. **Cross-Platform**: Run on Linux containers (lower cost)
2. **Performance**: .NET 10 performance improvements
3. **Scalability**: Horizontal scaling with Azure Container Apps
4. **Cloud-Native**: Leverage Azure PaaS services
5. **Modern Development**: Latest C# features and tooling
6. **Maintainability**: Simplified dependency management
7. **Cost Efficiency**: Pay-per-use container billing
8. **Developer Productivity**: Better IDE support and tooling

## Next Steps

1. ✅ **Review and approve this assessment**
2. 📋 **Create individual task issues** for each migration phase
3. 🏗️ **Begin Phase 1: Foundation** - Project structure updates
4. 🔄 **Iterative development** - Complete phases sequentially
5. 🧪 **Testing** - Comprehensive testing after each phase
6. 🚀 **Deployment** - Deploy to Azure Container Apps

---

**Assessment Completed By:** GitHub Copilot  
**Review Required By:** Development Team Lead  
**Approval Date:** _Pending_
