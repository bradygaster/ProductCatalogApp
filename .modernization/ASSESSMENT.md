# Modernization Assessment: ProductCatalogApp

**Assessment Date:** 2026-01-13  
**Assessed By:** GitHub Copilot Modernization Agent  
**Target Framework:** .NET 10  
**Target Platform:** Azure Container Apps  

---

## Executive Summary

The ProductCatalogApp is a **legacy .NET Framework 4.8.1** application built with **ASP.NET MVC 5**, **WCF services**, and **MSMQ messaging**. This assessment evaluates the effort required to modernize this application to **.NET 10** and deploy it to **Azure Container Apps**.

### Complexity Score: **7/10** (Medium-High Complexity)

The modernization requires significant architectural changes due to the heavy reliance on Windows-only technologies that are not supported in .NET Core/.NET 5+ or in containerized environments.

### Estimated Effort: **56-86 hours** (7-11 weeks for 1 developer)

---

## Current State Analysis

### Application Architecture

The ProductCatalogApp consists of three distinct components:

1. **ProductCatalog** - ASP.NET MVC 5 Web Application
   - Provides the user-facing product catalog interface
   - Displays products, manages shopping cart, handles checkout
   - Uses Session state for cart management
   - Communicates with ProductServiceLibrary via WCF
   - Sends orders to MSMQ queue asynchronously
   - ~800 lines of code across 9 C# files

2. **ProductServiceLibrary** - WCF Service Library
   - Provides CRUD operations for products and categories
   - Exposes WCF service endpoints with SOAP protocol
   - Contains business logic and data repository
   - ~650 lines of code across 6 C# files

3. **OrderProcessor** - Console Application
   - Background service that processes orders from MSMQ queue
   - Simulates order processing steps (payment, inventory, shipping, email)
   - ~514 lines of code across 2 C# files

### Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Framework | .NET Framework | 4.8.1 |
| Web Framework | ASP.NET MVC | 5.2.9 |
| Services | WCF | Built-in |
| Messaging | MSMQ | Windows feature |
| Project Format | Legacy .csproj | Pre-SDK style |
| Package Management | packages.config | Legacy NuGet |
| Configuration | Web.config/App.config | XML-based |
| Hosting | IIS | Windows-only |

### Dependencies

**ProductCatalog:**
- Microsoft.AspNet.Mvc 5.2.9
- Microsoft.AspNet.Razor 3.2.9
- Microsoft.AspNet.WebPages 3.2.9
- Newtonsoft.Json 13.0.3
- jQuery 3.7.0
- Bootstrap 5.2.3
- System.Messaging (MSMQ)
- System.ServiceModel (WCF client)

**ProductServiceLibrary:**
- System.ServiceModel (WCF)
- System.Runtime.Serialization

**OrderProcessor:**
- System.Messaging (MSMQ)
- System.Configuration

---

## Legacy Patterns Identified

### 🔴 High Severity Issues

#### 1. ASP.NET MVC 5 Framework
**Impact:** Cannot run on Linux or in containers without major rewrite

The application uses ASP.NET MVC 5, which:
- Requires Windows Server and IIS
- Is not cross-platform compatible
- Cannot be containerized easily
- Lacks modern features of ASP.NET Core

**Migration Required:** Complete rewrite to ASP.NET Core MVC

#### 2. WCF Service Architecture
**Impact:** WCF is not supported in .NET Core/.NET 5+

The ProductServiceLibrary uses WCF, which:
- Is a Windows-only technology
- Not available in .NET Core/.NET 5+
- Uses SOAP protocol (less common in modern apps)
- Cannot be hosted in containers easily

**Migration Required:** Replace with REST API (ASP.NET Core Web API) or gRPC

#### 3. MSMQ Message Queuing
**Impact:** MSMQ is Windows-only and not available in containers

The order processing uses MSMQ, which:
- Requires Windows Message Queuing feature
- Not available in Linux containers
- Not available in Azure Container Apps
- Cannot scale horizontally easily

**Migration Required:** Replace with Azure Service Bus or Azure Storage Queues

### 🟡 Medium Severity Issues

#### 4. Legacy Project Format
**Impact:** Cannot use modern .NET tooling

The projects use the old .csproj format:
- Verbose XML configuration
- Packages managed via packages.config
- Cannot use modern SDK features
- Slower build and restore times

**Migration Required:** Convert to SDK-style projects

#### 5. XML Configuration Files
**Impact:** Not compatible with modern configuration patterns

Uses Web.config/App.config:
- XML-based (not JSON)
- IIS-specific settings
- No environment-based configuration
- No Azure-friendly configuration

**Migration Required:** Migrate to appsettings.json and configuration providers

#### 6. In-Memory Session State
**Impact:** Not suitable for containerized/scaled deployments

Shopping cart uses ASP.NET Session:
- Stored in memory (not persistent)
- Tied to single server instance
- Lost on app restart or scale-out
- Not compatible with container orchestration

**Migration Required:** Use distributed cache (Redis) or database

#### 7. IIS-Specific Hosting
**Impact:** Cannot deploy to Azure Container Apps directly

Application designed for IIS:
- Web.config transformations
- IIS-specific handlers and modules
- Windows authentication dependencies
- Cannot run in Kestrel without changes

**Migration Required:** Migrate to Kestrel-based hosting

### 🟢 Low Severity Issues

#### 8. Packages.config Dependency Management
**Impact:** Minor inconvenience, slower operations

Using packages.config instead of PackageReference:
- Slower package restore
- More verbose project files
- Requires packages folder

**Migration Required:** Convert to PackageReference (part of SDK-style migration)

---

## Modernization Roadmap

### Phase 1: Project Structure Modernization
**Estimated Effort:** 4-8 hours | **Complexity:** 6/10

Convert all projects to SDK-style format and upgrade to .NET 10.

**Tasks:**
1. ✅ Convert ProductCatalog to SDK-style project
2. ✅ Convert ProductServiceLibrary to SDK-style project  
3. ✅ Convert OrderProcessor to SDK-style project
4. ✅ Update all projects to target net10.0
5. ✅ Migrate packages.config to PackageReference
6. ✅ Update NuGet packages to .NET 10 compatible versions
7. ✅ Test compilation and basic functionality

**Key Challenges:**
- Some ASP.NET MVC 5 packages don't have direct .NET 10 equivalents
- Need to identify replacement packages early
- Project structure changes may break references

**Success Criteria:**
- All projects compile as .NET 10 SDK-style projects
- NuGet packages restored correctly
- No build errors

---

### Phase 2: Web Application Migration
**Estimated Effort:** 16-24 hours | **Complexity:** 8/10

Migrate the ASP.NET MVC 5 web application to ASP.NET Core MVC.

**Tasks:**
1. ✅ Create new ASP.NET Core MVC project targeting .NET 10
2. ✅ Migrate controllers and action methods
3. ✅ Migrate Razor views and layouts
4. ✅ Update routing configuration (Convention-based to Endpoint routing)
5. ✅ Migrate bundling/minification (BundleConfig → modern approach)
6. ✅ Replace Session state with IDistributedCache (Redis)
7. ✅ Update dependency injection and services configuration
8. ✅ Migrate Global.asax to Program.cs/Startup pattern
9. ✅ Update authentication/authorization (if applicable)
10. ✅ Test all web pages and functionality

**Key Challenges:**
- ASP.NET Core has different middleware pipeline
- Razor syntax is mostly compatible but has some differences
- Session management completely different
- No Global.asax - use middleware instead
- Different model binding and validation

**Migration Pattern:**
```csharp
// Old: Global.asax.cs
public class MvcApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        AreaRegistration.RegisterAllAreas();
        RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
}

// New: Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
```

**Success Criteria:**
- All pages render correctly
- Shopping cart works with distributed cache
- Checkout flow completes successfully
- Static files (CSS, JS, images) served correctly

---

### Phase 3: WCF to REST API Migration
**Estimated Effort:** 12-16 hours | **Complexity:** 7/10

Replace WCF service with ASP.NET Core Web API.

**Tasks:**
1. ✅ Create ASP.NET Core Web API project targeting .NET 10
2. ✅ Convert WCF service contracts to API controllers
3. ✅ Implement REST endpoints for all operations:
   - `GET /api/products` - Get all products
   - `GET /api/products/{id}` - Get product by ID
   - `GET /api/products/category/{category}` - Get by category
   - `GET /api/products/search?term={term}` - Search products
   - `GET /api/categories` - Get all categories
   - `POST /api/products` - Create product
   - `PUT /api/products/{id}` - Update product
   - `DELETE /api/products/{id}` - Delete product
   - `GET /api/products/price-range?min={min}&max={max}` - Price range filter
4. ✅ Migrate repository and business logic
5. ✅ Update ProductCatalog to use HttpClient instead of WCF proxy
6. ✅ Add proper error handling and HTTP status codes
7. ✅ Implement API versioning
8. ✅ Add OpenAPI/Swagger documentation
9. ✅ Test all API endpoints

**Key Challenges:**
- SOAP to REST paradigm shift
- Different error handling (FaultException vs HTTP status codes)
- Need to implement proper REST conventions
- Service reference code must be removed
- Client code needs HttpClient instead of WCF proxy

**Migration Pattern:**
```csharp
// Old: WCF Service
[ServiceContract]
public interface IProductService
{
    [OperationContract]
    List<Product> GetAllProducts();
}

// New: REST API Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Product>> GetAllProducts()
    {
        // Implementation
        return Ok(products);
    }
}

// Old: WCF Client
using (var client = new ProductServiceClient())
{
    products = client.GetAllProducts().ToList();
}

// New: HTTP Client
var response = await _httpClient.GetAsync("/api/products");
products = await response.Content.ReadFromJsonAsync<List<Product>>();
```

**Success Criteria:**
- All WCF operations available as REST endpoints
- Web application successfully calls REST API
- Swagger documentation accessible
- Proper error handling and status codes

---

### Phase 4: MSMQ to Azure Service Bus Migration
**Estimated Effort:** 8-12 hours | **Complexity:** 6/10

Replace MSMQ with Azure Service Bus for message queuing.

**Tasks:**
1. ✅ Create Azure Service Bus namespace and queue
2. ✅ Install Azure.Messaging.ServiceBus NuGet package
3. ✅ Create Service Bus client in web application
4. ✅ Replace OrderQueueService to use Service Bus instead of MSMQ
5. ✅ Update OrderProcessor to consume from Service Bus
6. ✅ Implement error handling and retry policies
7. ✅ Update configuration for connection strings
8. ✅ Add dead letter queue handling
9. ✅ Test message flow end-to-end

**Key Challenges:**
- Different API than System.Messaging
- Need Azure subscription and resources
- Connection string management and security
- Error handling patterns different
- Need to handle cloud connectivity issues

**Migration Pattern:**
```csharp
// Old: MSMQ
using (MessageQueue queue = new MessageQueue(queuePath))
{
    queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Order) });
    queue.Send(order);
}

// New: Azure Service Bus
await using var client = new ServiceBusClient(connectionString);
ServiceBusSender sender = client.CreateSender(queueName);
var message = new ServiceBusMessage(JsonSerializer.Serialize(order));
await sender.SendMessageAsync(message);
```

**Azure Resources Required:**
- Azure Service Bus namespace (Standard or Premium tier)
- Queue for orders
- Managed Identity for authentication (recommended)
- Connection string configuration

**Success Criteria:**
- Orders successfully sent to Service Bus queue
- Order processor receives and processes orders
- Error handling works correctly
- Dead letter queue properly configured

---

### Phase 5: Containerization
**Estimated Effort:** 6-10 hours | **Complexity:** 5/10

Create Docker containers for all application components.

**Tasks:**
1. ✅ Create Dockerfile for web application
2. ✅ Create Dockerfile for API service
3. ✅ Create Dockerfile for order processor
4. ✅ Create docker-compose.yml for local testing
5. ✅ Optimize container images (multi-stage builds)
6. ✅ Configure health checks
7. ✅ Set up environment-based configuration
8. ✅ Test containers locally
9. ✅ Document container usage

**Dockerfile Example:**
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

**Success Criteria:**
- All applications build as Docker images
- Containers run successfully locally
- Health checks respond correctly
- Configuration works via environment variables

---

### Phase 6: Azure Container Apps Deployment
**Estimated Effort:** 10-16 hours | **Complexity:** 7/10

Deploy the containerized application to Azure Container Apps.

**Tasks:**
1. ✅ Create Azure Container Apps environment
2. ✅ Set up Azure Container Registry (ACR)
3. ✅ Configure GitHub Actions or Azure Pipelines for CI/CD
4. ✅ Deploy web application container app
5. ✅ Deploy API service container app
6. ✅ Deploy order processor container app (as a background job)
7. ✅ Configure ingress for web and API apps
8. ✅ Set up managed identity for Service Bus access
9. ✅ Configure environment variables and secrets
10. ✅ Set up autoscaling rules
11. ✅ Configure monitoring and Application Insights
12. ✅ Test end-to-end in Azure

**Azure Resources Required:**
- Azure Container Apps Environment
- Azure Container Registry
- Azure Service Bus (from Phase 4)
- Azure Redis Cache (for session state)
- Application Insights (monitoring)
- Azure Key Vault (for secrets)

**Architecture in Azure:**
```
┌─────────────────────────────────────────────────┐
│   Azure Container Apps Environment              │
│                                                  │
│  ┌──────────────┐      ┌──────────────┐        │
│  │   Web App    │─────→│   API App    │        │
│  │  Container   │      │  Container   │        │
│  └──────────────┘      └──────────────┘        │
│         │                                        │
│         └──────────────┐                        │
│                        ↓                        │
│              ┌──────────────────┐               │
│              │ Order Processor  │               │
│              │   Container      │               │
│              │  (Background)    │               │
│              └──────────────────┘               │
│                        ↓                        │
└────────────────────────│────────────────────────┘
                         │
              ┌──────────┴──────────┐
              ↓                     ↓
     ┌─────────────────┐   ┌──────────────┐
     │ Azure Service   │   │ Azure Redis  │
     │     Bus         │   │    Cache     │
     └─────────────────┘   └──────────────┘
```

**Success Criteria:**
- All containers deployed and running in Azure
- Web application accessible via public URL
- API accessible (optionally internal only)
- Order processing working with Service Bus
- Autoscaling configured and tested
- Monitoring and logging working

---

## Risk Assessment

### High Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| WCF to REST migration introduces bugs | High | Medium | Implement comprehensive API tests, staged rollout |
| MSMQ to Service Bus loses messages | High | Low | Use transactions, implement retry logic, monitor dead letter queue |
| Session state migration breaks cart | High | Medium | Thoroughly test cart functionality, implement fallback |
| No existing tests = regression risk | High | High | Create smoke tests before migration, add tests during |

### Medium Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Azure Service Bus costs higher than expected | Medium | Medium | Monitor usage, implement proper message cleanup |
| Container performance issues | Medium | Low | Performance testing, proper resource allocation |
| Configuration management complexity | Medium | Medium | Use Azure Key Vault, document configuration |

### Low Risks

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Learning curve for new technologies | Low | High | Training, documentation, pair programming |
| Dependency conflicts during migration | Low | Medium | Careful package management, version pinning |

---

## Testing Strategy

### Recommended Testing Approach

Since the application currently has **no automated tests**, testing is a critical risk factor.

**Phase-by-Phase Testing:**

1. **Phase 1 (Project Conversion):**
   - Manual smoke tests of existing functionality
   - Verify build and deployment process

2. **Phase 2 (Web Migration):**
   - Create integration tests for controllers
   - UI testing for critical paths (browse, cart, checkout)
   - Session state testing with distributed cache

3. **Phase 3 (API Migration):**
   - Unit tests for API controllers
   - Integration tests for API endpoints
   - Contract tests between web app and API

4. **Phase 4 (Messaging):**
   - End-to-end tests for order flow
   - Service Bus integration tests
   - Error handling and retry tests

5. **Phase 5 (Containers):**
   - Container health check tests
   - Local integration testing with docker-compose

6. **Phase 6 (Azure):**
   - Deployment verification tests
   - Performance testing
   - Load testing for autoscaling

**Testing Tools:**
- xUnit or NUnit for unit tests
- ASP.NET Core integration testing framework
- Playwright or Selenium for UI tests
- Postman/REST Client for API testing
- Azure Load Testing for performance

---

## Cost Estimation

### Development Costs

| Phase | Optimistic | Realistic | Conservative |
|-------|-----------|-----------|--------------|
| Phase 1 | 4 hours | 6 hours | 8 hours |
| Phase 2 | 16 hours | 20 hours | 24 hours |
| Phase 3 | 12 hours | 14 hours | 16 hours |
| Phase 4 | 8 hours | 10 hours | 12 hours |
| Phase 5 | 6 hours | 8 hours | 10 hours |
| Phase 6 | 10 hours | 14 hours | 16 hours |
| **Total** | **56 hours** | **72 hours** | **86 hours** |

**Timeline Estimates:**
- **Full-time (40 hrs/week):** 7-11 weeks
- **Part-time (20 hrs/week):** 12-18 weeks  
- **With other duties (10 hrs/week):** 16-22 weeks

### Azure Infrastructure Costs (Monthly, USD)

| Resource | Tier | Estimated Cost |
|----------|------|----------------|
| Container Apps (3 apps) | Consumption | $20-50 |
| Azure Container Registry | Basic | $5 |
| Azure Service Bus | Standard | $10 |
| Azure Redis Cache | Basic (1GB) | $17 |
| Application Insights | Pay-as-you-go | $5-15 |
| **Total Estimated Monthly** | | **$57-97** |

*Note: Costs can vary significantly based on usage patterns, scaling, and data transfer.*

---

## Success Criteria

### Technical Criteria

- ✅ All applications running on .NET 10
- ✅ Web application accessible via HTTPS in Azure Container Apps
- ✅ WCF completely replaced with REST API
- ✅ MSMQ completely replaced with Azure Service Bus
- ✅ All original functionality working correctly
- ✅ Applications containerized and scalable
- ✅ Session state persistent across restarts
- ✅ Order processing working asynchronously

### Operational Criteria

- ✅ Application Insights monitoring configured
- ✅ Logging implemented and accessible
- ✅ Health checks responding correctly
- ✅ Autoscaling rules configured
- ✅ CI/CD pipeline functional
- ✅ Documentation updated
- ✅ Deployment runbook created

### Quality Criteria

- ✅ No critical security vulnerabilities
- ✅ API documented with OpenAPI/Swagger
- ✅ Performance acceptable (no worse than current)
- ✅ Error handling properly implemented
- ✅ Configuration externalized
- ✅ Secrets stored securely (Key Vault)

---

## Immediate Next Steps

### Recommended Actions (Priority Order)

1. **✅ Review and Approve Assessment** (1 hour)
   - Stakeholder review of this assessment
   - Budget and timeline approval
   - Go/no-go decision

2. **✅ Environment Setup** (2-4 hours)
   - Install .NET 10 SDK
   - Install Docker Desktop
   - Set up Azure subscription
   - Create development resource group in Azure

3. **✅ Create Development Branch** (15 minutes)
   - Create `feature/dotnet10-migration` branch
   - Set up branch protection rules
   - Configure PR requirements

4. **✅ Phase 1: Start Project Conversion** (4-8 hours)
   - Begin with ProductServiceLibrary (smallest)
   - Then OrderProcessor
   - Finally ProductCatalog
   - Test after each conversion

5. **✅ Set Up Basic CI/CD** (2-4 hours)
   - Create GitHub Actions workflow for .NET 10
   - Set up automated builds
   - Configure Azure credentials

---

## Conclusion

The ProductCatalogApp modernization from **.NET Framework 4.8.1 to .NET 10** and deployment to **Azure Container Apps** is a **medium-high complexity project** requiring approximately **56-86 hours** of development effort.

### Key Takeaways

✅ **Feasible:** The migration is technically feasible with well-established migration paths.

⚠️ **Challenging:** Significant architectural changes required due to WCF, MSMQ, and ASP.NET MVC 5.

📊 **Estimated Effort:** 7-11 weeks for a single developer working full-time.

💰 **Cloud Costs:** ~$60-100/month for Azure resources.

🎯 **Risk Level:** Medium-High, primarily due to lack of automated tests and architectural changes.

### Recommendation

**Proceed with migration** using the phased approach outlined in this assessment. The legacy technologies (WCF, MSMQ, ASP.NET MVC 5) are becoming increasingly difficult to maintain and host, and the application will benefit significantly from modernization:

- **Cross-platform capability** (Linux containers)
- **Cloud-native deployment** (Azure Container Apps)
- **Better scalability** (horizontal scaling)
- **Modern development experience** (.NET 10 features)
- **Improved maintainability** (modern patterns)
- **Cost efficiency** (containerized hosting)

The investment in modernization will pay dividends in reduced hosting costs, improved developer productivity, and better application performance and scalability.

---

## Appendix

### A. Technology Mapping

| Legacy Technology | Modern Replacement |
|-------------------|-------------------|
| .NET Framework 4.8.1 | .NET 10 |
| ASP.NET MVC 5 | ASP.NET Core MVC |
| WCF | ASP.NET Core Web API (REST) |
| MSMQ | Azure Service Bus |
| System.Web.HttpContext | Microsoft.AspNetCore.Http.HttpContext |
| Session["key"] | IDistributedCache |
| Web.config | appsettings.json |
| Global.asax | Program.cs middleware |
| packages.config | PackageReference |
| IIS | Kestrel (in container) |

### B. Reference Documentation

- [Migrate from ASP.NET MVC to ASP.NET Core MVC](https://docs.microsoft.com/en-us/aspnet/core/migration/mvc)
- [WCF to gRPC migration](https://docs.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/)
- [Azure Service Bus Documentation](https://docs.microsoft.com/en-us/azure/service-bus-messaging/)
- [Azure Container Apps Documentation](https://docs.microsoft.com/en-us/azure/container-apps/)
- [.NET 10 Migration Guide](https://docs.microsoft.com/en-us/dotnet/core/migration/)

### C. Useful Tools

- **Try-Convert:** Converts legacy projects to SDK-style
- **.NET Upgrade Assistant:** Automated migration tool
- **Portability Analyzer:** Identifies compatibility issues
- **Azure Migrate:** Assess cloud readiness
- **Docker:** Containerization platform

---

**End of Assessment**
