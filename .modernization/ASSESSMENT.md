# ProductCatalogApp Modernization Assessment

**Assessment Date:** 2026-01-14  
**Repository:** bradygaster/ProductCatalogApp  
**Branch:** modernize/assess  
**Complexity Score:** 7/10 (High)

---

## Executive Summary

The ProductCatalogApp is a .NET Framework 4.8.1 application consisting of an ASP.NET MVC 5 web application and a WCF service library. The application demonstrates a product catalog with shopping cart functionality and order processing via MSMQ.

**Current State:** Windows-only, legacy technology stack  
**Target State:** .NET 10, cross-platform, containerized on Azure Container Apps  
**Overall Complexity:** 7/10 (High)  
**Estimated Effort:** 3-4 weeks  

### Key Challenges

1. **WCF Service Migration** (High complexity) - WCF is not supported in modern .NET and requires complete rewrite to REST API or gRPC
2. **MSMQ Replacement** (Medium complexity) - Windows-only message queuing needs cloud-native alternative
3. **ASP.NET MVC 5 to Core** (High complexity) - Significant framework differences requiring careful migration
4. **Session State Management** (Medium complexity) - In-memory sessions need distributed cache solution

---

## Current Architecture

### Technology Stack

| Component | Technology | Version | Status |
|-----------|-----------|---------|--------|
| Framework | .NET Framework | 4.8.1 | ❌ Legacy |
| Web Framework | ASP.NET MVC | 5.2.9 | ❌ Legacy |
| Service Layer | WCF | 4.8.1 | ❌ Not supported in .NET Core |
| Messaging | MSMQ | System.Messaging | ❌ Windows-only |
| Project Format | Legacy .csproj | - | ❌ Legacy |
| Package Management | packages.config | - | ❌ Legacy |
| Data Storage | In-Memory | - | ❌ Not production-ready |
| Configuration | Web.config | - | ❌ Legacy |

### Project Structure

```
ProductCatalogApp/
├── ProductCatalog/                    # ASP.NET MVC 5 Web Application
│   ├── Controllers/
│   │   └── HomeController.cs         # Product listing, cart, orders
│   ├── Models/
│   │   ├── CartItem.cs
│   │   └── Order.cs
│   ├── Services/
│   │   └── OrderQueueService.cs      # MSMQ integration
│   ├── Views/                        # Razor views
│   ├── Connected Services/
│   │   └── ProductServiceReference/  # WCF client
│   └── Web.config                    # Configuration
│
├── ProductServiceLibrary/             # WCF Service Library
│   ├── IProductService.cs            # WCF service contract
│   ├── ProductService.cs             # Service implementation
│   ├── ProductRepository.cs          # In-memory data store
│   ├── Product.cs                    # Data model
│   └── Category.cs                   # Data model
│
└── ProductCatalogApp.slnx            # Solution file
```

### Dependencies Analysis

**ProductCatalog Project** (16 NuGet packages):
- Microsoft.AspNet.Mvc 5.2.9
- Newtonsoft.Json 13.0.3
- jQuery 3.7.0
- bootstrap 5.2.3
- System.Messaging (MSMQ)
- System.Web.* (Multiple)

**ProductServiceLibrary Project** (3 core references):
- System.ServiceModel (WCF)
- System.Runtime.Serialization
- System.Core

---

## Legacy Components & Modernization Path

### Critical Priority

#### 1. WCF Services → REST API / gRPC

**Current State:**
- WCF service with 9 operations (GetAllProducts, GetProductById, etc.)
- ServiceContract/OperationContract attributes
- BasicHttpBinding configuration
- Synchronous request/response pattern

**Issues:**
- WCF is not supported in .NET 5+ (CoreWCF exists but adds complexity)
- Windows-only hosting
- Heavyweight protocol
- Not cloud-native

**Modernization Path:**
- **Option A:** ASP.NET Core Web API (REST)
  - Simpler to implement
  - Better HTTP integration
  - OpenAPI/Swagger support
  - Standard JSON serialization
- **Option B:** gRPC
  - High performance
  - Strong typing
  - Streaming support
  - Better for microservices

**Recommended:** ASP.NET Core Web API for simplicity and ecosystem support

**Complexity:** 9/10

#### 2. MSMQ → Azure Service Bus

**Current State:**
- OrderQueueService uses System.Messaging
- Private queue: `.\Private$\ProductCatalogOrders`
- XML message serialization
- Fire-and-forget pattern for orders

**Issues:**
- Windows-only (not available in Linux containers)
- Not cloud-native
- No built-in retry or dead-letter handling
- Requires MSMQ Windows feature

**Modernization Path:**
- **Azure Service Bus** (Recommended)
  - Fully managed
  - Cross-platform client libraries
  - Built-in retry, dead-letter, scheduled messages
  - Integrates with Azure ecosystem
- **Alternatives:** RabbitMQ, Azure Storage Queues, Kafka

**Complexity:** 7/10

#### 3. ASP.NET MVC 5 → ASP.NET Core MVC

**Current State:**
- ASP.NET MVC 5.2.9 with Razor views
- System.Web dependencies throughout
- Global.asax for application lifecycle
- Web.config for configuration
- In-memory session state

**Issues:**
- System.Web not available in .NET Core
- Different middleware pipeline
- Configuration system changed
- Hosting model changed
- No cross-platform support

**Modernization Path:**
- Create new ASP.NET Core MVC project
- Port controllers and views
- Replace System.Web.Mvc with Microsoft.AspNetCore.Mvc
- Migrate Global.asax logic to Program.cs
- Convert Web.config to appsettings.json
- Implement distributed session state (Redis)

**Complexity:** 8/10

### Moderate Priority

#### 4. Session State → Distributed Cache

**Current State:**
- In-memory session for shopping cart
- `Session["Cart"]` stores CartItem list
- Tied to single server

**Issues:**
- Not suitable for containerized environment
- No persistence across restarts
- Can't scale horizontally

**Modernization Path:**
- Azure Redis Cache for distributed sessions
- Consider moving cart to client-side storage (localStorage + API)
- Implement IDistributedCache interface

**Complexity:** 6/10

#### 5. Project Format → SDK-Style

**Current State:**
- Legacy .csproj format with verbose XML
- packages.config for NuGet packages
- Manual assembly references

**Modernization Path:**
- Convert to SDK-style projects
- Use PackageReference instead of packages.config
- Simplify project files (90% reduction in size)
- Tools: `dotnet try-convert` or manual migration

**Complexity:** 5/10

#### 6. Configuration → appsettings.json

**Current State:**
- Web.config for all configuration
- AppSettings, ConnectionStrings sections
- System.Configuration API

**Modernization Path:**
- Create appsettings.json and appsettings.Development.json
- Use IConfiguration interface
- Support environment variables for containers
- Azure Key Vault integration for secrets

**Complexity:** 5/10

### Minor Priority

#### 7. Global.asax → Program.cs

**Current State:**
- Application_Start for route registration, bundle config
- Global error handling in Global.asax

**Modernization Path:**
- Move to Program.cs with minimal hosting model
- Use middleware pipeline for error handling
- Configure services in dependency injection

**Complexity:** 4/10

---

## Target Architecture for Azure Container Apps

### Proposed Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Azure Container Apps                     │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌────────────────────────────────────────────────────┐     │
│  │  product-catalog-web (ASP.NET Core MVC)            │     │
│  │  - .NET 10                                          │     │
│  │  - Razor views                                      │     │
│  │  - Calls Product API                                │     │
│  │  - Sends orders to Service Bus                      │     │
│  └────────────────────────────────────────────────────┘     │
│                          │                                   │
│                          │ HTTP                              │
│                          ▼                                   │
│  ┌────────────────────────────────────────────────────┐     │
│  │  product-catalog-api (ASP.NET Core Web API)        │     │
│  │  - .NET 10                                          │     │
│  │  - REST endpoints                                   │     │
│  │  - Entity Framework Core                            │     │
│  │  - Connects to Azure SQL                            │     │
│  └────────────────────────────────────────────────────┘     │
│                                                               │
│  ┌────────────────────────────────────────────────────┐     │
│  │  product-catalog-worker (Background Service)       │     │
│  │  - .NET 10                                          │     │
│  │  - Processes orders from Service Bus               │     │
│  │  - Updates database                                 │     │
│  └────────────────────────────────────────────────────┘     │
│                                                               │
└─────────────────────────────────────────────────────────────┘
                          │
                          │
          ┌───────────────┼───────────────┐
          │               │               │
          ▼               ▼               ▼
   ┌──────────┐   ┌──────────────┐   ┌──────────┐
   │ Azure    │   │ Azure        │   │ Azure    │
   │ Redis    │   │ Service Bus  │   │ SQL DB   │
   │ Cache    │   │              │   │          │
   └──────────┘   └──────────────┘   └──────────┘
```

### Container Apps

1. **product-catalog-web**
   - Type: ASP.NET Core MVC
   - Port: 8080
   - Scaling: 1-10 replicas based on HTTP requests
   - Health: /health endpoint

2. **product-catalog-api**
   - Type: ASP.NET Core Web API
   - Port: 8080
   - Scaling: 2-20 replicas based on CPU/requests
   - Health: /health endpoint

3. **product-catalog-worker**
   - Type: Background Service
   - Scaling: 1-5 replicas based on Service Bus queue length
   - Health: Custom health check

### Azure Services

1. **Azure Service Bus** (Standard/Premium)
   - Queue: order-queue
   - Dead-letter queue for failed orders
   - Session support if needed

2. **Azure Redis Cache** (Basic/Standard)
   - Session state storage
   - Output caching
   - Distributed locks

3. **Azure SQL Database** (Serverless or DTU-based)
   - Products table
   - Categories table
   - Orders table
   - OrderItems table

4. **Azure Application Insights**
   - Distributed tracing
   - Performance monitoring
   - Exception tracking

---

## Complexity Analysis

### Overall Complexity Score: 7/10 (High)

**Breakdown by Area:**

| Area | Score | Reasoning |
|------|-------|-----------|
| Framework Migration | 8/10 | .NET Framework to .NET 10 is significant; System.Web removal is challenging |
| WCF to Modern API | 9/10 | Complete rewrite required; no direct migration path |
| MSMQ Replacement | 7/10 | Different programming model; requires architectural changes |
| Session State Refactoring | 6/10 | Straightforward Redis integration but requires testing |
| Project Structure | 5/10 | Mechanical conversion with some manual adjustments |
| Business Logic | 4/10 | Simple domain logic; minimal changes needed |
| UI Modernization | 5/10 | Razor views are similar; some syntax changes required |
| Containerization | 6/10 | Standard Dockerfile creation; configuration management |

**Factors Reducing Complexity:**
- Small codebase (~1,600 lines of code)
- Simple business logic
- No complex authentication/authorization
- No real-time features (SignalR)
- Modern client-side libraries already in use (Bootstrap 5, jQuery 3.7)

**Factors Increasing Complexity:**
- WCF requires complete replacement
- MSMQ has no direct equivalent
- Heavy System.Web dependencies
- Session state used extensively
- Windows-specific technologies

---

## Migration Strategy

### Approach: Incremental Rewrite with Parallel Run

**Phase 1: Foundation** (3-4 days)
- ✅ Create new .NET 10 solution structure
- ✅ Set up SDK-style projects
- ✅ Configure CI/CD pipeline
- ✅ Add Entity Framework Core and data models
- ✅ Set up Azure resources (SQL, Redis, Service Bus)

**Phase 2: Data Layer** (2-3 days)
- ✅ Design database schema
- ✅ Create EF Core DbContext
- ✅ Implement repository pattern
- ✅ Add migrations
- ✅ Seed sample data

**Phase 3: Service Layer** (5-7 days)
- ✅ Create ASP.NET Core Web API project
- ✅ Implement product endpoints (GET, POST, PUT, DELETE)
- ✅ Add category endpoints
- ✅ Implement search and filtering
- ✅ Add OpenAPI/Swagger documentation
- ✅ Deploy as Container App
- ⚠️ **Keep WCF service running temporarily**

**Phase 4: Web Application** (5-7 days)
- ✅ Create ASP.NET Core MVC project
- ✅ Port controllers to ASP.NET Core
- ✅ Migrate Razor views
- ✅ Update view models and validation
- ✅ Implement distributed session with Redis
- ✅ Configure authentication (if needed)
- ✅ Migrate static files and bundles
- ✅ Deploy as Container App
- ⚠️ **Test both old and new web apps**

**Phase 5: Messaging** (3-4 days)
- ✅ Create Azure Service Bus queue
- ✅ Implement order message models
- ✅ Create OrderQueueService for Service Bus
- ✅ Create background worker service
- ✅ Implement order processing logic
- ✅ Add retry and error handling
- ✅ Deploy worker as Container App

**Phase 6: Containerization & Deployment** (3-5 days)
- ✅ Create Dockerfiles for all services
- ✅ Set up Azure Container Registry
- ✅ Configure Container Apps environment
- ✅ Set up networking and ingress
- ✅ Configure auto-scaling rules
- ✅ Implement health checks and readiness probes
- ✅ Set up Application Insights
- ✅ Configure Key Vault for secrets
- ✅ Deploy and smoke test
- ✅ Load test and optimize

**Phase 7: Cutover** (2-3 days)
- ✅ Final testing
- ✅ Update DNS/routing
- ✅ Monitor and validate
- ✅ Decommission old services
- ✅ Document new architecture

---

## Estimated Effort

**Total Time:** 3-4 weeks (15-20 working days)

| Phase | Duration | Complexity |
|-------|----------|------------|
| Foundation | 3-4 days | Low |
| Data Layer | 2-3 days | Low-Medium |
| Service Layer | 5-7 days | High |
| Web Application | 5-7 days | High |
| Messaging | 3-4 days | Medium |
| Containerization | 3-5 days | Medium |
| Cutover | 2-3 days | Low |

**Assumptions:**
- 1 full-time developer
- Access to Azure subscription
- No major scope changes
- Existing functionality preserved
- Basic testing only (not comprehensive QA)

---

## Risks & Mitigation

### High Impact Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| WCF feature parity not achieved | High | Medium | Document all WCF features; verify REST API covers all scenarios; consider CoreWCF as fallback |
| Service Bus message format incompatible | High | Low | Design messages to be backward compatible; implement schema versioning |
| Performance degradation | High | Low | Load test early; profile and optimize; use Application Insights |

### Medium Impact Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Session state issues in distributed env | Medium | Medium | Implement Redis early; test thoroughly; consider client-side storage |
| Entity Framework performance issues | Medium | Low | Use proper indexing; implement caching; use compiled queries |
| Container startup time issues | Medium | Low | Optimize Docker images; use multi-stage builds; pre-warm containers |

### Low Impact Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Breaking changes in .NET 10 | Low | Low | Follow migration guides; test incrementally |
| Azure cost overruns | Medium | Low | Use cost management tools; set budgets and alerts |

---

## Recommendations

### Technical Recommendations

1. **Start with API layer** - Build the foundation first; this is the most complex part
2. **Add database early** - Avoid data loss; enables proper testing
3. **Use feature flags** - Control rollout; enable A/B testing
4. **Implement comprehensive logging** - Application Insights from day one
5. **Use Dapr (optional)** - Abstracts messaging and state management; portable across clouds
6. **Consider API versioning** - Plan for future changes; use URL or header versioning
7. **Implement Circuit Breaker pattern** - Handle service failures gracefully
8. **Use Health Checks** - Essential for Container Apps orchestration

### Process Recommendations

1. **Set up CI/CD early** - Automate builds and deployments
2. **Test incrementally** - Don't wait until the end
3. **Document as you go** - Architecture decisions, API contracts
4. **Plan for rollback** - Keep old services running until confident
5. **Involve stakeholders** - Regular demos and feedback
6. **Monitor proactively** - Set up alerts before going live

### Azure Container Apps Recommendations

1. **Use managed identity** - Avoid connection strings where possible
2. **Configure auto-scaling** - Based on CPU, memory, or custom metrics
3. **Use KEDA scalers** - Service Bus queue length for worker
4. **Implement proper health checks** - Startup, liveness, and readiness
5. **Use Application Insights** - Built-in integration
6. **Consider Dapr components** - State store, pub/sub, service invocation

---

## Benefits

### Technical Benefits

- ✅ **Cross-platform deployment** - Run on Linux containers (lower cost)
- ✅ **Modern development experience** - Latest C# features, better tooling
- ✅ **Better performance** - .NET 10 is significantly faster than .NET Framework
- ✅ **Cloud-native architecture** - Built for containers and microservices
- ✅ **Auto-scaling** - Handle traffic spikes automatically
- ✅ **Improved security** - Latest security patches and features
- ✅ **Better observability** - Built-in metrics, logs, traces

### Business Benefits

- 💰 **Lower infrastructure costs** - Linux containers + consumption-based pricing
- 🚀 **Faster deployment cycles** - CI/CD with containers
- 📈 **Better availability** - Auto-healing, zero-downtime deployments
- 👥 **Easier hiring** - Modern stack attracts developers
- 🔮 **Future-proof** - Active development, long-term support
- ☁️ **Cloud benefits** - Global distribution, disaster recovery
- 🔧 **Reduced maintenance** - Managed services, automated updates

### Cost Comparison (Estimated)

**Current (On-Premises/VM):**
- Windows Server licenses: $$$
- IIS hosting
- MSMQ infrastructure
- Manual scaling

**Future (Azure Container Apps):**
- No Windows licenses needed
- Pay-per-use pricing
- Managed infrastructure
- Auto-scaling included

**Estimated Savings:** 30-50% on infrastructure costs

---

## Next Steps

1. ✅ **Review and approve this assessment** with stakeholders
2. ⏭️ **Create detailed migration plan** based on phases outlined above
3. ⏭️ **Set up Azure environment** - Subscriptions, resource groups, services
4. ⏭️ **Create proof of concept** - Single endpoint migration to validate approach
5. ⏭️ **Begin Phase 1: Foundation** - Set up project structure and CI/CD

---

## Conclusion

The ProductCatalogApp is a good candidate for modernization to .NET 10 and Azure Container Apps. While the complexity is relatively high (7/10) due to WCF and MSMQ dependencies, the small codebase and straightforward business logic make it manageable. The estimated effort of 3-4 weeks is reasonable, and the benefits in terms of cost savings, performance, and maintainability are significant.

The recommended approach is an incremental migration starting with the service layer, followed by the web application, then messaging, and finally containerization. This allows for testing at each stage and reduces risk.

**Readiness for Azure Container Apps:** Not ready in current state; requires modernization first.  
**Recommended Timeline:** 3-4 weeks for complete migration  
**Investment:** Medium effort, High return

---

*This assessment was generated on 2026-01-14 for the ProductCatalogApp modernization initiative.*
