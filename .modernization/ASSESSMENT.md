# Modernization Assessment for ProductCatalogApp

**Assessment Date:** January 17, 2026  
**Target Framework:** .NET 10  
**Target Platform:** Azure Container Apps  
**Complexity:** Medium  
**Estimated Effort:** 2-4 weeks

---

## Executive Summary

This application is a .NET Framework 4.8.1 product catalog system consisting of an ASP.NET MVC 5 web application and a WCF service library. The modernization effort will migrate this to .NET 10, replacing legacy technologies with modern cloud-native equivalents suitable for deployment to Azure Container Apps.

### Key Migration Targets

| Current Technology | Target Technology | Priority |
|-------------------|-------------------|----------|
| .NET Framework 4.8.1 | .NET 10 | High |
| ASP.NET MVC 5 | ASP.NET Core MVC | High |
| WCF Service | gRPC | High |
| MSMQ | Azure Service Bus | High |
| Web.config | appsettings.json | Medium |
| In-memory Session | Distributed Cache (Redis) | Medium |

---

## Current State Analysis

### Architecture Overview

The application consists of two projects:

1. **ProductCatalog** - ASP.NET MVC 5 web application
   - Provides product browsing and shopping cart functionality
   - Communicates with backend via WCF service reference
   - Uses MSMQ for order processing
   - Stores cart state in ASP.NET Session

2. **ProductServiceLibrary** - WCF Service Library
   - Exposes product CRUD operations via WCF
   - Uses in-memory data repository
   - Defines service contracts with WCF attributes

### Technology Stack

#### Web Application (ProductCatalog)
- **Framework:** .NET Framework 4.8.1
- **Web Framework:** ASP.NET MVC 5.2.9
- **Project Type:** Legacy .csproj (non-SDK style)
- **Service Communication:** WCF Client (ProductServiceReference)
- **Messaging:** MSMQ (System.Messaging)
- **Session State:** In-memory ASP.NET Session
- **Configuration:** Web.config
- **Dependencies:** packages.config

#### Service Layer (ProductServiceLibrary)
- **Framework:** .NET Framework 4.8.1
- **Service Framework:** WCF (Windows Communication Foundation)
- **Project Type:** WCF Service Library
- **Data Access:** In-memory repository
- **Configuration:** App.config

---

## Identified Issues and Migration Requirements

### 🔴 High Priority Issues

#### 1. WCF Service Migration
**Location:** `ProductServiceLibrary/` (entire project)  
**Impact:** Complete service rewrite required

The WCF service library must be migrated to gRPC:
- Define `.proto` file for service contracts
- Implement gRPC service methods
- Host in ASP.NET Core application
- Update client to use gRPC instead of WCF

**Files Affected:**
- `IProductService.cs` - ServiceContract interface
- `ProductService.cs` - Service implementation
- `App.config` - WCF configuration

#### 2. WCF Client Migration
**Location:** `ProductCatalog/Controllers/HomeController.cs`  
**Impact:** Replace WCF service references with gRPC client

Current code uses WCF proxy:
```csharp
using (var client = new ProductServiceClient())
{
    products = client.GetAllProducts().ToList();
}
```

Must be replaced with gRPC client calls.

#### 3. MSMQ to Azure Service Bus Migration
**Location:** `ProductCatalog/Services/OrderQueueService.cs`  
**Impact:** Complete messaging infrastructure replacement

Current implementation:
- Uses `System.Messaging` namespace (Windows-only)
- Creates local MSMQ queues
- Uses XML message formatting

Required changes:
- Replace with `Azure.Messaging.ServiceBus` SDK
- Update configuration for connection strings
- Implement sender/receiver pattern
- Add support for managed identity
- Handle message serialization differences

**Current Code Pattern:**
```csharp
using (MessageQueue queue = new MessageQueue(_queuePath))
{
    queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Order) });
    queue.Send(message);
}
```

#### 4. ASP.NET MVC to ASP.NET Core Migration
**Location:** `ProductCatalog/` (entire project)  
**Impact:** Major framework migration

Required changes:
- Convert project to SDK-style .csproj
- Update namespace imports (System.Web.Mvc → Microsoft.AspNetCore.Mvc)
- Replace Global.asax with Startup.cs/Program.cs
- Update views with new tag helpers
- Rewrite routing configuration
- Migrate bundling and minification
- Update HTTP context access patterns

### 🟡 Medium Priority Issues

#### 5. Session State Management
**Location:** `ProductCatalog/Controllers/HomeController.cs`  
**Impact:** Shopping cart functionality needs redesign

Current usage of in-memory session:
```csharp
var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
Session["Cart"] = cart;
```

**Challenges:**
- In-memory session doesn't work across container instances
- Container Apps may scale to multiple replicas
- Session data will be lost on container restart

**Recommended Solution:**
- Implement distributed cache using Redis
- Configure distributed session state
- Alternative: Move to stateless design with client-side storage

#### 6. Configuration System
**Location:** `Web.config`, `App.config`  
**Impact:** Configuration management modernization

Migrate from:
- `Web.config` / `App.config` XML-based configuration
- `ConfigurationManager.AppSettings`

To:
- `appsettings.json` and environment-specific files
- `IConfiguration` interface with strongly-typed options
- Azure Key Vault for secrets (production)

#### 7. Project Format
**Location:** Both `.csproj` files  
**Impact:** Project file modernization

Current format:
- Legacy .csproj with verbose XML
- packages.config for NuGet dependencies
- Explicit file listings

Target format:
- SDK-style .csproj (concise)
- PackageReference for NuGet
- Implicit file inclusion

### 🟢 Low Priority Issues

#### 8. Static Content and Bundling
**Location:** `ProductCatalog/App_Start/BundleConfig.cs`  
**Impact:** Update bundling approach

ASP.NET Core uses different bundling/minification approaches. Consider:
- Using built-in static file middleware
- WebOptimizer library
- Or modern frontend build tools (webpack, vite)

#### 9. Authentication & Authorization
**Current State:** None implemented  
**Recommendation:** Consider adding authentication for production

Recommended additions:
- Microsoft Identity for authentication
- Azure AD integration
- Role-based authorization

---

## Modernization Roadmap

### Phase 1: Service Layer Migration (Week 1)
**Goal:** Migrate WCF service to gRPC

**Tasks:**
1. Create new .NET 10 ASP.NET Core project for gRPC service
2. Define `.proto` file with service contracts
3. Implement gRPC service methods
4. Port business logic from ProductService.cs
5. Set up gRPC service hosting
6. Test service endpoints

**Deliverables:**
- Working gRPC service on .NET 10
- Containerized service with Dockerfile
- Integration tests

### Phase 2: Messaging Migration (Week 2)
**Goal:** Replace MSMQ with Azure Service Bus

**Tasks:**
1. Create Azure Service Bus namespace and queue
2. Install Azure.Messaging.ServiceBus NuGet package
3. Create IMessageQueueService interface
4. Implement Azure Service Bus sender
5. Implement Azure Service Bus receiver
6. Update configuration system
7. Add managed identity support
8. Test message flow end-to-end

**Deliverables:**
- Service Bus sender/receiver implementation
- Configuration templates
- Integration tests with actual Azure Service Bus

### Phase 3: Web Application Migration (Week 3)
**Goal:** Migrate ASP.NET MVC to ASP.NET Core

**Tasks:**
1. Create new .NET 10 ASP.NET Core MVC project
2. Port controllers to ASP.NET Core
3. Update views and layouts
4. Replace WCF client with gRPC client
5. Implement distributed session state
6. Migrate configuration to appsettings.json
7. Update routing and middleware
8. Test all user flows

**Deliverables:**
- ASP.NET Core MVC application on .NET 10
- All features working with new backend
- Containerized application with Dockerfile

### Phase 4: Containerization & Deployment (Week 4)
**Goal:** Deploy to Azure Container Apps

**Tasks:**
1. Optimize Dockerfiles for both applications
2. Create docker-compose.yml for local testing
3. Set up Azure Container Registry
4. Create Container App environments
5. Configure ingress and scaling
6. Set up Service Bus connections with managed identity
7. Configure distributed cache (Redis)
8. Create CI/CD pipeline
9. Deploy to Azure Container Apps
10. Perform end-to-end testing

**Deliverables:**
- Production-ready container images
- Azure Container Apps deployment
- CI/CD pipeline
- Monitoring and logging setup

---

## Required Azure Services

### 1. Azure Container Apps
**Purpose:** Host both web application and gRPC service  
**Configuration:**
- **Environment:** Single environment for both apps
- **Ingress:** External HTTPS for web app, internal for gRPC service
- **Scaling:** HTTP-based auto-scaling (1-10 replicas)
- **Resources:** 0.5 CPU, 1 GB RAM per replica (adjustable)

### 2. Azure Service Bus
**Purpose:** Replace MSMQ for order processing  
**Configuration:**
- **SKU:** Standard (or Premium for enhanced features)
- **Queue:** `product-catalog-orders`
- **Features:** 
  - Dead-letter queue for failed messages
  - Message sessions (if needed for ordering)
  - At-least-once delivery guarantee

### 3. Azure Container Registry
**Purpose:** Store and manage container images  
**Configuration:**
- **SKU:** Basic or Standard
- **Features:** 
  - Private repository
  - Integrated with Container Apps
  - Webhook for CI/CD

### 4. Azure Redis Cache (Optional but Recommended)
**Purpose:** Distributed session state for shopping cart  
**Configuration:**
- **SKU:** Basic (development) or Standard (production)
- **Size:** C1 (1 GB) minimum
- **Features:** SSL-only connections

### 5. Azure Application Insights (Recommended)
**Purpose:** Monitoring, logging, and diagnostics  
**Configuration:**
- Application map
- Performance monitoring
- Exception tracking
- Custom telemetry

---

## Dependencies Update

### Dependencies to Remove
- ✖️ Microsoft.AspNet.Mvc 5.2.9 → Part of ASP.NET Core
- ✖️ System.ServiceModel (WCF) → Replace with gRPC
- ✖️ System.Messaging (MSMQ) → Replace with Service Bus
- ✖️ Microsoft.AspNet.* packages → ASP.NET Core equivalents

### Dependencies to Add
- ✅ **Grpc.AspNetCore** - gRPC service and client support
- ✅ **Azure.Messaging.ServiceBus** (7.x) - Service Bus SDK
- ✅ **Azure.Identity** - Managed identity for Azure services
- ✅ **Microsoft.Extensions.Caching.StackExchangeRedis** - Redis cache
- ✅ **Google.Protobuf** - Protocol buffers (dependency of gRPC)
- ✅ **Grpc.Tools** - Code generation for .proto files

### Dependencies to Evaluate
- **Newtonsoft.Json 13.0.3** - Consider using System.Text.Json (built-in)
  - Keep if complex serialization scenarios exist
  - Migrate to System.Text.Json for better performance if possible

---

## Risks and Mitigation Strategies

### Risk 1: Session State in Stateless Containers
**Severity:** Medium  
**Issue:** In-memory session doesn't work with multiple container replicas

**Mitigation:**
- Implement distributed cache using Redis
- Configure session state to use Redis as backing store
- Test with multiple replicas to ensure cart persistence
- Alternative: Redesign to stateless architecture with client-side storage

### Risk 2: WCF to gRPC Protocol Breaking Changes
**Severity:** High  
**Issue:** gRPC uses different serialization and communication patterns than WCF

**Mitigation:**
- Define clear .proto contracts matching WCF interfaces
- Run both WCF and gRPC services in parallel during transition
- Use feature flags to gradually switch clients
- Thorough integration testing between client and service
- Document any API changes

### Risk 3: MSMQ to Service Bus Message Format
**Severity:** Medium  
**Issue:** XML serialization vs JSON/binary serialization differences

**Mitigation:**
- Ensure Order model serializes correctly in Service Bus
- Test message round-trip (send and receive)
- Implement proper error handling and dead-letter queue monitoring
- Consider message version tracking for future compatibility

### Risk 4: View Rendering Differences
**Severity:** Low  
**Issue:** ASP.NET Core Razor has subtle differences from ASP.NET MVC 5

**Mitigation:**
- Test all views after migration
- Update HTML helpers to tag helpers
- Check for any custom HTML helpers that need porting
- Verify JavaScript bundling and minification

### Risk 5: Cold Start Performance
**Severity:** Low  
**Issue:** Container Apps may have cold start latency

**Mitigation:**
- Configure minimum replicas (at least 1)
- Implement proper health checks
- Optimize container image size
- Consider using readiness probes

---

## Testing Strategy

### Unit Testing
**Current State:** No unit tests exist  
**Recommendation:** Add unit tests for business logic

**Focus Areas:**
- ProductRepository operations
- OrderQueueService/Service Bus sender (with mocks)
- Controller logic (isolated from external dependencies)

### Integration Testing
**Required Tests:**
- gRPC service endpoint testing
- Service Bus message send/receive
- End-to-end order submission flow
- Session state persistence across requests

### Migration Testing
**Verify Feature Parity:**
- All product listing and filtering features work
- Shopping cart add/update/remove operations
- Order submission and queuing
- View rendering matches expected layout

### Performance Testing
**Load Test Scenarios:**
- Concurrent user sessions
- Container scaling behavior
- Service Bus throughput
- Cache hit rates for distributed session

---

## Key Recommendations

### 1. Migration Sequence
✅ **Start with the service layer (WCF → gRPC)** - This is the foundation  
✅ **Implement Service Bus early** - Allows testing messaging patterns  
✅ **Migrate web app last** - Depends on service layer being complete  

### 2. Development Practices
- Use feature flags for gradual migration
- Set up CI/CD pipeline early for continuous deployment
- Implement comprehensive logging from the start
- Use Application Insights for monitoring

### 3. Security & Operations
- Use managed identity instead of connection strings
- Store secrets in Azure Key Vault
- Implement health checks for all services
- Set up alerts for Service Bus dead-letter queue
- Enable Azure Defender for containers (production)

### 4. Scalability Considerations
- Design for stateless operation where possible
- Use distributed cache for any shared state
- Configure appropriate auto-scaling rules
- Monitor resource utilization and adjust container resources

### 5. Additional Enhancements (Optional)
- Add authentication/authorization (Azure AD)
- Implement API versioning for gRPC service
- Add database persistence (EF Core) instead of in-memory data
- Implement CQRS pattern for better scalability
- Add OpenTelemetry for distributed tracing

---

## Cost Considerations

### Development Environment
- **Container Apps:** ~$0-30/month (consumption-based with generous free tier)
- **Service Bus Standard:** ~$10/month (includes 12.5M operations)
- **Container Registry Basic:** ~$5/month
- **Redis Cache C0 (dev):** ~$16/month

**Estimated Dev Cost:** $30-60/month

### Production Environment
- **Container Apps:** ~$50-200/month (depends on scaling)
- **Service Bus Standard:** ~$10-50/month (based on usage)
- **Container Registry Standard:** ~$20/month
- **Redis Cache C1:** ~$75/month
- **Application Insights:** ~$2-10/month (based on volume)

**Estimated Production Cost:** $150-350/month

*Note: Actual costs depend on usage patterns, scaling configuration, and region*

---

## Next Steps

1. ✅ **Review this assessment** with stakeholders
2. ✅ **Prioritize features** for migration vs. redesign
3. ⬜ **Set up Azure resources** (Resource Group, Service Bus, Container Registry)
4. ⬜ **Create .NET 10 projects** with SDK-style format
5. ⬜ **Begin Phase 1** (Service Layer Migration)
6. ⬜ **Establish CI/CD pipeline** early in the process
7. ⬜ **Iterate through phases** with regular testing and validation

---

## Conclusion

The ProductCatalogApp is a good candidate for modernization to .NET 10 and Azure Container Apps. The **medium complexity** rating reflects the need to migrate multiple legacy technologies (WCF, MSMQ, ASP.NET MVC), but the application's relatively straightforward architecture and small codebase make it achievable in **2-4 weeks** with focused effort.

The migration will result in a **modern, cloud-native application** that is:
- ✅ Cross-platform and containerized
- ✅ Scalable with auto-scaling capabilities
- ✅ Cost-effective with consumption-based pricing
- ✅ Maintainable with modern .NET practices
- ✅ Secure with managed identity and Azure integration

**Recommended Approach:** Follow the phased roadmap, starting with the service layer, then messaging, then the web application, and finally containerization and deployment. This approach minimizes risk and allows for incremental testing and validation.

---

**Assessment prepared by:** GitHub Copilot  
**Version:** 1.0  
**Last updated:** January 17, 2026
