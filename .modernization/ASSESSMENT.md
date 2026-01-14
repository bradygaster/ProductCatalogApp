# ProductCatalogApp Modernization Assessment

**Assessment Date:** January 14, 2026  
**Target Platform:** .NET 10 on Azure Container Apps  
**Current State:** .NET Framework 4.8.1 ASP.NET MVC 5 + WCF

---

## Executive Summary

This ProductCatalogApp is a legacy .NET Framework 4.8.1 application that requires significant modernization to meet the goals of upgrading to .NET 10 and deploying to Azure Container Apps. The application currently consists of:

- **ProductCatalog**: ASP.NET MVC 5 web application
- **ProductServiceLibrary**: WCF service library

**Current Azure Container Apps Readiness: 20%**

### Critical Blockers
1. **.NET Framework 4.8.1** - Windows-only, requires migration to .NET 10
2. **WCF Services** - Not supported in modern .NET, must be replaced
3. **MSMQ** - Windows-only messaging, incompatible with Azure Container Apps
4. **IIS Hosting** - Must migrate to Kestrel for container compatibility

### Estimated Effort
**Total: 6-8 weeks** with experienced team

---

## Current Architecture

### Technology Stack
- **Framework:** .NET Framework 4.8.1
- **Web Framework:** ASP.NET MVC 5
- **Service Communication:** WCF (Windows Communication Foundation)
- **Messaging:** MSMQ (Microsoft Message Queuing)
- **Project Format:** Legacy MSBuild with packages.config
- **Configuration:** Web.config / App.config (XML-based)
- **Hosting:** IIS on Windows Server
- **Data Storage:** In-memory repository (no persistence)
- **Session Management:** In-memory Session state

### Project Structure
```
ProductCatalogApp/
├── ProductCatalog/              # ASP.NET MVC 5 Web App
│   ├── Controllers/             # MVC Controllers
│   ├── Views/                   # Razor Views
│   ├── Models/                  # View Models (Order, CartItem)
│   ├── Services/                # OrderQueueService (MSMQ)
│   ├── Connected Services/      # WCF Service Reference
│   ├── App_Start/               # MVC Configuration
│   ├── Web.config               # Configuration
│   └── packages.config          # NuGet packages (legacy)
│
└── ProductServiceLibrary/       # WCF Service Library
    ├── IProductService.cs       # Service Contract
    ├── ProductService.cs        # Service Implementation
    ├── ProductRepository.cs     # In-memory data access
    ├── Product.cs / Category.cs # Data models
    └── App.config               # WCF Configuration
```

---

## Detailed Findings

### 1. Framework & Runtime (Critical - High Effort)

#### Current State
- .NET Framework 4.8.1 (Windows-only runtime)
- Legacy .csproj format with packages.config
- Requires Windows Server for hosting

#### Issues
- **Cannot run on Linux containers** - Azure Container Apps requires Linux containers
- **Missing modern runtime features** - No Span<T>, System.Text.Json, etc.
- **Limited performance** - Older runtime with less optimization
- **Windows licensing costs** - Requires Windows Server licenses

#### Recommendation
✅ **Migrate to .NET 10**
- Use SDK-style project format
- Convert to PackageReference
- Leverage modern C# language features
- Enable cross-platform deployment

**Effort:** 2-3 weeks

---

### 2. Web Framework (Critical - High Effort)

#### Current State
- ASP.NET MVC 5 (legacy Windows-only)
- Global.asax for application startup
- System.Web dependencies
- Bundle and Minification via App_Start

#### Issues
- **Not portable to .NET Core/.NET 5+**
- **Tied to System.Web** - Not available in modern .NET
- **IIS-dependent features** - HttpContext, Session, etc.
- **Missing modern middleware pipeline**

#### Recommendation
✅ **Migrate to ASP.NET Core MVC**
- Use Program.cs and WebApplication builder
- Migrate to middleware pipeline
- Update controllers (minimal changes usually)
- Migrate Razor views (mostly compatible)
- Replace bundling with modern tools (Vite, webpack, or built-in)

**Effort:** 1-2 weeks

**Compatibility Notes:**
- Most Razor views will work with minimal changes
- Controller actions are similar in ASP.NET Core
- ViewBag, TempData, Session require updates
- Filters and attributes may need adjustments

---

### 3. WCF Services (Critical - High Effort)

#### Current State
- WCF service library with SOAP/XML contracts
- Service contract: `IProductService`
- Operations: CRUD for products, categories, search
- Client uses WCF proxy (ProductServiceReference)

#### Issues
- **WCF not supported in .NET Core/.NET 5+**
- **Windows-only technology**
- **Complex, heavyweight protocol**
- **Not cloud-native or container-friendly**

#### Recommendation Options

**Option A: REST API (Recommended)**
✅ **Replace with ASP.NET Core Web API**
- Create minimal API or controller-based API
- RESTful endpoints with JSON
- Use HttpClient for client communication
- Modern, lightweight, cloud-native

**Advantages:**
- Industry standard for web services
- Easy to consume from any platform
- Great tooling and documentation
- Works perfectly in containers

**Example Migration:**
```csharp
// Before (WCF)
[OperationContract]
List<Product> GetAllProducts();

// After (ASP.NET Core API)
[HttpGet("api/products")]
public async Task<ActionResult<List<Product>>> GetAllProducts()
{
    return Ok(await _repository.GetAllProductsAsync());
}
```

**Option B: gRPC**
- Modern RPC framework
- High performance, compact binary protocol
- Strong typing with Protocol Buffers
- Good for service-to-service communication

**Option C: CoreWCF (Interim)**
- Community-maintained WCF for .NET Core
- Allows gradual migration
- Not recommended for greenfield

**Effort:** 2-3 weeks (REST API approach)

---

### 4. Messaging - MSMQ (Critical - Medium Effort)

#### Current State
- `OrderQueueService` uses MSMQ for order processing
- Private queue: `.\Private$\ProductCatalogOrders`
- XML serialization of Order objects
- Synchronous send, receive with timeout

#### Issues
- **MSMQ is Windows-only**
- **Not available in Linux containers**
- **Not available in Azure Container Apps**
- **No cloud-native alternative from Microsoft built-in**

#### Recommendation
✅ **Migrate to Azure Service Bus**

**Why Azure Service Bus:**
- Fully managed cloud messaging service
- Works seamlessly with Azure Container Apps
- Supports queues, topics, and subscriptions
- Dead-letter queue for error handling
- Enterprise features: transactions, sessions, duplicate detection

**Migration Path:**
1. Replace `System.Messaging` with `Azure.Messaging.ServiceBus`
2. Update `OrderQueueService` to use Service Bus client
3. Configure connection string in configuration
4. Update send/receive logic (very similar API)

**Example:**
```csharp
// Before (MSMQ)
using System.Messaging;
var queue = new MessageQueue(queuePath);
queue.Send(new Message(order));

// After (Azure Service Bus)
using Azure.Messaging.ServiceBus;
var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender(queueName);
await sender.SendMessageAsync(new ServiceBusMessage(JsonSerializer.Serialize(order)));
```

**Alternative:** Azure Storage Queues (simpler, lower cost, but fewer features)

**Effort:** 1 week

---

### 5. Project Format (Medium - Low Effort)

#### Current State
- Old-style MSBuild .csproj (verbose XML)
- packages.config for NuGet packages
- Separate AssemblyInfo.cs files
- Complex import statements

#### Issues
- Verbose and hard to maintain
- Slower restore times
- Cannot use modern .NET SDK features
- Incompatible with .NET Core/.NET 5+

#### Recommendation
✅ **Convert to SDK-style projects**

**Benefits:**
- Compact, readable project files
- PackageReference (faster, more reliable)
- Auto-generated AssemblyInfo
- Multi-targeting support
- Built-in NuGet package creation

**Migration:**
- Can be automated with conversion tools
- Manual review recommended for complex projects

**Effort:** 2-3 days

---

### 6. Configuration (Medium - Low Effort)

#### Current State
- Web.config (XML) for web app
- App.config (XML) for service library
- appSettings, connectionStrings sections
- System.Configuration APIs

#### Issues
- XML format is verbose
- Not flexible for modern deployment scenarios
- No environment-based configuration
- No Azure Key Vault integration

#### Recommendation
✅ **Migrate to appsettings.json**

**Benefits:**
- JSON format (more readable)
- Environment-specific files (appsettings.Development.json)
- IConfiguration abstraction
- Easy Azure Key Vault, environment variables integration
- Strongly-typed configuration with IOptions

**Example:**
```json
{
  "ServiceBus": {
    "ConnectionString": "...",
    "QueueName": "product-catalog-orders"
  },
  "ProductApi": {
    "BaseUrl": "https://api.productcatalog.com"
  }
}
```

**Effort:** 3-5 days

---

### 7. Dependency Injection (Medium - Medium Effort)

#### Current State
- No built-in DI in MVC 5
- Manual instantiation: `new ProductServiceClient()`
- Services created in controllers
- No lifecycle management

#### Issues
- Tight coupling
- Hard to test
- Repeated instantiation code
- No centralized configuration

#### Recommendation
✅ **Use ASP.NET Core Built-in DI**

**Benefits:**
- Loose coupling
- Easy testing with mocks
- Centralized service registration
- Lifecycle management (Transient, Scoped, Singleton)

**Example:**
```csharp
// Program.cs
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddSingleton<IOrderQueueService, OrderQueueService>();

// Controller
public class HomeController : Controller
{
    private readonly IProductService _productService;
    
    public HomeController(IProductService productService)
    {
        _productService = productService;
    }
}
```

**Effort:** 1 week

---

### 8. Session State (Medium - Low Effort)

#### Current State
- In-memory Session storage: `Session["Cart"]`
- Stores shopping cart in web server memory
- Lost on application restart
- Not suitable for load balancing

#### Issues
- **Not scalable** - Each container has separate session
- **Lost on restart** - Poor user experience
- **No distribution** - Cannot load balance effectively
- **Memory inefficient** - Grows with user count

#### Recommendation
✅ **Implement distributed cache with Azure Redis**

**Benefits:**
- Shared state across all container instances
- Survives restarts
- Scales independently
- Fast in-memory performance

**Implementation:**
```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Controller
private readonly IDistributedCache _cache;

public async Task<IActionResult> AddToCart(int productId)
{
    var cart = await _cache.GetAsync<List<CartItem>>("cart_" + userId);
    // ... update cart
    await _cache.SetAsync("cart_" + userId, cart);
}
```

**Effort:** 3-5 days

---

### 9. Data Persistence (Low - Medium Effort)

#### Current State
- `ProductRepository` uses in-memory List<Product>
- No database
- Data reset on application restart
- Hard-coded sample data

#### Issues
- Not suitable for production
- No data persistence
- No concurrency control
- No data integrity

#### Recommendation
✅ **Add Azure SQL Database with Entity Framework Core**

**Benefits:**
- Proper data persistence
- ACID transactions
- Scalable cloud database
- Managed service (backups, HA, etc.)

**Alternative:** Azure Cosmos DB for global distribution

**Effort:** 1-2 weeks (includes schema design, EF Core setup, migrations)

---

### 10. Hosting (High - High Effort)

#### Current State
- Designed for IIS hosting
- Windows Server required
- System.Web dependencies
- Machine-level configuration

#### Issues
- Windows-only
- Cannot run in Linux containers
- Complex configuration
- Not cloud-native

#### Recommendation
✅ **Migrate to Kestrel web server**

**Benefits:**
- Cross-platform
- Lightweight and fast
- Container-friendly
- No Windows/IIS dependencies

**This is automatic with ASP.NET Core migration**

**Effort:** Included in framework migration

---

## Modernization Roadmap

### Phase 1: Assessment & Planning (1 week)
- ✅ Complete modernization assessment (this document)
- Document current architecture and dependencies
- Set up development environment with .NET 10
- Create feature branch for modernization

### Phase 2: Project Modernization (2-3 weeks)
1. **Convert projects to SDK-style format** (2-3 days)
   - Use conversion tools or manual migration
   - Convert packages.config to PackageReference
   - Test build and restore

2. **Upgrade to .NET 10** (1-2 weeks)
   - Create new .NET 10 projects
   - Migrate ASP.NET MVC 5 to ASP.NET Core MVC
   - Update Program.cs with middleware pipeline
   - Migrate Razor views
   - Update controllers and filters
   - Test web application functionality

3. **Migrate configuration** (3-5 days)
   - Convert Web.config to appsettings.json
   - Set up environment-specific configuration
   - Implement IOptions pattern
   - Test configuration loading

### Phase 3: Service Architecture Migration (2-3 weeks)
1. **Replace WCF with REST API** (2 weeks)
   - Create ASP.NET Core Web API project
   - Define RESTful endpoints
   - Implement controllers with same business logic
   - Add Swagger/OpenAPI documentation
   - Update client to use HttpClient
   - Test API endpoints

2. **Remove WCF dependencies** (3-5 days)
   - Delete ProductServiceLibrary (WCF) project
   - Remove Connected Services from web app
   - Clean up WCF configuration
   - Test end-to-end functionality

### Phase 4: Infrastructure Modernization (1-2 weeks)
1. **Replace MSMQ with Azure Service Bus** (1 week)
   - Create Azure Service Bus namespace
   - Update OrderQueueService to use Service Bus SDK
   - Configure connection strings
   - Test order submission and processing
   - Add error handling and dead-letter queue monitoring

2. **Implement distributed session** (3-5 days)
   - Create Azure Redis Cache instance
   - Add StackExchange.Redis NuGet package
   - Configure distributed cache
   - Migrate session usage to distributed cache
   - Test cart functionality across restarts

3. **Add database persistence** (1 week)
   - Create Azure SQL Database
   - Design database schema
   - Set up Entity Framework Core
   - Create migrations
   - Update repositories to use EF Core
   - Test CRUD operations

### Phase 5: Containerization (3-5 days)
1. **Create Dockerfile** (1-2 days)
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
   WORKDIR /app
   EXPOSE 8080

   FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
   WORKDIR /src
   COPY ["ProductCatalog/ProductCatalog.csproj", "ProductCatalog/"]
   RUN dotnet restore "ProductCatalog/ProductCatalog.csproj"
   COPY . .
   WORKDIR "/src/ProductCatalog"
   RUN dotnet build "ProductCatalog.csproj" -c Release -o /app/build

   FROM build AS publish
   RUN dotnet publish "ProductCatalog.csproj" -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "ProductCatalog.dll"]
   ```

2. **Configure container settings** (1 day)
   - Add health check endpoints
   - Configure logging for containers
   - Set up readiness and liveness probes
   - Optimize container size

3. **Test locally** (1-2 days)
   - Build container image
   - Run container with Docker
   - Test all functionality
   - Validate external service connections

### Phase 6: Azure Container Apps Deployment (3-5 days)
1. **Provision Azure resources** (1 day)
   - Create resource group
   - Create Azure Container Registry
   - Create Azure Container Apps environment
   - Set up networking and security

2. **Configure deployment** (1-2 days)
   - Create Container App
   - Configure ingress (HTTPS, custom domain)
   - Set scaling rules (CPU, HTTP, custom metrics)
   - Configure managed identity for Azure services
   - Set environment variables and secrets

3. **Set up CI/CD** (1 day)
   - Create GitHub Actions workflow
   - Configure container build and push
   - Deploy to Azure Container Apps
   - Set up staging/production slots

4. **Validation and monitoring** (1 day)
   - Deploy to staging environment
   - Run smoke tests
   - Set up Application Insights
   - Configure alerts and dashboards
   - Deploy to production

---

## Risk Assessment

### High Risk Items

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| WCF migration breaks functionality | High | Medium | Thorough API contract design, extensive testing |
| MSMQ message format incompatibility | High | Medium | Design compatible serialization, dual-run period |
| Session state migration issues | Medium | Low | Test extensively with concurrent users |
| Performance degradation | Medium | Low | Load testing before and after migration |

### Medium Risk Items

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| Container networking issues | Medium | Low | Test locally with Docker, use proper networking |
| Azure service connectivity | Medium | Low | Use managed identities, proper error handling |
| Configuration management | Low | Medium | Use Azure Key Vault, environment variables |
| Learning curve for team | Medium | Medium | Training, pair programming, documentation |

---

## Cost Considerations

### Current Costs (Estimated)
- Windows Server licenses
- IIS hosting
- On-premises infrastructure
- Maintenance overhead

### Future Costs (Azure Container Apps)
- **Azure Container Apps:** ~$50-200/month (varies with scale)
- **Azure Service Bus:** ~$10-50/month (Standard tier)
- **Azure Redis Cache:** ~$15-30/month (Basic C1)
- **Azure SQL Database:** ~$5-100/month (depends on tier)
- **Azure Container Registry:** ~$5/month (Basic)
- **Application Insights:** ~$0-50/month (depends on volume)

**Estimated Monthly Cost:** $85-430/month (depending on scale and tier selection)

**Savings:**
- No Windows Server licenses
- No on-premises infrastructure
- Pay-as-you-go scaling
- Reduced maintenance overhead

---

## Technical Debt Items

The following technical debt should be addressed during or after modernization:

### Critical
- [ ] No automated testing (unit, integration, E2E)
- [ ] No CI/CD pipeline
- [ ] No authentication/authorization
- [ ] No input validation/sanitization

### High
- [ ] No logging framework (use ILogger/Serilog)
- [ ] No error handling strategy
- [ ] No API versioning
- [ ] No rate limiting or throttling

### Medium
- [ ] No API documentation (add Swagger/OpenAPI)
- [ ] No health checks
- [ ] No distributed tracing
- [ ] No structured logging

### Low
- [ ] Hard-coded configuration values
- [ ] No caching strategy
- [ ] No code comments/documentation
- [ ] No performance monitoring

---

## Success Metrics

### Technical Metrics
- ✅ Application runs on .NET 10
- ✅ Runs in Linux container
- ✅ Successfully deploys to Azure Container Apps
- ✅ No WCF dependencies
- ✅ No MSMQ dependencies
- ✅ No Windows-only dependencies
- ✅ Health checks return 200 OK
- ✅ All features functional after migration

### Performance Metrics
- Response time < 200ms (p95)
- Container startup time < 30 seconds
- Zero downtime deployments
- Auto-scaling within 60 seconds

### Quality Metrics
- Code coverage > 70%
- Zero critical security vulnerabilities
- All tests passing
- No deployment failures

---

## Recommendations Summary

### Immediate Actions (Week 1)
1. ✅ Complete this assessment
2. Set up .NET 10 development environment
3. Create modernization branch
4. Provision Azure resources (Service Bus, Redis, SQL)
5. Set up project tracking (GitHub Projects/Azure Boards)

### High Priority (Weeks 2-4)
1. Upgrade to .NET 10 with SDK-style projects
2. Migrate ASP.NET MVC 5 to ASP.NET Core MVC
3. Replace WCF with REST API
4. Migrate configuration to appsettings.json

### Medium Priority (Weeks 5-6)
1. Replace MSMQ with Azure Service Bus
2. Implement distributed cache with Redis
3. Add Entity Framework Core with Azure SQL
4. Implement dependency injection

### Final Steps (Weeks 7-8)
1. Create Dockerfile and test locally
2. Deploy to Azure Container Apps
3. Set up CI/CD pipeline
4. Add monitoring and logging
5. Perform load testing
6. Document changes

---

## Conclusion

The ProductCatalogApp requires significant modernization to achieve the goal of running on .NET 10 in Azure Container Apps. The primary challenges are:

1. **Framework Migration:** .NET Framework 4.8.1 → .NET 10
2. **Service Architecture:** WCF → REST API or gRPC
3. **Messaging:** MSMQ → Azure Service Bus
4. **State Management:** In-memory → Distributed Redis Cache

With proper planning and execution, this modernization is achievable in **6-8 weeks** with an experienced development team. The result will be a modern, cloud-native application that is:

- Cross-platform and container-ready
- Scalable and cloud-native
- Easier to maintain and extend
- Lower operational costs
- Better performance and reliability

### Next Steps
1. Review and approve this assessment
2. Allocate resources and budget
3. Set up development environment
4. Begin Phase 2: Project Modernization
5. Follow the roadmap iteratively with regular check-ins

---

**Document Version:** 1.0  
**Last Updated:** January 14, 2026  
**Prepared by:** GitHub Copilot Modernization Assessment  
**Status:** Complete
