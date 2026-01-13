# Modernization Assessment Report
## .NET 10 & Azure Container Apps Migration

**Assessment Date:** 2026-01-13  
**Target Framework:** .NET 10  
**Target Platform:** Azure Container Apps  
**Status:** ✅ Complete

---

## Executive Summary

ProductCatalogApp is a traditional .NET Framework 4.8.1 application consisting of three components:
1. **ProductCatalog** - ASP.NET MVC 5 web application
2. **ProductServiceLibrary** - WCF service library
3. **OrderProcessor** - MSMQ-based console processor

The application demonstrates a classic three-tier architecture with Windows-specific technologies that require modernization for cloud deployment.

### Complexity Score: **7/10**

**Justification:**
- Legacy frameworks (ASP.NET MVC 5, WCF, MSMQ) require significant replacement
- Old-style project format needs conversion to SDK-style
- Windows-specific dependencies need cloud alternatives
- Moderate codebase size (~17 C# files, ~32 total files)
- Well-structured architecture simplifies migration

---

## Current Technology Stack

### ProductCatalog (Web Application)
- **Framework:** .NET Framework 4.8.1
- **Web Framework:** ASP.NET MVC 5.2.9
- **View Engine:** Razor
- **Dependencies:**
  - WCF Service Reference (ProductServiceLibrary)
  - System.Messaging (MSMQ)
  - Bootstrap 5.2.3
  - jQuery 3.7.0
  - Newtonsoft.Json 13.0.3
- **Project Format:** Old-style .csproj with package references

### ProductServiceLibrary (WCF Service)
- **Framework:** .NET Framework 4.8.1
- **Service Type:** WCF with BasicHttpBinding
- **Dependencies:**
  - System.ServiceModel
  - System.Runtime.Serialization
- **Features:**
  - Product catalog operations
  - Category management
  - SOAP-based service contracts

### OrderProcessor (Background Worker)
- **Framework:** .NET Framework 4.8.1 (inferred)
- **Type:** Console Application
- **Dependencies:**
  - System.Messaging (MSMQ)
- **Function:** 
  - Consumes orders from MSMQ queue
  - Simulates order processing workflow

---

## Detected Legacy Patterns

### 1. ASP.NET MVC 5 → ASP.NET Core MVC
**Impact:** High | **Effort:** High

The web application uses ASP.NET MVC 5, which is tightly coupled to System.Web and IIS. Migration involves:
- Converting to ASP.NET Core MVC
- Replacing System.Web dependencies
- Updating routing and middleware
- Converting Web.config to appsettings.json
- Updating view engine compatibility

### 2. WCF Service → REST API or gRPC
**Impact:** High | **Effort:** Medium

The WCF service uses SOAP/BasicHttpBinding. Modern alternatives:
- **Option A:** ASP.NET Core Web API (REST) - Recommended for simplicity
- **Option B:** gRPC - For high-performance scenarios

**Migration tasks:**
- Convert service contracts to controller actions (REST) or proto files (gRPC)
- Replace DataContract with JSON serialization or Protobuf
- Update client references in ProductCatalog

### 3. MSMQ → Azure Service Bus
**Impact:** High | **Effort:** Medium

MSMQ is Windows-specific and unavailable in containers. Replace with Azure Service Bus:
- Create Azure Service Bus namespace and queue
- Replace System.Messaging with Azure.Messaging.ServiceBus
- Update OrderQueueService to use Service Bus SDK
- Maintain transactional semantics where needed

**Alternative:** Azure Storage Queues for simpler scenarios

### 4. Old-Style Project Format → SDK-Style
**Impact:** Medium | **Effort:** Low

Convert from verbose XML .csproj to modern SDK-style:
- Simplified project files
- Implicit package references
- Better NuGet integration
- Required for .NET 10

---

## Migration Strategy

### Phase 1: Foundation (Days 1-3)
✅ **Create New .NET 10 Projects**
- Create SDK-style projects for all three components
- Set up solution structure
- Configure basic dependencies

✅ **Replace WCF with REST API**
- Create ProductCatalog.Api project (ASP.NET Core Web API)
- Port IProductService contract to API controllers
- Implement ProductRepository and data access
- Add OpenAPI/Swagger documentation

### Phase 2: Web Application (Days 4-7)
✅ **Migrate ASP.NET MVC to ASP.NET Core**
- Create new ProductCatalog.Web project
- Port controllers and views
- Update routing and middleware
- Migrate configuration to appsettings.json
- Update service references to call REST API
- Replace MSMQ with Azure Service Bus SDK

### Phase 3: Background Worker (Days 8-9)
✅ **Modernize Order Processor**
- Create .NET 10 Worker Service or hosted service
- Replace MSMQ with Azure Service Bus consumer
- Implement graceful shutdown
- Add health checks and monitoring

### Phase 4: Containerization (Days 10-12)
✅ **Docker and Azure Container Apps**
- Create Dockerfiles for each service
- Build and test containers locally
- Create Azure Container Apps environment
- Deploy to Azure Container Apps
- Configure inter-service communication
- Set up Azure Service Bus integration

### Phase 5: Validation (Days 13-15)
✅ **Testing and Optimization**
- End-to-end testing
- Performance validation
- Security review
- Documentation updates
- Production readiness checklist

---

## Detailed Project Analysis

### 1. ProductCatalog (Web Application)
**Complexity:** High  
**Files:** ~20 source files  
**Migration Path:** ASP.NET Core MVC Web App

**Key Changes Required:**
- ✅ Replace System.Web with ASP.NET Core
- ✅ Convert Global.asax to Program.cs with middleware
- ✅ Update Web.config → appsettings.json
- ✅ Replace WCF service reference with HttpClient
- ✅ Replace OrderQueueService to use Azure Service Bus
- ✅ Update NuGet packages (Bootstrap, jQuery already compatible)
- ✅ Convert old-style .csproj to SDK-style

**Migration Blockers:**
- WCF service reference needs REST/gRPC client
- MSMQ operations need Service Bus replacement
- Session state may need distributed cache
- Some System.Web APIs may need alternatives

### 2. ProductServiceLibrary (WCF Service)
**Complexity:** Medium  
**Files:** ~7 source files  
**Migration Path:** ASP.NET Core Web API

**Key Changes Required:**
- ✅ Create new Web API project
- ✅ Convert IProductService to controller(s)
- ✅ Replace [DataContract] with standard C# classes
- ✅ Implement RESTful endpoints
- ✅ Add JSON serialization
- ✅ Add health checks

**Service Contract Mapping:**
```
WCF IProductService → ProductsController (REST API)
- GetProducts() → GET /api/products
- GetProduct(id) → GET /api/products/{id}
- GetCategories() → GET /api/categories
```

### 3. OrderProcessor (Console App)
**Complexity:** Low  
**Files:** ~2 source files  
**Migration Path:** .NET Worker Service or Container App Job

**Key Changes Required:**
- ✅ Convert to Worker Service template
- ✅ Replace MSMQ with Azure Service Bus receiver
- ✅ Implement IHostedService pattern
- ✅ Add structured logging
- ✅ Add health checks

**Deployment Options:**
- **Option A:** Azure Container Apps with scale-to-zero
- **Option B:** Azure Functions with Service Bus trigger
- **Option C:** Worker Service in Container Apps

---

## Azure Container Apps Architecture

### Recommended Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                  Azure Container Apps Environment            │
│                                                              │
│  ┌──────────────────┐      ┌──────────────────┐            │
│  │  ProductCatalog  │──────│  ProductCatalog  │            │
│  │   Web (MVC)      │      │   API (REST)     │            │
│  │  Port: 8080      │      │  Port: 8080      │            │
│  └────────┬─────────┘      └────────┬─────────┘            │
│           │                         │                       │
│           │  HTTP                   │ HTTP Internal        │
│           │                         │                       │
│           └─────────┬───────────────┘                      │
│                     │                                       │
│                     │ Publishes Orders                     │
│                     ▼                                       │
│           ┌──────────────────────┐                         │
│           │  Azure Service Bus   │◄────────────┐           │
│           │  Queue: orders       │             │           │
│           └──────────────────────┘             │           │
│                     │                          │           │
│                     │ Consumes                 │           │
│                     ▼                          │           │
│           ┌──────────────────┐                 │           │
│           │  OrderProcessor  │                 │           │
│           │  (Worker)        │─────────────────┘           │
│           │  Background Job  │  Completes                  │
│           └──────────────────┘                             │
└─────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────┐
│  Supporting Azure Services                          │
│  - Azure Container Registry (images)                │
│  - Azure Application Insights (monitoring)          │
│  - Azure Key Vault (secrets)                        │
│  - Azure SQL / Cosmos DB (optional data)            │
└────────────────────────────────────────────────────┘
```

### Container Apps Configuration

**ProductCatalog.Web**
- Ingress: External HTTPS
- Scale: 1-10 replicas (HTTP requests)
- CPU: 0.5 cores
- Memory: 1 GB

**ProductCatalog.Api**
- Ingress: Internal (or External if needed)
- Scale: 1-10 replicas (HTTP requests)
- CPU: 0.5 cores
- Memory: 1 GB

**OrderProcessor**
- Ingress: None
- Scale: 0-5 replicas (Service Bus queue depth)
- CPU: 0.25 cores
- Memory: 0.5 GB

---

## Migration Roadmap

### Immediate Actions (Week 1-2)

1. **Set up .NET 10 Development Environment**
   - Install .NET 10 SDK
   - Update Visual Studio / VS Code
   - Install Docker Desktop

2. **Create New Solution Structure**
   ```
   ProductCatalogApp/
   ├── src/
   │   ├── ProductCatalog.Web/          # ASP.NET Core MVC
   │   ├── ProductCatalog.Api/          # ASP.NET Core Web API
   │   ├── ProductCatalog.Worker/       # Worker Service
   │   └── ProductCatalog.Shared/       # Shared models/contracts
   ├── docker/
   │   ├── Dockerfile.web
   │   ├── Dockerfile.api
   │   └── Dockerfile.worker
   ├── infrastructure/
   │   └── azure-container-apps.bicep   # IaC templates
   └── .github/
       └── workflows/
           └── deploy.yml               # CI/CD pipeline
   ```

3. **Provision Azure Resources**
   - Azure Container Apps Environment
   - Azure Service Bus Namespace + Queue
   - Azure Container Registry
   - Azure Application Insights

### Implementation Checklist

#### ProductCatalog.Api (REST API)
- [ ] Create ASP.NET Core Web API project (.NET 10)
- [ ] Port IProductService to ProductsController
- [ ] Port Product, Category models
- [ ] Implement ProductRepository
- [ ] Add health checks endpoint
- [ ] Add OpenAPI/Swagger
- [ ] Create Dockerfile
- [ ] Add logging and monitoring

#### ProductCatalog.Web (MVC App)
- [ ] Create ASP.NET Core MVC project (.NET 10)
- [ ] Port controllers (HomeController)
- [ ] Port views (Index, Cart, OrderConfirmation, etc.)
- [ ] Port models (CartItem, Order, OrderItem)
- [ ] Create API client service (HttpClient)
- [ ] Replace WCF service reference with API client
- [ ] Replace MSMQ with Azure Service Bus
- [ ] Port configuration to appsettings.json
- [ ] Update static files (CSS, JS, images)
- [ ] Create Dockerfile
- [ ] Add logging and monitoring

#### ProductCatalog.Worker (Order Processor)
- [ ] Create Worker Service project (.NET 10)
- [ ] Port order processing logic
- [ ] Replace MSMQ with Azure Service Bus consumer
- [ ] Implement BackgroundService
- [ ] Add health checks
- [ ] Add graceful shutdown
- [ ] Create Dockerfile
- [ ] Add logging and monitoring

#### Infrastructure & Deployment
- [ ] Create Bicep/ARM templates
- [ ] Set up Container Registry
- [ ] Build and push container images
- [ ] Deploy to Container Apps
- [ ] Configure Service Bus connection strings
- [ ] Set up Application Insights
- [ ] Configure auto-scaling rules
- [ ] Set up CI/CD pipeline

---

## Effort Estimation

| Task | Estimated Days | Notes |
|------|---------------|-------|
| **ProductCatalog.Api Migration** | 2-3 days | Replace WCF with REST API |
| **ProductCatalog.Web Migration** | 5-7 days | ASP.NET Core MVC conversion, largest component |
| **OrderProcessor Migration** | 1-2 days | Simple worker service |
| **Azure Infrastructure Setup** | 1-2 days | Container Apps, Service Bus, monitoring |
| **Containerization** | 1 day | Dockerfiles and testing |
| **Testing & Validation** | 1-2 days | E2E testing, performance validation |
| **Documentation** | 1 day | Updated README, deployment guides |
| **Buffer** | 1-2 days | Unexpected issues |
| **Total** | **10-15 days** | Single developer, full-time |

---

## Risks & Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| WCF behavior differences in REST | Medium | Medium | Thorough testing, maintain contract compatibility |
| MSMQ transactional semantics | High | Low | Use Service Bus sessions/transactions |
| View rendering differences | Low | Medium | Side-by-side testing during migration |
| Performance degradation | Medium | Low | Load testing, optimize with caching |
| Azure cost overruns | Medium | Low | Start with small scale, monitor costs |

---

## Cost Considerations (Azure)

**Estimated Monthly Cost (Development/Small Production):**

- Azure Container Apps Environment: $0 (free tier available)
- Container Apps (3 apps, minimal scale): ~$50-100/month
- Azure Service Bus (Basic): ~$10/month
- Azure Container Registry (Basic): ~$5/month
- Application Insights: ~$10-20/month (5GB included)

**Total Estimate:** ~$75-135/month for small-scale deployment

**Production Scale:** Costs scale with usage, typically $200-500/month for moderate traffic.

---

## Success Criteria

✅ All applications running on .NET 10  
✅ Deployed to Azure Container Apps  
✅ MSMQ replaced with Azure Service Bus  
✅ WCF replaced with REST API  
✅ All features working (product catalog, cart, order submission)  
✅ Order processor consuming and processing orders  
✅ Health checks implemented  
✅ Logging and monitoring in place  
✅ CI/CD pipeline operational  
✅ Documentation updated  

---

## Next Steps

1. **Review and approve this assessment**
2. **Provision Azure resources**
3. **Begin Phase 1: Create new .NET 10 projects**
4. **Set up development environment with Azure emulators**
5. **Start migration with ProductCatalog.Api** (smallest component)
6. **Iteratively migrate, test, and deploy each component**

---

## Additional Resources

- [Migrate from ASP.NET MVC to ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/migration/mvc)
- [Migrate from .NET Framework to .NET](https://learn.microsoft.com/en-us/dotnet/core/porting/)
- [Azure Container Apps documentation](https://learn.microsoft.com/en-us/azure/container-apps/)
- [Azure Service Bus .NET SDK](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues)
- [Modernize WCF applications](https://learn.microsoft.com/en-us/dotnet/core/porting/wcf)

---

**Assessment completed by:** GitHub Copilot  
**Date:** January 13, 2026  
**Version:** 1.0
