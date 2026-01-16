# Modernization Assessment Report

**Date:** January 16, 2026  
**Repository:** bradygaster/ProductCatalogApp  
**Assessment Version:** 1.0

---

## Executive Summary

This repository contains a .NET Framework 4.8.1 e-commerce application requiring modernization to .NET 10 and deployment to Azure Container Apps. The application currently uses legacy technologies (WCF, MSMQ, ASP.NET MVC) that are incompatible with modern containerized deployments.

**User Request:**
> upgrade this app to .net 10 and ready it for deployment to azure container apps. migrate all the msmq functionality to azure service bus

**Overall Assessment:** Medium complexity modernization requiring 4-6 weeks of effort.

---

## Current State Analysis

### Technology Stack

| Component | Current Technology | Version | Status |
|-----------|-------------------|---------|---------|
| Framework | .NET Framework | 4.8.1 | ⚠️ Legacy |
| Web Framework | ASP.NET MVC | 5.2.9 | ⚠️ Legacy |
| Service Layer | WCF | 4.8.1 | ⚠️ Legacy |
| Messaging | MSMQ | System.Messaging | ⚠️ Legacy |
| Session State | In-Memory | - | ⚠️ Not Cloud-Ready |
| Project Format | Classic .csproj | - | ⚠️ Legacy |

### Project Structure

The solution contains **2 projects**:

1. **ProductCatalog** - ASP.NET MVC Web Application
   - Controllers, Views, Models
   - WCF client (Connected Services)
   - MSMQ integration for order processing
   - Session-based shopping cart

2. **ProductServiceLibrary** - WCF Service Library
   - Product service interface and implementation
   - Category and Product data contracts
   - Repository pattern for data access

### Code Metrics

- **Total C# Files:** 16
- **Total Lines of Code:** ~1,776
- **Complexity:** Low to Medium

---

## Legacy Patterns Identified

### 🔴 Critical Issues

#### 1. .NET Framework 4.8.1
- **Impact:** Blocks containerization and cross-platform deployment
- **Files Affected:** All .csproj files
- **Effort:** High
- **Modernization Path:** Migrate to .NET 10

The entire application is built on .NET Framework 4.8.1, which:
- Requires Windows hosting environment
- Cannot run in Linux containers efficiently
- Is in maintenance mode (no new features)
- Lacks modern framework features

#### 2. ASP.NET MVC (System.Web)
- **Impact:** Tied to Windows/IIS, incompatible with containerization
- **Files Affected:** 
  - `ProductCatalog/Controllers/HomeController.cs`
  - `ProductCatalog/Global.asax.cs`
  - `ProductCatalog/App_Start/*.cs`
  - `ProductCatalog/Views/**/*.cshtml`
- **Effort:** High
- **Modernization Path:** Migrate to ASP.NET Core MVC

Key issues:
- Dependent on System.Web namespace (Windows-only)
- Uses Global.asax for application lifecycle
- Session management tied to in-memory state
- Incompatible with Kestrel web server

#### 3. WCF (Windows Communication Foundation)
- **Impact:** Not supported in .NET Core/5+, limited cross-platform support
- **Files Affected:**
  - `ProductServiceLibrary/IProductService.cs`
  - `ProductServiceLibrary/ProductService.cs`
  - `ProductCatalog/Connected Services/ProductServiceReference/*`
- **Effort:** Medium
- **Modernization Path:** Migrate to gRPC

WCF limitations:
- No .NET Core/.NET 5+ support (server-side)
- Complex configuration
- Verbose XML-based communication
- Not containerization-friendly

#### 4. MSMQ (Microsoft Message Queuing)
- **Impact:** Windows-only, not available in Azure Container Apps
- **Files Affected:**
  - `ProductCatalog/Services/OrderQueueService.cs`
- **Effort:** Medium
- **Modernization Path:** Migrate to Azure Service Bus

MSMQ is fundamentally incompatible with cloud-native architectures:
- Requires Windows Server with MSMQ role installed
- Local queues cannot span containers
- No cloud-managed equivalent
- Limited monitoring and management capabilities

### 🟡 Medium Priority Issues

#### 5. Classic csproj Format
- **Impact:** Verbose, not compatible with .NET Core/5+
- **Effort:** Low
- **Modernization Path:** Convert to SDK-style csproj

The old-style csproj format:
- Lists every file explicitly
- Incompatible with .NET Core/5+
- More difficult to maintain
- Lacks modern MSBuild features

#### 6. In-Memory Session State
- **Impact:** Not suitable for distributed/containerized environments
- **Files Affected:**
  - `ProductCatalog/Controllers/HomeController.cs`
- **Effort:** Medium
- **Modernization Path:** Use distributed cache (Redis)

Current session implementation:
```csharp
var cart = Session["Cart"] as List<CartItem>;
```

Problems in containerized environment:
- Session lost when container restarts
- Cannot share session across multiple instances
- No persistence across scale events

#### 7. packages.config
- **Impact:** Old NuGet package management
- **Effort:** Low
- **Modernization Path:** Use PackageReference in csproj

---

## Target State Architecture

### Technology Stack (Target)

| Component | Target Technology | Version | Benefits |
|-----------|------------------|---------|----------|
| Framework | .NET | 10.0 | Modern, cross-platform, high performance |
| Web Framework | ASP.NET Core MVC | 10.0 | Container-friendly, Kestrel-based |
| Service Layer | gRPC | - | High performance, modern RPC |
| Messaging | Azure Service Bus | - | Cloud-native, managed, scalable |
| Session State | Redis (Distributed) | - | Shared across instances |
| Project Format | SDK-style | - | Clean, maintainable |

### Deployment Architecture

```
Azure Container Apps
├── Web Container (ASP.NET Core MVC)
│   ├── Ingress: HTTP/HTTPS
│   ├── Session: Redis Cache
│   └── Dapr Sidecar: Pub/Sub
├── Service Container (gRPC)
│   └── Internal gRPC endpoint
└── Dapr Components
    └── Azure Service Bus (pub/sub)
```

---

## Migration Roadmap

### Phase 1: Foundation Modernization (Critical Priority)
**Estimated Effort:** 13 story points (~2-3 weeks)

#### Tasks:
1. **Convert to SDK-style csproj** (Low effort, Low risk)
   - Use `dotnet try-convert` tool
   - Manually adjust if needed
   - Remove packages.config, use PackageReference

2. **Upgrade to .NET 10** (High effort, Medium risk)
   - Create new .NET 10 projects
   - Port code file by file
   - Update namespaces and APIs
   - Handle breaking changes

3. **Migrate ASP.NET MVC to ASP.NET Core MVC** (High effort, Medium risk)
   - Convert controllers (remove System.Web dependencies)
   - Update views (minimal changes expected)
   - Replace Global.asax with Startup/Program.cs
   - Migrate App_Start configuration to ASP.NET Core middleware
   - Update routing to ASP.NET Core routing

**Key Changes:**
- `System.Web.Mvc.Controller` → `Microsoft.AspNetCore.Mvc.Controller`
- `Global.asax` → `Program.cs` with WebApplication builder
- `Web.config` → `appsettings.json`
- `FilterConfig`, `RouteConfig`, `BundleConfig` → Middleware configuration

### Phase 2: Service Communication Modernization (High Priority)
**Estimated Effort:** 8 story points (~1-2 weeks)

#### Tasks:
1. **Create gRPC service definitions** (Medium effort, Low risk)
   - Define .proto files from WCF contracts
   - Generate service stubs
   
   Example:
   ```protobuf
   service ProductService {
     rpc GetAllProducts(Empty) returns (ProductList);
     rpc GetProductById(ProductIdRequest) returns (Product);
     rpc CreateProduct(Product) returns (Product);
   }
   ```

2. **Implement gRPC service** (Medium effort, Low risk)
   - Port WCF service implementation to gRPC
   - Update ProductRepository for .NET 10

3. **Update client to gRPC** (Medium effort, Low risk)
   - Replace WCF client with gRPC client
   - Update HomeController to use gRPC

**Migration Example:**
```csharp
// Before (WCF)
using (var client = new ProductServiceClient())
{
    products = client.GetAllProducts().ToList();
}

// After (gRPC)
var client = new ProductService.ProductServiceClient(_grpcChannel);
var response = await client.GetAllProductsAsync(new Empty());
products = response.Products.ToList();
```

### Phase 3: Messaging Modernization (High Priority)
**Estimated Effort:** 8 story points (~1-2 weeks)

#### Tasks:
1. **Replace MSMQ with Azure Service Bus** (Medium effort, Medium risk)
   - Choose approach: Direct SDK or Dapr pub/sub
   - Recommended: Use Dapr for abstraction

2. **Reimplement OrderQueueService** (Medium effort, Low risk)
   - Replace System.Messaging with Azure.Messaging.ServiceBus
   - Maintain similar interface for minimal code changes

3. **Update order flow** (Low effort, Low risk)
   - Test order submission and processing

**Migration Example:**
```csharp
// Before (MSMQ)
using System.Messaging;
var queue = new MessageQueue(_queuePath);
queue.Send(new Message(order));

// After (Azure Service Bus with Dapr)
await _daprClient.PublishEventAsync("pubsub", "orders", order);

// Or Direct Azure Service Bus
await _serviceBusClient.SendMessageAsync(
    new ServiceBusMessage(JsonSerializer.Serialize(order)));
```

### Phase 4: State Management Modernization (Medium Priority)
**Estimated Effort:** 3 story points (~2-3 days)

#### Tasks:
1. **Configure distributed session with Redis** (Low effort, Low risk)
   ```csharp
   builder.Services.AddStackExchangeRedisCache(options =>
   {
       options.Configuration = configuration["Redis:ConnectionString"];
   });
   builder.Services.AddSession(options =>
   {
       options.IdleTimeout = TimeSpan.FromMinutes(30);
       options.Cookie.IsEssential = true;
   });
   ```

2. **Update session usage** (Low effort, Low risk)
   - Code changes minimal (HttpContext.Session remains the same)
   - Session serialization requirements (ensure cart is serializable)

### Phase 5: Containerization (High Priority)
**Estimated Effort:** 2 story points (~1-2 days)

#### Tasks:
1. **Create Dockerfiles** (Low effort, Low risk)
   
   Example Dockerfile:
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
   WORKDIR /app
   EXPOSE 8080

   FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
   WORKDIR /src
   COPY ["ProductCatalog/ProductCatalog.csproj", "ProductCatalog/"]
   RUN dotnet restore
   COPY . .
   RUN dotnet build -c Release -o /app/build

   FROM build AS publish
   RUN dotnet publish -c Release -o /app/publish

   FROM base AS final
   WORKDIR /app
   COPY --from=publish /app/publish .
   ENTRYPOINT ["dotnet", "ProductCatalog.dll"]
   ```

2. **Create Azure Container Apps manifests** (Medium effort, Medium risk)
   - Define container apps YAML
   - Configure Dapr components
   - Set up secrets for Service Bus connection string
   - Configure ingress rules

---

## Risk Assessment

### Overall Risk Level: 🟡 Medium

### Identified Risks

| Risk | Severity | Mitigation Strategy |
|------|----------|---------------------|
| ASP.NET MVC to Core migration complexity | Medium | Follow Microsoft's official migration guide; use incremental approach; thorough testing |
| WCF to gRPC API contract changes | Low | Create proto files that match WCF contracts closely; version APIs if needed |
| MSMQ to Service Bus semantic differences | Medium | Map MSMQ patterns carefully; use Dapr for abstraction; extensive testing |
| Session state loss during transition | Low | Accept session reset or plan migration window; use Redis for persistence |
| Learning curve for new technologies | Medium | Provide training; reference documentation; pair programming |

---

## Estimated Effort

### Total Effort: 34 Story Points

**Breakdown by Phase:**
- Phase 1 (Foundation): 13 points
- Phase 2 (gRPC): 8 points
- Phase 3 (Messaging): 8 points
- Phase 4 (State): 3 points
- Phase 5 (Containers): 2 points

**Timeline:** 4-6 weeks with 1-2 developers

**Velocity Assumptions:**
- 1 story point ≈ 4-6 hours of development
- Includes testing and documentation
- Assumes familiarity with .NET ecosystem

---

## Benefits Analysis

### Technical Benefits

✅ **Cross-platform deployment** - Run on Linux containers (cost savings)  
✅ **Modern framework** - .NET 10 with latest features and performance  
✅ **Better performance** - gRPC is faster than WCF, native async/await  
✅ **Cloud-native messaging** - Azure Service Bus with retry, dead-letter, etc.  
✅ **Container orchestration** - Azure Container Apps handles scaling, health checks  
✅ **Improved developer experience** - Modern tooling, better debugging  

### Business Benefits

💰 **Reduced hosting costs** - Linux containers are ~70% cheaper than Windows  
📈 **Better scalability** - Container Apps scales automatically based on load  
🔒 **Improved security** - Regular .NET updates, modern security practices  
🚀 **Faster deployments** - Container-based CI/CD pipelines  
🔮 **Future-proof** - Technology stack supported for years to come  

### Cost Comparison

| Aspect | Current (Windows) | Target (Linux Containers) | Savings |
|--------|------------------|---------------------------|---------|
| Compute | Windows VM/App Service | Azure Container Apps (Linux) | ~60-70% |
| Messaging | Windows Server + MSMQ | Azure Service Bus | Managed service |
| Maintenance | Higher (Windows updates, MSMQ) | Lower (PaaS services) | ~40% time |

---

## Next Steps

### Immediate Actions

1. ✅ **Review this assessment** with stakeholders
2. ✅ **Approve migration plan** and timeline
3. 📋 **Set up development environment** with .NET 10 SDK
4. 📋 **Create Azure resources**:
   - Azure Service Bus namespace
   - Azure Container Registry
   - Azure Cache for Redis
   - Azure Container Apps environment
5. 📋 **Begin Phase 1** - Foundation modernization

### Long-term Recommendations

- **Implement CI/CD** - Automated build and deployment pipelines
- **Add monitoring** - Application Insights for observability
- **Consider microservices** - Further decompose application if needed
- **Implement API versioning** - For backward compatibility
- **Add automated tests** - Unit tests, integration tests, E2E tests
- **Document architecture** - Architecture decision records (ADRs)

---

## Resources

### Microsoft Documentation
- [Migrate from .NET Framework to .NET 10](https://learn.microsoft.com/en-us/dotnet/core/porting/)
- [Migrate WCF to gRPC](https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/)
- [Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/)
- [Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)

### Tools
- [try-convert](https://github.com/dotnet/try-convert) - Convert to SDK-style projects
- [.NET Upgrade Assistant](https://dotnet.microsoft.com/en-us/platform/upgrade-assistant) - Automated migration tool
- [CoreWCF](https://github.com/CoreWCF/CoreWCF) - Alternative to full gRPC migration (if needed)

### Dapr Resources
- [Dapr pub/sub](https://docs.dapr.io/developing-applications/building-blocks/pubsub/)
- [Dapr with Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/dapr-overview)

---

## Conclusion

This modernization project is **highly recommended** and **technically feasible**. The current codebase is relatively small (~1,800 LOC) and well-structured, making it an ideal candidate for modernization.

**Key Success Factors:**
- Clear migration path for each legacy component
- Strong Microsoft tooling and documentation support
- Manageable scope (4-6 weeks)
- Significant long-term benefits (cost, performance, maintainability)

**Recommendation:** Proceed with migration following the phased approach outlined in this assessment.

---

*Assessment completed by: GitHub Copilot AI Agent*  
*For questions or clarification, please open an issue in the repository.*
