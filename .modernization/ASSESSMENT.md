# Modernization Assessment: ProductCatalogApp

**Assessment Date:** 2026-01-11  
**Repository:** bradygaster/ProductCatalogApp  
**Target Framework:** .NET 10  
**Target Platform:** Azure Container Apps  
**Status:** ✅ Complete

---

## Executive Summary

The ProductCatalogApp is a .NET Framework 4.8.1 application consisting of three main components: an ASP.NET MVC 5 web frontend, a WCF service library for product data, and an MSMQ-based order processor. The application demonstrates a traditional Windows-based enterprise architecture that requires significant modernization to run on .NET 10 and Azure Container Apps.

**Overall Complexity: 7/10** (Moderate to High)

**Estimated Effort:** 3-4 weeks for 2 developers (~55 story points)

---

## Current Architecture

### Components

1. **ProductCatalog** - ASP.NET MVC 5 Web Application
   - Framework: .NET Framework 4.8.1
   - UI: Razor views with Bootstrap 5.2.3
   - Dependencies: System.Web, ASP.NET MVC 5.2.9
   - Features: Product catalog browsing, shopping cart, order submission
   - Session: In-memory (HttpContext.Session)

2. **ProductServiceLibrary** - WCF Service Library
   - Framework: .NET Framework 4.8.1
   - Protocol: SOAP/WCF
   - Purpose: Product CRUD operations
   - Data: In-memory repository (mock data)

3. **OrderProcessor** - Console Application
   - Framework: .NET Framework 4.8.1
   - Messaging: MSMQ (Microsoft Message Queue)
   - Purpose: Asynchronous order processing
   - Platform: Windows-only

### Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | .NET Framework | 4.8.1 |
| Web Framework | ASP.NET MVC | 5.2.9 |
| Service Framework | WCF | Built-in |
| Messaging | MSMQ | Windows Feature |
| UI Library | Bootstrap | 5.2.3 |
| JavaScript | jQuery | 3.7.0 |
| JSON | Newtonsoft.Json | 13.0.3 |

---

## Legacy Patterns & Blockers

### High Impact Issues

#### 1. Windows Communication Foundation (WCF)
**Location:** ProductServiceLibrary  
**Impact:** High  
**Issue:** WCF is not fully supported in .NET Core/.NET 5+. CoreWCF exists but REST APIs are preferred for modern applications.

**Migration Path:**
- Convert WCF service to ASP.NET Core Web API
- Replace SOAP with REST/JSON endpoints
- Update client to use HttpClient instead of WCF proxy
- Consider gRPC for high-performance scenarios

#### 2. Microsoft Message Queue (MSMQ)
**Location:** OrderProcessor, ProductCatalog/Services/OrderQueueService.cs  
**Impact:** High  
**Issue:** MSMQ is Windows-only and cannot run in Linux containers. Not available in Azure PaaS services.

**Migration Path:**
- Replace with Azure Service Bus for enterprise messaging
- Alternative: Azure Storage Queues for simpler scenarios
- Update OrderQueueService to use Azure SDK
- Migrate OrderProcessor to Worker Service consuming from Azure Service Bus

#### 3. ASP.NET MVC 5 with System.Web
**Location:** ProductCatalog  
**Impact:** High  
**Issue:** System.Web is tied to IIS and Windows, not compatible with .NET Core/.NET 5+.

**Migration Path:**
- Migrate to ASP.NET Core MVC
- Replace System.Web.Mvc with Microsoft.AspNetCore.Mvc
- Update Razor views (minimal changes needed)
- Replace HttpContext.Session with distributed session state
- Move from IIS to Kestrel web server

### Medium Impact Issues

#### 4. Legacy .csproj Format
**Location:** All projects  
**Impact:** Medium  
**Issue:** Old-style MSBuild project files are verbose and less maintainable.

**Migration Path:**
- Convert to SDK-style projects using .NET Upgrade Assistant
- Simpler project files with implicit references
- Better NuGet package management

#### 5. In-Memory Session State
**Location:** ProductCatalog/Controllers/HomeController.cs  
**Impact:** Medium  
**Issue:** In-memory session state won't work in containerized, multi-instance environments.

**Migration Path:**
- Implement Azure Cache for Redis for distributed session
- Use ASP.NET Core distributed session middleware
- Store cart data in Redis with session key
- Consider stateless JWT-based approach for API scenarios

### Low Impact Issues

#### 6. NuGet packages.config
**Location:** ProductCatalog  
**Impact:** Low  
**Issue:** Legacy package management format.

**Migration Path:**
- Automatically migrated with SDK-style projects
- Packages become PackageReference in .csproj

---

## Modernization Requirements

### Phase 1: Foundation (Week 1)

**Objective:** Setup modern development environment and project structure

✅ **Tasks:**
1. Install .NET 10 SDK
2. Create new solution with SDK-style projects
3. Setup Git branching strategy for migration
4. Configure CI/CD pipeline for .NET 10
5. Create development/staging/production environments
6. Setup Azure resources (Container Apps, Service Bus, Redis)

**Deliverables:**
- New solution structure
- CI/CD pipeline configured
- Azure infrastructure provisioned

---

### Phase 2: Service Layer (Week 2)

**Objective:** Modernize ProductServiceLibrary from WCF to REST API

✅ **Tasks:**
1. Create new ASP.NET Core Web API project (ProductServiceAPI)
2. Migrate Product, Category, and ProductRepository classes
3. Implement REST endpoints:
   - GET /api/products - Get all products
   - GET /api/products/{id} - Get product by ID
   - GET /api/products/category/{category} - Get products by category
   - GET /api/products/search?term={term} - Search products
   - GET /api/categories - Get all categories
   - POST /api/products - Create product
   - PUT /api/products/{id} - Update product
   - DELETE /api/products/{id} - Delete product
4. Add Swagger/OpenAPI documentation
5. Implement proper error handling and logging
6. Add health check endpoints
7. Create Dockerfile for ProductServiceAPI
8. Test API with Postman/curl

**Deliverables:**
- ProductServiceAPI running on .NET 10
- Swagger documentation
- Docker image
- Integration tests

---

### Phase 3: Web Application (Week 3)

**Objective:** Migrate ASP.NET MVC 5 to ASP.NET Core MVC

✅ **Tasks:**
1. Create new ASP.NET Core MVC project (ProductCatalog.Web)
2. Migrate models (CartItem, Order, OrderItem)
3. Migrate views to ASP.NET Core format:
   - Update _ViewStart.cshtml
   - Update _Layout.cshtml
   - Migrate Index, Cart, About, Contact views
   - Update tag helpers and HTML helpers
4. Migrate controllers (HomeController)
5. Replace WCF client with HttpClient:
   - Create IProductService interface
   - Implement HttpProductService client
   - Configure HttpClient with DI
6. Implement distributed session with Redis:
   - Add Microsoft.Extensions.Caching.StackExchangeRedis
   - Configure Redis connection
   - Update session configuration
   - Test cart persistence across restarts
7. Update static files (CSS, JS, images)
8. Configure app settings and environment variables
9. Create Dockerfile for web application
10. Test end-to-end functionality

**Deliverables:**
- ProductCatalog.Web running on .NET 10
- Redis-backed session state
- HTTP-based service communication
- Docker image
- UI/UX unchanged from user perspective

---

### Phase 4: Messaging & Backend (Week 4)

**Objective:** Replace MSMQ with Azure Service Bus and modernize OrderProcessor

✅ **Tasks:**
1. Setup Azure Service Bus namespace and queue
2. Create new Worker Service project (OrderProcessor.Worker)
3. Implement Azure Service Bus message handling:
   - Add Azure.Messaging.ServiceBus NuGet package
   - Create ServiceBusOrderService
   - Implement message serialization/deserialization
   - Add error handling and dead-letter queue handling
4. Update ProductCatalog.Web to send orders to Service Bus:
   - Replace OrderQueueService with ServiceBusOrderService
   - Update configuration for Service Bus connection string
5. Implement OrderProcessor.Worker:
   - Create BackgroundService for message processing
   - Implement order processing logic
   - Add structured logging
   - Add health checks
6. Create Dockerfile for OrderProcessor.Worker
7. Deploy all services to Azure Container Apps:
   - Create Container App for ProductServiceAPI
   - Create Container App for ProductCatalog.Web
   - Create Container App for OrderProcessor.Worker
   - Configure ingress for web and API
   - Configure environment variables and secrets
   - Setup service-to-service communication
   - Configure auto-scaling rules
8. Setup monitoring:
   - Application Insights for all services
   - Log Analytics workspace
   - Custom dashboards and alerts

**Deliverables:**
- OrderProcessor.Worker running on .NET 10
- Azure Service Bus integration
- All services deployed to Azure Container Apps
- Monitoring and alerting configured
- Complete end-to-end testing

---

## Technical Migration Details

### WCF to REST API Migration

**Current WCF Contract:**
```csharp
[ServiceContract]
public interface IProductService
{
    [OperationContract]
    List<Product> GetAllProducts();
    
    [OperationContract]
    Product GetProductById(int productId);
    
    // ... other operations
}
```

**New REST API Controller:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Product>> GetAllProducts()
    
    [HttpGet("{id}")]
    public ActionResult<Product> GetProductById(int id)
    
    // ... other endpoints
}
```

### MSMQ to Azure Service Bus Migration

**Current MSMQ Code:**
```csharp
using System.Messaging;

var queue = new MessageQueue(@".\Private$\ProductCatalogOrders");
queue.Send(order);
```

**New Azure Service Bus Code:**
```csharp
using Azure.Messaging.ServiceBus;

var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender(queueName);
var message = new ServiceBusMessage(JsonSerializer.Serialize(order));
await sender.SendMessageAsync(message);
```

### Session State Migration

**Current In-Memory Session:**
```csharp
Session["Cart"] = cart;
var cart = Session["Cart"] as List<CartItem>;
```

**New Distributed Session:**
```csharp
// Startup configuration
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration["Redis:ConnectionString"];
});
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Controller usage (same API)
HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
var cart = JsonSerializer.Deserialize<List<CartItem>>(
    HttpContext.Session.GetString("Cart"));
```

---

## Complexity Analysis

### Overall Complexity: 7/10

**Justification:** This is a moderate-to-high complexity migration due to the need to replace several legacy Windows-specific technologies (WCF, MSMQ, System.Web). However, the application logic is straightforward, and there are well-established patterns for each migration step.

### Complexity Breakdown

| Component | Score | Reasoning |
|-----------|-------|-----------|
| Framework Migration | 6/10 | Standard .NET Framework to .NET 10 migration path |
| WCF Replacement | 7/10 | Requires new API implementation and client updates |
| MSMQ Replacement | 8/10 | Architectural change with cloud messaging patterns |
| MVC Migration | 6/10 | Well-documented process, views mostly compatible |
| Containerization | 5/10 | Standard Docker containerization |
| Azure Deployment | 6/10 | Container Apps setup is straightforward |

### Effort Estimation

- **Small Tasks:** 15 (1-2 story points each) = ~20 points
- **Medium Tasks:** 8 (3-5 story points each) = ~30 points  
- **Large Tasks:** 5 (1-2 days each) = ~5 points

**Total:** ~55 story points

**Timeline:** 3-4 weeks for 2 developers working in parallel

---

## Risks & Mitigation Strategies

### High Risk Items

#### 1. WCF to REST API Migration
**Risk:** Breaking changes in API contract could impact clients  
**Severity:** High  
**Mitigation:**
- Design REST API to match WCF operations closely
- Implement comprehensive integration tests
- Consider parallel deployment if external clients exist
- Use API versioning from the start

#### 2. MSMQ to Azure Service Bus
**Risk:** Message delivery semantics may differ, potential message loss  
**Severity:** High  
**Mitigation:**
- Study Service Bus message patterns thoroughly
- Implement proper error handling and retry logic
- Use transactions where necessary
- Test failover scenarios extensively
- Setup dead-letter queue monitoring

### Medium Risk Items

#### 3. Session State in Distributed Environment
**Risk:** Cart data loss if Redis is unavailable  
**Severity:** Medium  
**Mitigation:**
- Implement Redis with high availability (cluster/replica)
- Add graceful degradation (fallback to in-memory for dev)
- Test Redis failover scenarios
- Monitor Redis health and performance

#### 4. Breaking Changes in .NET 10
**Risk:** APIs or behaviors may have changed  
**Severity:** Medium  
**Mitigation:**
- Use .NET Upgrade Assistant to identify issues
- Follow official migration guides
- Implement comprehensive test suite
- Test on .NET 10 early and often

### Low Risk Items

#### 5. Container Orchestration Complexity
**Risk:** Learning curve for Container Apps  
**Severity:** Low  
**Mitigation:**
- Start with simple configurations
- Use Azure CLI and Infrastructure as Code
- Leverage Azure documentation and samples
- Implement monitoring early

---

## Recommended Migration Strategy

### Strategy: Incremental Migration

**Rationale:** Minimize risk by migrating components independently while maintaining backward compatibility where possible.

### Migration Order

1. **Service Layer First** - Convert WCF to REST API
   - Independent component, can be tested in isolation
   - Web app can temporarily use both WCF and REST during transition
   - Provides immediate value (modern API)

2. **Web Application Second** - Migrate ASP.NET MVC to Core
   - Can consume new REST API immediately
   - Session state migration is isolated change
   - User-facing, requires thorough testing

3. **Messaging Layer Last** - Replace MSMQ with Service Bus
   - Most complex architectural change
   - Benefits from other migrations being complete
   - Requires both frontend and backend changes

### Testing Strategy

Each phase should include:
- ✅ Unit tests for business logic
- ✅ Integration tests for external dependencies
- ✅ End-to-end tests for critical user journeys
- ✅ Performance tests under load
- ✅ Security testing (OWASP top 10)

---

## Azure Container Apps Architecture

### Proposed Architecture

```
┌─────────────────────────────────────────────────────┐
│  Azure Container Apps Environment                   │
│                                                      │
│  ┌────────────────────┐                            │
│  │ ProductCatalog.Web │  ← HTTPS Ingress (Public)  │
│  │ (ASP.NET Core MVC) │                            │
│  └──────────┬─────────┘                            │
│             │ HTTP                                  │
│  ┌──────────▼─────────┐                            │
│  │ ProductServiceAPI  │  ← HTTPS Ingress (Internal)│
│  │ (ASP.NET Core API) │                            │
│  └────────────────────┘                            │
│                                                      │
│  ┌─────────────────────┐                           │
│  │ OrderProcessor      │  ← No Ingress (Background)│
│  │ (Worker Service)    │                           │
│  └──────────┬──────────┘                           │
└─────────────┼───────────────────────────────────────┘
              │
    ┌─────────┴─────────┐
    │                   │
┌───▼────────────┐  ┌──▼────────────────┐
│ Azure Service  │  │ Azure Cache       │
│ Bus            │  │ for Redis         │
└────────────────┘  └───────────────────┘
```

### Container Apps Configuration

**ProductCatalog.Web:**
- Ingress: Enabled (External)
- Min Replicas: 1
- Max Replicas: 5
- Target Port: 8080
- CPU: 0.5 cores
- Memory: 1 GB
- Environment Variables:
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `ProductServiceAPI__BaseUrl=https://product-api.internal`
  - `Redis__ConnectionString=<redis-connection>`
  - `ServiceBus__ConnectionString=<sb-connection>`

**ProductServiceAPI:**
- Ingress: Enabled (Internal only)
- Min Replicas: 1
- Max Replicas: 3
- Target Port: 8080
- CPU: 0.25 cores
- Memory: 0.5 GB

**OrderProcessor.Worker:**
- Ingress: Disabled
- Min Replicas: 1
- Max Replicas: 2
- CPU: 0.25 cores
- Memory: 0.5 GB
- Environment Variables:
  - `ServiceBus__ConnectionString=<sb-connection>`
  - `ServiceBus__QueueName=orders`

---

## Benefits of Modernization

### Technical Benefits

✅ **Cross-Platform Compatibility**
- Run on Linux containers (lower cost)
- No dependency on Windows Server
- Portable across cloud providers

✅ **Performance Improvements**
- .NET 10 is significantly faster than .NET Framework
- Reduced memory footprint
- Better throughput and latency

✅ **Cloud-Native Architecture**
- Auto-scaling based on demand
- Zero-downtime deployments
- Built-in load balancing
- Automatic SSL/TLS certificates

✅ **Developer Productivity**
- Modern C# language features
- Better tooling and IDE support
- Faster build and deployment cycles
- Hot reload and live debugging

✅ **Observability**
- Application Insights integration
- Structured logging
- Distributed tracing
- Custom metrics and dashboards

### Business Benefits

✅ **Cost Reduction**
- No Windows Server licensing costs
- Pay-per-use pricing model
- Efficient resource utilization with auto-scaling
- Reduced operational overhead

✅ **Improved Scalability**
- Automatic scaling based on HTTP requests or queue depth
- Handle traffic spikes without manual intervention
- Global distribution capabilities

✅ **Faster Time to Market**
- Streamlined CI/CD pipelines
- Faster iteration cycles
- Easy rollback capabilities

✅ **Future-Proof Technology**
- Long-term support from Microsoft
- Active community and ecosystem
- Regular updates and security patches

---

## Current Blockers for Container Deployment

### Hard Blockers (Must Fix)

❌ **MSMQ Dependency**
- Windows-only technology
- Cannot run in Linux containers
- Not available in Azure PaaS
- **Solution:** Migrate to Azure Service Bus

❌ **System.Web Dependencies**
- Tied to IIS and Windows
- Not available in .NET Core/.NET 5+
- **Solution:** Migrate to ASP.NET Core

❌ **WCF Service**
- Limited .NET Core support
- Not ideal for containerized services
- **Solution:** Migrate to REST API

### Soft Blockers (Should Fix)

⚠️ **In-Memory Session State**
- Won't persist across container restarts
- Won't work with multiple instances
- **Solution:** Use Azure Cache for Redis

⚠️ **Legacy Project Format**
- Difficult to containerize
- Complex dependencies
- **Solution:** Convert to SDK-style projects

---

## Success Criteria

### Phase 1 Success
- [ ] .NET 10 SDK installed and working
- [ ] Azure resources provisioned
- [ ] CI/CD pipeline operational

### Phase 2 Success
- [ ] ProductServiceAPI running on .NET 10
- [ ] All WCF operations available as REST endpoints
- [ ] Swagger documentation accessible
- [ ] API containerized and deployable

### Phase 3 Success
- [ ] ProductCatalog.Web running on .NET 10
- [ ] All pages functional and styled correctly
- [ ] Cart persists across app restarts (Redis)
- [ ] API communication working
- [ ] Web app containerized and deployable

### Phase 4 Success
- [ ] OrderProcessor.Worker running on .NET 10
- [ ] Azure Service Bus integration working
- [ ] Orders flow from web → Service Bus → processor
- [ ] All services deployed to Azure Container Apps
- [ ] Monitoring and alerting configured
- [ ] End-to-end testing passed

### Final Validation
- [ ] Application runs entirely on .NET 10
- [ ] All services running in Azure Container Apps
- [ ] No Windows dependencies remain
- [ ] Auto-scaling verified under load
- [ ] Monitoring dashboards showing healthy metrics
- [ ] User acceptance testing passed

---

## Next Steps

1. **Review and Approve Assessment** - Stakeholder sign-off on approach and timeline
2. **Provision Azure Resources** - Create Container Apps environment, Service Bus, Redis
3. **Setup Development Environment** - Install .NET 10, Azure CLI, Docker Desktop
4. **Create Backlog** - Break down phases into individual tasks/stories
5. **Begin Phase 1** - Start with foundation and infrastructure
6. **Regular Checkpoints** - Weekly reviews to track progress and adjust plan

---

## Resources & References

### Official Microsoft Documentation
- [Migrate from .NET Framework to .NET](https://learn.microsoft.com/en-us/dotnet/core/porting/)
- [Migrate from ASP.NET MVC to ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/migration/mvc)
- [Azure Container Apps Documentation](https://learn.microsoft.com/en-us/azure/container-apps/)
- [Azure Service Bus Documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)

### Tools
- [.NET Upgrade Assistant](https://dotnet.microsoft.com/en-us/platform/upgrade-assistant)
- [try-convert Tool](https://github.com/dotnet/try-convert)
- [CoreWCF](https://github.com/CoreWCF/CoreWCF) (if WCF must be preserved)

### Best Practices
- [Azure Architecture Center](https://learn.microsoft.com/en-us/azure/architecture/)
- [Cloud Design Patterns](https://learn.microsoft.com/en-us/azure/architecture/patterns/)
- [Microservices Architecture](https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/microservices)

---

## Conclusion

The ProductCatalogApp modernization is a **moderate-to-high complexity project** that requires replacing several Windows-specific technologies with modern, cloud-native alternatives. The recommended incremental migration approach minimizes risk while delivering value at each phase.

With proper planning, resource allocation, and adherence to the phased approach, the application can be successfully modernized to .NET 10 and deployed to Azure Container Apps within **3-4 weeks**.

The key challenges—WCF to REST API, MSMQ to Service Bus, and distributed session state—all have well-established solutions and patterns. The resulting application will be more scalable, cost-effective, and maintainable while providing a foundation for future enhancements.

**Status:** ✅ Assessment Complete - Ready for Migration Planning

---

*This assessment was generated on 2026-01-11 as part of the ProductCatalogApp modernization initiative.*
