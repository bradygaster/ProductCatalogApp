# ProductCatalogApp Modernization Assessment

**Assessment Date:** 2026-01-13  
**Repository:** bradygaster/ProductCatalogApp  
**Target:** .NET 10 + Azure Container Apps

---

## Executive Summary

The ProductCatalogApp is a classic .NET Framework 4.8.1 application consisting of three components: an ASP.NET MVC 5 web application, a WCF service library, and an MSMQ-based order processor. This assessment outlines the path to modernize the application to .NET 10 and deploy it on Azure Container Apps.

**Complexity Score:** 7/10 (Medium-High)  
**Estimated Effort:** 60-80 hours  
**Risk Level:** Medium

---

## Current State Analysis

### Application Architecture

The application currently consists of:

1. **ProductCatalog (Web Application)**
   - Technology: ASP.NET MVC 5
   - Framework: .NET Framework 4.8.1
   - Features:
     - Product browsing with category filtering
     - Session-based shopping cart
     - Order submission via MSMQ
     - WCF service consumption for product data
   - Dependencies: MVC 5.2.9, Bootstrap 5.x, jQuery 3.7.0, System.Messaging

2. **ProductServiceLibrary (WCF Service)**
   - Technology: WCF Service Library
   - Framework: .NET Framework 4.8.1
   - Features:
     - Product CRUD operations
     - Category management
     - Search and filtering
     - In-memory data repository
   - Dependencies: System.ServiceModel

3. **OrderProcessor (Console Application)**
   - Technology: .NET Framework Console App
   - Framework: .NET Framework 4.8.1
   - Features:
     - MSMQ message processing
     - Order processing simulation
     - Console-based monitoring
   - Status: Standalone files, not properly structured as a project
   - Dependencies: System.Messaging

### Technology Stack

| Component | Current Technology | Version |
|-----------|-------------------|---------|
| Web Framework | ASP.NET MVC | 5.2.9 |
| Service Framework | WCF | .NET Framework 4.8.1 |
| Messaging | MSMQ | Windows Built-in |
| Session State | In-Memory | ASP.NET Session |
| Data Storage | In-Memory | Custom Repository |
| Project Format | Legacy .csproj | Framework Format |
| Package Management | packages.config | NuGet |
| Configuration | Web.config | XML |
| Hosting | IIS | Windows Server |

---

## Legacy Patterns Identified

### 1. WCF (Windows Communication Foundation) - **HIGH IMPACT**

**Location:** ProductServiceLibrary/*  
**Issue:** WCF is not supported in .NET Core/.NET 10 and is Windows-specific  
**Impact:** Core service communication layer needs complete replacement  
**Modern Alternative:** ASP.NET Core Web API (REST) or gRPC

**Migration Approach:**
- Replace WCF service with ASP.NET Core Web API
- Convert SOAP contracts to REST endpoints
- Update client to use HttpClient instead of WCF proxy
- Consider gRPC for high-performance scenarios

### 2. MSMQ (Microsoft Message Queue) - **HIGH IMPACT**

**Location:** ProductCatalog/Services/OrderQueueService.cs, OrderProcessor/Program.cs  
**Issue:** MSMQ is Windows-specific and not compatible with Linux containers  
**Impact:** Entire messaging infrastructure needs replacement  
**Modern Alternative:** Azure Service Bus or Azure Storage Queues

**Migration Approach:**
- Replace MessageQueue with Azure Service Bus Queue/Topic
- Update OrderQueueService to use Azure.Messaging.ServiceBus SDK
- Migrate message serialization from XML to JSON
- Implement retry policies and dead-letter queue handling

### 3. Session State - **HIGH IMPACT**

**Location:** ProductCatalog/Controllers/HomeController.cs  
**Issue:** In-memory session state doesn't work in containerized/distributed environments  
**Impact:** Shopping cart will be lost on container restart or scaling  
**Modern Alternative:** Distributed cache (Azure Cache for Redis) or database-backed state

**Migration Approach:**
- Implement distributed session with Redis
- Configure session state provider in ASP.NET Core
- Add Microsoft.Extensions.Caching.StackExchangeRedis package
- Update configuration for Redis connection

### 4. ASP.NET MVC 5 - **HIGH IMPACT**

**Location:** ProductCatalog/*  
**Issue:** MVC 5 is .NET Framework-specific, not supported in .NET Core/.NET 10  
**Impact:** Entire web application needs migration  
**Modern Alternative:** ASP.NET Core MVC or Razor Pages

**Migration Approach:**
- Create new ASP.NET Core MVC project targeting .NET 10
- Port controllers, views, and models
- Update view syntax (minor differences from MVC 5)
- Replace System.Web dependencies with ASP.NET Core equivalents
- Update authentication/authorization if present

### 5. Legacy Project Format - **MEDIUM IMPACT**

**Location:** *.csproj  
**Issue:** Old-style .csproj format is verbose and less maintainable  
**Impact:** Project files need conversion  
**Modern Alternative:** SDK-style project format

**Migration Approach:**
- Convert to SDK-style csproj format
- Move from packages.config to PackageReference
- Simplify project structure
- Remove unnecessary MSBuild imports

### 6. XML Configuration - **MEDIUM IMPACT**

**Location:** Web.config, App.config  
**Issue:** XML configuration is replaced by JSON in .NET Core/.NET 10  
**Impact:** Configuration files need conversion  
**Modern Alternative:** appsettings.json and IConfiguration

**Migration Approach:**
- Convert Web.config to appsettings.json
- Use IConfiguration for runtime configuration
- Implement environment-specific settings (Development, Production)
- Move secrets to Azure Key Vault or user secrets

---

## Target State Architecture

### Proposed Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Azure Container Apps                      │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────┐         ┌─────────────────┐           │
│  │  Web App        │────────▶│  Product API    │           │
│  │  (ASP.NET Core) │  REST   │  (ASP.NET Core) │           │
│  │  .NET 10        │         │  .NET 10        │           │
│  └────────┬────────┘         └─────────────────┘           │
│           │                                                  │
│           │ Azure Service Bus                               │
│           ▼                                                  │
│  ┌─────────────────┐         ┌─────────────────┐           │
│  │ Service Bus     │────────▶│ Order Processor │           │
│  │ Queue           │         │ (.NET 10)       │           │
│  └─────────────────┘         └─────────────────┘           │
│                                                               │
└─────────────────────────────────────────────────────────────┘
         │                              │
         ▼                              ▼
┌─────────────────┐         ┌─────────────────┐
│ Azure Cache     │         │ Azure SQL DB    │
│ for Redis       │         │ (Optional)      │
└─────────────────┘         └─────────────────┘
```

### Technology Stack (Target)

| Component | Target Technology | Version |
|-----------|------------------|---------|
| Web Framework | ASP.NET Core MVC | .NET 10 |
| API Framework | ASP.NET Core Web API | .NET 10 |
| Messaging | Azure Service Bus | Latest SDK |
| Session State | Azure Cache for Redis | Latest |
| Data Storage | Azure SQL Database | Latest |
| Container Runtime | Docker | Latest |
| Hosting | Azure Container Apps | Managed |
| Configuration | appsettings.json + Key Vault | .NET 10 |

---

## Migration Strategy

### Recommended Approach: Incremental Modernization

We recommend a phased approach to minimize risk and allow for testing at each stage.

### Phase 1: Foundation (Week 1-2)
**Objective:** Establish API foundation

**Tasks:**
1. Create new .NET 10 solution structure
2. Migrate ProductServiceLibrary WCF to ASP.NET Core Web API
3. Implement REST endpoints replacing WCF operations
4. Add Swagger/OpenAPI documentation
5. Create unit tests for API

**Deliverables:**
- Functional REST API for product operations
- API documentation
- Unit test coverage

### Phase 2: Web Application Migration (Week 2-3)
**Objective:** Modernize web frontend

**Tasks:**
1. Create ASP.NET Core MVC project targeting .NET 10
2. Port controllers, views, and models
3. Replace WCF client with HttpClient-based API calls
4. Update routing and middleware pipeline
5. Configure distributed session state with Redis

**Deliverables:**
- Functional web application on .NET 10
- Redis-based session state
- UI parity with existing application

### Phase 3: Messaging Modernization (Week 3-4)
**Objective:** Replace MSMQ with Azure Service Bus

**Tasks:**
1. Create Azure Service Bus namespace and queue
2. Replace OrderQueueService with Azure Service Bus implementation
3. Create proper .NET 10 OrderProcessor console app
4. Implement retry policies and error handling
5. Add monitoring and logging

**Deliverables:**
- Azure Service Bus-based messaging
- Modernized order processor
- Message processing reliability

### Phase 4: Containerization (Week 4-5)
**Objective:** Containerize all applications

**Tasks:**
1. Create Dockerfile for Web App
2. Create Dockerfile for Product API
3. Create Dockerfile for Order Processor
4. Set up multi-stage builds for optimization
5. Configure health checks and readiness probes
6. Test containers locally with Docker Compose

**Deliverables:**
- Docker images for all components
- Docker Compose configuration
- Local container testing validated

### Phase 5: Azure Deployment (Week 5-6)
**Objective:** Deploy to Azure Container Apps

**Tasks:**
1. Provision Azure Container Apps environment
2. Configure Azure Container Registry
3. Set up CI/CD pipeline (GitHub Actions)
4. Deploy all containers
5. Configure scaling rules
6. Set up monitoring and Application Insights
7. Perform load testing and optimization

**Deliverables:**
- Production deployment on Azure Container Apps
- CI/CD automation
- Monitoring and alerting configured

---

## Complexity Breakdown

| Area | Complexity (1-10) | Justification |
|------|-------------------|---------------|
| Framework Migration | 8 | Multiple projects, different types (Web, WCF, Console) |
| WCF Replacement | 9 | Complete service communication paradigm change |
| MSMQ Replacement | 7 | Message queue infrastructure replacement |
| Session State Refactoring | 6 | Distributed state management implementation |
| Containerization | 5 | Standard process, well-documented |
| Azure Deployment | 6 | Azure Container Apps learning curve |
| **Overall** | **7** | **Medium-High complexity** |

---

## Risks and Mitigation Strategies

### High-Priority Risks

1. **WCF to REST/gRPC Migration Complexity**
   - **Severity:** High
   - **Impact:** Service communication may break
   - **Mitigation:** 
     - Create comprehensive API tests before migration
     - Implement adapter pattern for gradual transition
     - Maintain contract compatibility

2. **MSMQ Message Queue Data Loss**
   - **Severity:** Medium
   - **Impact:** In-flight orders may be lost
   - **Mitigation:**
     - Drain MSMQ queues before migration
     - Implement dual-write pattern during transition
     - Run parallel systems during cutover

3. **Session State Loss During Migration**
   - **Severity:** Medium
   - **Impact:** Users may lose shopping carts
   - **Mitigation:**
     - Communicate maintenance window to users
     - Implement session migration if possible
     - Accept cart loss as acceptable for modernization

### Medium-Priority Risks

4. **Container Orchestration Learning Curve**
   - **Severity:** Medium
   - **Impact:** Deployment delays
   - **Mitigation:**
     - Start with simple container setup
     - Leverage Azure Container Apps managed features
     - Invest in team training

5. **Performance Degradation**
   - **Severity:** Low-Medium
   - **Impact:** Application may be slower initially
   - **Mitigation:**
     - Implement comprehensive performance testing
     - Use Application Insights for monitoring
     - Optimize based on metrics

---

## Prerequisites and Dependencies

### Azure Resources Required

- Azure Subscription with appropriate permissions
- Azure Container Registry (for storing Docker images)
- Azure Service Bus namespace (Standard or Premium tier)
- Azure Cache for Redis (Standard or Premium tier)
- Azure Container Apps environment
- Azure Application Insights (for monitoring)
- (Optional) Azure SQL Database for persistent storage

### Development Tools

- Visual Studio 2022 or VS Code with C# extension
- .NET 10 SDK
- Docker Desktop
- Azure CLI
- Git

### Team Skills

- ASP.NET Core development
- Docker and containerization
- Azure services (Container Apps, Service Bus, Redis)
- REST API design
- CI/CD with GitHub Actions

---

## Cost Estimates (Azure Resources)

| Resource | Tier | Estimated Monthly Cost |
|----------|------|----------------------|
| Azure Container Apps | Consumption | $50-150 |
| Azure Service Bus | Standard | $10-50 |
| Azure Cache for Redis | Basic 250MB | $15-20 |
| Azure Container Registry | Basic | $5 |
| Application Insights | Pay-as-you-go | $10-30 |
| **Total** | | **$90-255/month** |

*Note: Costs vary based on usage, scaling, and data transfer*

---

## Recommendations

### High Priority

1. **Start with ProductServiceLibrary Migration**
   - This establishes the API foundation
   - Enables parallel development of web app
   - Reduces WCF dependency early

2. **Replace MSMQ Early**
   - MSMQ is incompatible with containers
   - Azure Service Bus provides better reliability
   - Enables cloud-native architecture

3. **Implement Proper Persistence**
   - Current in-memory repository won't scale
   - Consider Azure SQL or Cosmos DB
   - Enables proper data retention

### Medium Priority

4. **Use Redis for Session State from Start**
   - Essential for container orchestration
   - Enables horizontal scaling
   - Improves resilience

5. **Implement Comprehensive Logging**
   - Use ILogger throughout
   - Configure Application Insights
   - Enable distributed tracing

### Low Priority

6. **Consider API Gateway Pattern**
   - Simplifies client communication
   - Provides centralized authentication
   - Enables better routing and load balancing

7. **Add Health Checks**
   - Implement health check endpoints
   - Configure liveness and readiness probes
   - Enables better container orchestration

---

## Migration Tasks (High-Level)

The following tasks have been identified and will be created as individual GitHub issues:

1. **Migrate ProductServiceLibrary to ASP.NET Core Web API (.NET 10)**
   - Convert WCF service to REST API
   - Implement controllers and DTOs
   - Add Swagger documentation

2. **Migrate ProductCatalog to ASP.NET Core MVC (.NET 10)**
   - Port MVC 5 application to ASP.NET Core
   - Update views and controllers
   - Replace WCF client with HTTP client

3. **Replace MSMQ with Azure Service Bus**
   - Provision Azure Service Bus
   - Update OrderQueueService implementation
   - Implement retry and error handling

4. **Implement Distributed Session State with Redis**
   - Provision Azure Cache for Redis
   - Configure session state provider
   - Test session persistence

5. **Create OrderProcessor as .NET 10 Console App**
   - Create proper project structure
   - Implement Azure Service Bus consumer
   - Add logging and monitoring

6. **Containerize Applications with Docker**
   - Create Dockerfiles for all apps
   - Optimize container images
   - Test locally with Docker Compose

7. **Configure Azure Container Apps Deployment**
   - Set up Container Apps environment
   - Configure scaling and networking
   - Implement CI/CD pipeline

---

## Success Criteria

The modernization effort will be considered successful when:

1. ✅ All applications run on .NET 10
2. ✅ All components are containerized and deployable
3. ✅ WCF is replaced with REST API
4. ✅ MSMQ is replaced with Azure Service Bus
5. ✅ Session state uses distributed cache
6. ✅ Applications are deployed to Azure Container Apps
7. ✅ All existing functionality is preserved
8. ✅ Application passes performance testing
9. ✅ Monitoring and logging are operational
10. ✅ Documentation is updated

---

## Next Steps

1. Review and approve this assessment
2. Provision required Azure resources
3. Create individual GitHub issues for each migration task
4. Begin Phase 1: Foundation work
5. Set up project tracking and communication channels

---

## Appendix: References

- [ASP.NET Core Migration Guide](https://docs.microsoft.com/aspnet/core/migration/)
- [WCF to gRPC Migration](https://docs.microsoft.com/dotnet/architecture/grpc-for-wcf-developers/)
- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Azure Container Apps Documentation](https://docs.microsoft.com/azure/container-apps/)
- [.NET 10 Documentation](https://docs.microsoft.com/dotnet/)

---

**Assessment Completed:** 2026-01-13  
**Prepared for:** Brady Gaster (@bradygaster)  
**Assessment Version:** 1.0
