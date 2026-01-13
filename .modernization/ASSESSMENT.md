# .NET 10 & Azure Container Apps Modernization Assessment

**Repository:** ProductCatalogApp  
**Assessment Date:** 2026-01-13  
**Target Framework:** .NET 10  
**Target Platform:** Azure Container Apps  
**Status:** ✅ Complete

---

## Executive Summary

This is a legacy .NET Framework 4.8.1 application consisting of an ASP.NET MVC 5 web application, a WCF service library, and an MSMQ-based order processor. The application requires **significant modernization** to run on .NET 10 and deploy to Azure Container Apps.

**Overall Complexity Score: 7/10** (High)

**Estimated Effort:** 15-25 days

**Key Blockers:**
- Windows-specific dependencies (MSMQ, WCF)
- ASP.NET MVC 5 framework
- IIS dependencies
- Legacy project format

---

## Current Architecture

### Application Components

#### 1. **ProductCatalog** (ASP.NET MVC 5 Web App)
- **Framework:** .NET Framework 4.8.1
- **Type:** ASP.NET MVC 5 Web Application
- **Purpose:** Customer-facing product catalog and shopping cart
- **Key Technologies:**
  - ASP.NET MVC 5.2.9
  - System.Web (IIS dependencies)
  - WCF client (for product service)
  - MSMQ client (for order submission)
  - In-memory session state

#### 2. **ProductServiceLibrary** (WCF Service)
- **Framework:** .NET Framework 4.8.1
- **Type:** WCF Service Library
- **Purpose:** Product CRUD operations
- **Key Technologies:**
  - Windows Communication Foundation (WCF)
  - BasicHttpBinding
  - ServiceContract/OperationContract attributes

#### 3. **OrderProcessor** (Console Application)
- **Framework:** Unknown (.NET Framework assumed)
- **Type:** Console Application
- **Purpose:** Background order processing
- **Key Technologies:**
  - MSMQ message consumption
  - Console-based worker

---

## Detected Legacy Patterns

### 🔴 High Severity

#### 1. **ASP.NET MVC 5 on .NET Framework**
- **Impact:** Complete framework migration required
- **Effort:** 8-12 days
- **Migration Path:** ASP.NET Core MVC on .NET 10
- **Breaking Changes:**
  - System.Web → ASP.NET Core abstractions
  - Global.asax → Program.cs/Startup.cs
  - Web.config → appsettings.json
  - Routing changes
  - View engine updates

#### 2. **WCF Service**
- **Impact:** WCF not supported in .NET Core/10
- **Effort:** 4-6 days
- **Migration Options:**
  1. **gRPC** (Recommended for microservices)
     - Modern, high-performance RPC framework
     - Native .NET support
     - HTTP/2 based
  2. **ASP.NET Core Web API** (REST)
     - Standard REST patterns
     - Wider client compatibility
     - OpenAPI/Swagger support
  3. **CoreWCF** (Compatibility layer)
     - Maintains WCF contracts
     - Eases migration
     - Limited long-term support

#### 3. **MSMQ (Microsoft Message Queuing)**
- **Impact:** Not available on Linux/containers
- **Effort:** 2-3 days
- **Migration Path:** Azure Service Bus (recommended)
- **Alternatives:**
  - Azure Service Bus (full features, enterprise)
  - Azure Storage Queues (simpler, lower cost)
  - RabbitMQ (self-hosted option)
- **Key Considerations:**
  - Transaction semantics
  - Message ordering guarantees
  - Dead-letter queue handling
  - Retry policies

### 🟡 Medium Severity

#### 4. **System.Web Dependencies**
- **Impact:** In-memory session state not portable
- **Effort:** 1-2 days
- **Migration Path:**
  - Redis Cache (Azure Cache for Redis)
  - SQL Server session state
  - Distributed Memory Cache (dev/test)
- **Affected Areas:**
  - Shopping cart storage
  - Session["Cart"] usage

#### 5. **IIS-Specific Configuration**
- **Impact:** Containers use Kestrel, not IIS
- **Effort:** 0.5-1 day
- **Changes Required:**
  - Remove Web.config HTTP modules
  - Remove IIS deployment settings
  - Configure Kestrel options
  - Update authentication/authorization

#### 6. **Legacy Project Format**
- **Impact:** SDK-style projects required for .NET 10
- **Effort:** 0.5-1 day
- **Migration Path:** Convert to SDK-style .csproj
- **Benefits:**
  - Simpler project files
  - Better NuGet integration
  - Improved build performance

---

## Projects Analysis

### ProductCatalog
- **Current:** ASP.NET MVC 5 (.NET Framework 4.8.1)
- **Target:** ASP.NET Core MVC (.NET 10)
- **Complexity:** High
- **Key Dependencies:**
  - System.Web.Mvc 5.2.9
  - System.Messaging (MSMQ)
  - WCF Service Reference
- **Blockers:**
  - WCF client proxy
  - MSMQ dependency
  - System.Web.Session
  - IIS-specific features

### ProductServiceLibrary
- **Current:** WCF Service Library (.NET Framework 4.8.1)
- **Target:** gRPC service or Web API (.NET 10)
- **Complexity:** High
- **Key Dependencies:**
  - System.ServiceModel (WCF)
- **Blockers:**
  - WCF service contracts
  - WCF-specific attributes

### OrderProcessor
- **Current:** Console App (likely .NET Framework)
- **Target:** Azure Container Apps job or Azure Functions
- **Complexity:** Medium
- **Key Dependencies:**
  - System.Messaging (MSMQ)
- **Blockers:**
  - MSMQ dependency

---

## Azure Container Apps Readiness

### Current Status: ❌ Not Ready

### Blockers
1. ✗ Windows-specific dependencies (MSMQ, WCF)
2. ✗ IIS dependencies
3. ✗ Legacy .NET Framework
4. ✗ No containerization support
5. ✗ No health check endpoints

### Required Changes

#### Infrastructure
- ✓ Migrate to .NET 10
- ✓ Convert to SDK-style projects
- ✓ Create Dockerfiles for each service
- ✓ Set up Azure Container Registry
- ✓ Configure environment variables
- ✓ Implement health check endpoints

#### Application Architecture
- ✓ Replace WCF with gRPC or REST API
- ✓ Replace MSMQ with Azure Service Bus
- ✓ Replace in-memory session with distributed cache
- ✓ Remove IIS dependencies
- ✓ Configure Linux containers

### Recommended Azure Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Azure Container Apps                     │
│                                                               │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │   Web App        │◄────►│   API Service    │            │
│  │  (ASP.NET Core)  │      │   (gRPC/REST)    │            │
│  │   Port: 80/443   │      │   Port: 8080     │            │
│  └──────────────────┘      └──────────────────┘            │
│           │                          │                       │
│           │                          │                       │
│           ▼                          ▼                       │
│  ┌──────────────────────────────────────────┐              │
│  │      Azure Service Bus                    │              │
│  │      (Queues/Topics)                      │              │
│  └──────────────────────────────────────────┘              │
│           │                                                  │
│           ▼                                                  │
│  ┌──────────────────┐                                      │
│  │ Order Processor   │                                      │
│  │ (Background Job)  │                                      │
│  └──────────────────┘                                      │
│                                                              │
└──────────────────────────────────────────────────────────────┘
         │                    │
         ▼                    ▼
┌──────────────────┐  ┌──────────────────┐
│ Azure Cache      │  │ Azure Monitor    │
│ for Redis        │  │ (Logging/Metrics)│
└──────────────────┘  └──────────────────┘
```

**Components:**
1. **Web App Container** - ASP.NET Core MVC application
2. **API Service Container** - gRPC or REST API for product operations
3. **Azure Service Bus** - Message queue for order processing
4. **Order Processor Container** - Background worker for order processing
5. **Azure Cache for Redis** - Distributed session/caching
6. **Azure Container Registry** - Private container image storage
7. **Azure Monitor** - Logging, metrics, and alerting

---

## Migration Strategy

### Recommended Approach: **Incremental Migration**

### Phase 1: Foundation (2-3 days)
**Goal:** Prepare project structure for .NET 10

- [ ] Convert projects to SDK-style format
- [ ] Create .NET 10 project structure
- [ ] Set up dependency injection
- [ ] Configure logging (ILogger)
- [ ] Implement health check endpoints
- [ ] Set up configuration system (appsettings.json)

### Phase 2: Core Migration (8-12 days)
**Goal:** Migrate application logic to .NET 10

#### Web Application
- [ ] Migrate ASP.NET MVC to ASP.NET Core MVC
- [ ] Update controllers and action results
- [ ] Migrate Razor views (minimal changes expected)
- [ ] Update routing configuration
- [ ] Replace System.Web.Session with distributed cache
- [ ] Update authentication/authorization

#### API Service
- [ ] Replace WCF service with gRPC or Web API
- [ ] Migrate service contracts to gRPC proto files or API controllers
- [ ] Update data transfer objects
- [ ] Implement service layer
- [ ] Add API documentation (Swagger/OpenAPI)

### Phase 3: Messaging Migration (2-3 days)
**Goal:** Replace MSMQ with Azure Service Bus

- [ ] Create Azure Service Bus namespace and queue
- [ ] Update OrderQueueService to use Azure.Messaging.ServiceBus
- [ ] Migrate OrderProcessor to Azure Container Apps job
- [ ] Implement retry policies and dead-letter queue handling
- [ ] Add monitoring and alerting

### Phase 4: Containerization (2-3 days)
**Goal:** Prepare for Azure Container Apps deployment

- [ ] Create Dockerfile for web app
- [ ] Create Dockerfile for API service
- [ ] Create Dockerfile for order processor
- [ ] Set up Azure Container Registry
- [ ] Build and push container images
- [ ] Configure Azure Container Apps
- [ ] Set up networking and ingress
- [ ] Configure scaling rules

### Phase 5: Testing & Deployment (2-4 days)
**Goal:** Validate and deploy to production

- [ ] Integration testing
- [ ] Load/performance testing
- [ ] Security testing
- [ ] Set up CI/CD pipeline
- [ ] Production deployment
- [ ] Monitoring and alerting setup
- [ ] Documentation updates

---

## Effort Estimation

| Phase | Component | Estimated Days |
|-------|-----------|----------------|
| **1** | Foundation & Project Setup | 2-3 |
| **2** | ASP.NET Core Migration | 8-12 |
| **2** | WCF to gRPC/API Migration | 4-6 |
| **3** | MSMQ to Service Bus | 2-3 |
| **4** | Containerization | 2-3 |
| **5** | Testing & Deployment | 2-4 |
| **Total** | | **15-25 days** |

**Assumptions:**
- 1 full-time developer
- Familiarity with ASP.NET Core and Azure
- No major business logic changes
- Existing test coverage (if any)

---

## Risks & Mitigation

### High Risk

#### 1. WCF Service Migration Complexity
- **Risk:** Breaking changes in service contracts and behavior
- **Mitigation:**
  - Consider CoreWCF for compatibility layer
  - Thorough integration testing
  - Contract-first development approach
  - Maintain parallel systems during migration

#### 2. MSMQ Transaction Semantics
- **Risk:** Different delivery guarantees and transaction handling
- **Mitigation:**
  - Configure Azure Service Bus with appropriate session mode
  - Implement idempotent message handlers
  - Set up dead-letter queue monitoring
  - Test failure scenarios thoroughly

### Medium Risk

#### 3. Session State Migration
- **Risk:** Shopping cart data loss during migration
- **Mitigation:**
  - Implement Redis-based distributed cache
  - Test session failover scenarios
  - Plan maintenance window for cutover

#### 4. Breaking Changes in ASP.NET Core
- **Risk:** Views, controllers, or routing may break
- **Mitigation:**
  - Comprehensive testing of all routes
  - Visual regression testing
  - Staged rollout strategy

#### 5. Performance Differences
- **Risk:** Container performance vs. IIS
- **Mitigation:**
  - Load testing in pre-production
  - Configure Kestrel appropriately
  - Monitor performance metrics

---

## Dependencies to Modernize

### Replace These NuGet Packages:

| Current | Replace With |
|---------|--------------|
| System.Web.Mvc | Microsoft.AspNetCore.Mvc |
| System.ServiceModel (WCF) | Grpc.AspNetCore or Microsoft.AspNetCore.WebApi |
| System.Messaging (MSMQ) | Azure.Messaging.ServiceBus |
| Newtonsoft.Json | System.Text.Json (recommended) or keep Newtonsoft.Json |
| System.Web.Optimization | WebOptimizer.Core or built-in bundling |

### New Packages to Add:

- Microsoft.AspNetCore.App (metapackage)
- Microsoft.Extensions.Caching.StackExchangeRedis (session)
- Azure.Messaging.ServiceBus (messaging)
- Grpc.AspNetCore (if using gRPC)
- Microsoft.ApplicationInsights.AspNetCore (monitoring)

---

## Next Steps

### Immediate Actions:
1. ✅ Review this assessment with stakeholders
2. ⬜ Get approval for migration approach
3. ⬜ Set up development/test Azure environment
4. ⬜ Create migration project plan
5. ⬜ Begin Phase 1: Foundation work

### Prerequisites:
- Azure subscription with Container Apps support
- Development team trained on .NET 10 and Azure Container Apps
- CI/CD pipeline infrastructure
- Test environment for validation

---

## Recommendations

### Priority: **High**

This application is built on technologies that are either deprecated or not suitable for modern cloud-native deployments. Migration to .NET 10 and Azure Container Apps will provide:

✅ **Benefits:**
- Modern, supported framework (.NET 10)
- Cloud-native architecture
- Container-based deployment
- Better scalability and resilience
- Reduced infrastructure costs
- Improved developer experience
- Better monitoring and observability

⚠️ **Considerations:**
- Significant development effort required
- Thorough testing needed
- Team training on new technologies
- Potential business disruption during migration

### Success Criteria:
- ✓ All features working on .NET 10
- ✓ Deployed to Azure Container Apps
- ✓ No Windows dependencies
- ✓ Health checks and monitoring operational
- ✓ Performance meets or exceeds current system
- ✓ Successfully handling production load

---

## Additional Resources

- [Migrate from ASP.NET to ASP.NET Core](https://learn.microsoft.com/aspnet/core/migration/proper-to-2x)
- [Azure Container Apps documentation](https://learn.microsoft.com/azure/container-apps)
- [Azure Service Bus documentation](https://learn.microsoft.com/azure/service-bus-messaging)
- [gRPC for .NET documentation](https://learn.microsoft.com/aspnet/core/grpc)
- [CoreWCF GitHub repository](https://github.com/CoreWCF/CoreWCF)

---

**Assessment completed by:** GitHub Copilot  
**Last updated:** 2026-01-13
