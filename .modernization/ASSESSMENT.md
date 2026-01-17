# Product Catalog App - Modernization Assessment

**Assessment Date:** January 17, 2026  
**Repository:** bradygaster/ProductCatalogApp  
**Branch:** modernize/assess

---

## Executive Summary

This assessment evaluates the Product Catalog App for modernization from .NET Framework 4.8.1 to .NET 10 with deployment to Azure Container Apps. The application currently consists of an ASP.NET MVC 5 web application and a WCF service library, using MSMQ for order processing.

### Current State
- **Framework:** .NET Framework 4.8.1
- **Web:** ASP.NET MVC 5
- **Services:** WCF (Windows Communication Foundation)
- **Messaging:** MSMQ (Microsoft Message Queuing)
- **Deployment:** IIS on Windows Server
- **Session State:** In-memory

### Target State
- **Framework:** .NET 10
- **Web:** ASP.NET Core MVC
- **Services:** gRPC
- **Messaging:** Azure Service Bus
- **Deployment:** Azure Container Apps
- **Session State:** Azure Redis Cache (distributed)

### Effort Estimate
- **Complexity:** Medium-High
- **Duration:** 15-20 days
- **Risk Level:** Medium

---

## Findings

### Legacy Patterns Identified

#### 1. .NET Framework 4.8.1 → .NET 10 ⚠️ HIGH IMPACT
**Location:**
- `ProductCatalog/ProductCatalog.csproj`
- `ProductServiceLibrary/ProductServiceLibrary.csproj`

**Issue:** Application is using .NET Framework 4.8.1, which is Windows-only and does not support modern container deployment.

**Recommendation:** Migrate to .NET 10 for cross-platform support, better performance, and modern features.

**Migration Steps:**
1. Convert to SDK-style project format
2. Update target framework to `net10.0`
3. Replace .NET Framework-specific APIs with .NET equivalents
4. Update all NuGet packages to .NET 10-compatible versions

---

#### 2. ASP.NET MVC 5 → ASP.NET Core MVC ⚠️ HIGH IMPACT
**Location:**
- `ProductCatalog/ProductCatalog.csproj`
- `ProductCatalog/Controllers/HomeController.cs`
- `ProductCatalog/Global.asax.cs`

**Issue:** Using ASP.NET MVC 5, which is legacy and not compatible with .NET Core.

**Recommendation:** Migrate to ASP.NET Core MVC for modern web development.

**Migration Steps:**
1. Replace `Global.asax` with `Program.cs` and `Startup.cs`
2. Convert `Web.config` to `appsettings.json`
3. Replace `System.Web` dependencies
4. Update controller actions and filters
5. Migrate Razor views (minimal changes needed)
6. Update session state handling

**Breaking Changes:**
- Session state requires distributed cache in container environment
- TempData storage needs configuration
- Authentication/authorization model changed

---

#### 3. WCF → gRPC ⚠️ HIGH IMPACT
**Location:**
- `ProductServiceLibrary/IProductService.cs`
- `ProductServiceLibrary/ProductService.cs`
- `ProductServiceLibrary/App.config`
- `ProductCatalog/Web.config` (client configuration)

**Issue:** Using WCF for service communication, which is not fully supported in .NET Core/.NET 5+.

**Recommendation:** Migrate to gRPC for modern, high-performance service communication.

**Affected Operations:**
- `GetAllProducts`
- `GetProductById`
- `GetProductsByCategory`
- `SearchProducts`
- `GetCategories`
- `CreateProduct`
- `UpdateProduct`
- `DeleteProduct`
- `GetProductsByPriceRange`

**Migration Steps:**
1. Define service contract in `.proto` files
2. Generate gRPC code from proto definitions
3. Implement gRPC service methods
4. Update client to use gRPC channel and client
5. Configure gRPC in ASP.NET Core
6. Add gRPC-Web if browser access is needed

**Alternative:** REST API with ASP.NET Core Web API
- **Pros:** Simpler migration, HTTP-based, wide tooling support
- **Cons:** Less efficient than gRPC, more manual serialization

---

#### 4. MSMQ → Azure Service Bus ⚠️ HIGH IMPACT
**Location:**
- `ProductCatalog/Services/OrderQueueService.cs`
- `ProductCatalog/Controllers/HomeController.cs` (line 197-198)

**Issue:** Using MSMQ for order queue processing, which is Windows-only and not suitable for cloud/container deployment.

**Recommendation:** Migrate to Azure Service Bus for cloud-native messaging.

**Current Usage:**
- **Queue Path:** `.\Private$\ProductCatalogOrders`
- **Operations:** SendOrder, ReceiveOrder, GetQueueMessageCount
- **Message Type:** Order (XML serialized)

**Migration Mapping:**
| MSMQ | Azure Service Bus |
|------|-------------------|
| `MessageQueue` | `ServiceBusClient` |
| `MessageQueue.Send()` | `ServiceBusSender.SendMessageAsync()` |
| `MessageQueue.Receive()` | `ServiceBusReceiver.ReceiveMessageAsync()` |
| `Message.Body` | `ServiceBusMessage.Body` |
| Queue path | Queue name + connection string |

**Azure Resources Needed:**
- **Service Bus Namespace:** productcatalog-servicebus
- **Queue Name:** orders
- **SKU:** Standard

**Migration Steps:**
1. Create Azure Service Bus namespace and queue
2. Replace `System.Messaging` with `Azure.Messaging.ServiceBus`
3. Update `OrderQueueService` to use `ServiceBusClient`
4. Configure connection string in configuration
5. Update message serialization (JSON instead of XML)
6. Implement retry policies and error handling

---

#### 5. Legacy .csproj Format → SDK-style ⚠️ MEDIUM IMPACT
**Location:**
- `ProductCatalog/ProductCatalog.csproj`
- `ProductServiceLibrary/ProductServiceLibrary.csproj`

**Issue:** Using old-style verbose .csproj format with explicit file listings.

**Recommendation:** Convert to SDK-style project format for simpler management.

**Benefits:**
- Simpler and more readable project files
- Automatic file inclusion with wildcards
- Better multi-targeting support
- Integrated NuGet package management

---

#### 6. In-Memory Session State → Azure Redis Cache ⚠️ MEDIUM IMPACT
**Location:**
- `ProductCatalog/Controllers/HomeController.cs` (Session usage)
- `ProductCatalog/Web.config`

**Issue:** Using in-memory session state which won't work across multiple container instances.

**Recommendation:** Implement distributed cache using Azure Redis Cache.

**Session Usage:**
- **Data:** Cart (List<CartItem>)
- **Controllers:** HomeController

**Azure Resources Needed:**
- **Redis Cache:** productcatalog-cache
- **SKU:** Basic C0 (250 MB)

**Migration Steps:**
1. Add `Microsoft.Extensions.Caching.StackExchangeRedis` package
2. Configure Redis in `appsettings.json`
3. Update session configuration in `Program.cs`
4. Test session persistence across instances

---

#### 7. Web.config / App.config → appsettings.json ⚠️ MEDIUM IMPACT
**Location:**
- `ProductCatalog/Web.config`
- `ProductServiceLibrary/App.config`

**Issue:** Using XML-based configuration files which are replaced by JSON in .NET Core.

**Recommendation:** Migrate to `appsettings.json` and environment variables.

**Configuration Items to Migrate:**
- OrderQueuePath
- WCF service endpoints
- Session state configuration
- Compilation settings
- HTTP runtime settings

---

## Dependencies

### Legacy Packages to Remove
| Package | Replacement | Notes |
|---------|-------------|-------|
| `System.Web` | `Microsoft.AspNetCore.Http` | Core ASP.NET functionality |
| `System.Web.Mvc` | `Microsoft.AspNetCore.Mvc` | MVC framework |
| `System.ServiceModel` | `Grpc.AspNetCore` | WCF replacement |
| `System.Messaging` | `Azure.Messaging.ServiceBus` | MSMQ replacement |
| `Microsoft.AspNet.WebPages` | Built into ASP.NET Core | Razor pages support |
| `Microsoft.Web.Infrastructure` | Not needed | Legacy infrastructure |
| `Newtonsoft.Json` | `System.Text.Json` | Built-in JSON serialization |

### Modern Packages to Add
| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.Mvc` | 10.0.0 | ASP.NET Core MVC framework |
| `Grpc.AspNetCore` | 2.60.0+ | gRPC service hosting |
| `Azure.Messaging.ServiceBus` | 7.17.0+ | Azure Service Bus client |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | 10.0.0 | Redis distributed cache |
| `Grpc.Tools` | 2.60.0+ | gRPC code generation |
| `Google.Protobuf` | 3.25.0+ | Protocol Buffers support |

---

## Recommendations

### Immediate Actions
1. **HIGH PRIORITY:** Create .NET 10 project structure - Foundation for all other migrations
2. **HIGH PRIORITY:** Setup development environment with .NET 10 SDK - Required for development
3. **HIGH PRIORITY:** Create proto files for gRPC service definition - Defines the new service contract
4. **MEDIUM PRIORITY:** Setup Azure Service Bus namespace - Required for MSMQ migration testing

### Phased Migration Plan

#### Phase 1: Foundation Migration (3-5 days)
- Convert projects to SDK-style format
- Migrate to .NET 10
- Update NuGet packages
- Resolve compilation errors

#### Phase 2: Service Layer Migration (4-6 days)
- Define gRPC proto files
- Implement gRPC service
- Create gRPC client
- Test service operations

#### Phase 3: Messaging Migration (2-3 days)
- Create Azure Service Bus resources
- Update OrderQueueService to use Service Bus
- Update message serialization
- Test message flow

#### Phase 4: Web Application Migration (4-5 days)
- Migrate to ASP.NET Core MVC
- Update controllers and views
- Configure distributed session state
- Update configuration system

#### Phase 5: Containerization (2-3 days)
- Create Dockerfiles
- Configure for Azure Container Apps
- Setup health checks
- Test container deployments

---

## Risks & Mitigations

### Technical Risks
| Risk | Severity | Mitigation |
|------|----------|-----------|
| WCF to gRPC migration may require protocol changes | Medium | Run both services in parallel during transition, use adapter pattern |
| Session state migration may require code changes | Low | Test thoroughly with distributed cache, consider cookie-based state |
| MSMQ message format incompatibility | Low | Update serialization to JSON, ensure structure compatibility |

### Operational Risks
| Risk | Severity | Mitigation |
|------|----------|-----------|
| Azure resource costs | Low | Start with Basic/Standard tiers, monitor usage, implement cost alerts |
| Container deployment learning curve | Low | Follow Azure Container Apps documentation, use Azure samples |

---

## Azure Infrastructure

### Required Resources
1. **Azure Container Apps Environment**
   - Name: productcatalog-env
   - SKU: Consumption
   - Cost: $0.000024/vCPU-second + $0.000003/GiB-second

2. **Azure Container App (Web)**
   - Name: productcatalog-web
   - Config: 1-10 replicas, 0.5 CPU, 1Gi memory

3. **Azure Container App (Service)**
   - Name: productcatalog-service
   - Config: 1-5 replicas, 0.5 CPU, 1Gi memory

4. **Azure Service Bus Namespace**
   - Name: productcatalog-servicebus
   - SKU: Standard
   - Cost: $0.05/million operations (after 13M free/month)

5. **Azure Cache for Redis**
   - Name: productcatalog-cache
   - SKU: Basic C0 (250 MB)
   - Cost: ~$11.52/month

6. **Azure Container Registry**
   - Name: productcatalogacr
   - SKU: Basic
   - Cost: $5/month + storage

### Estimated Monthly Cost
- **Minimum:** $25-35/month (low usage)
- **Typical:** $50-75/month (medium usage)
- **Note:** Actual costs vary with traffic and usage patterns

---

## Code Metrics

- **Total Projects:** 2
- **Total Files:** 16
- **Estimated Lines of Code:** 2,500
- **Controllers:** 1
- **Services:** 2
- **Models:** 4
- **Views:** 5
- **WCF Operations:** 9

---

## Tooling Requirements

### Required Tools
- .NET 10 SDK
- Docker Desktop
- Azure CLI
- Visual Studio 2022 or VS Code with C# extension

### Recommended Tools
- Azure Service Bus Explorer
- Redis Insight
- Postman or gRPCurl for API testing
- Azure Storage Explorer

---

## Conclusion

The Product Catalog App is a good candidate for modernization to .NET 10 and Azure Container Apps. The application follows a relatively clean architecture with separation of concerns between the web layer, service layer, and messaging components.

The main challenges are:
1. Migrating from WCF to gRPC (medium-high complexity)
2. Replacing MSMQ with Azure Service Bus (medium complexity)
3. Converting to ASP.NET Core MVC (high effort but well-documented)
4. Implementing distributed session state (medium effort)

With a phased approach and the recommended 15-20 day timeline, this modernization is achievable with medium risk. The resulting application will be cloud-native, containerized, and ready for deployment to Azure Container Apps with improved performance, scalability, and maintainability.

---

**Next Steps:**
1. Review and approve this assessment
2. Setup .NET 10 development environment
3. Create Azure resources for development/testing
4. Begin Phase 1: Foundation Migration
