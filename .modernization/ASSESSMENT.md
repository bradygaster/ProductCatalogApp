# ProductCatalogApp Modernization Assessment

**Assessment Date:** 2026-01-10  
**Target Platform:** Azure Container Apps  
**Current Framework:** .NET Framework 4.8.1  
**Target Framework:** .NET 9.0

---

## Executive Summary

The ProductCatalogApp is a classic .NET Framework application built with legacy Windows-specific technologies that require comprehensive modernization to deploy to Azure Container Apps. The application consists of three main components:

1. **ProductCatalog** - ASP.NET MVC 5 web application
2. **ProductServiceLibrary** - WCF service for product data operations
3. **OrderProcessor** - Console application for MSMQ-based order processing

**Overall Complexity Score: 7/10**

The migration is rated as **HIGH COMPLEXITY** due to multiple architectural blockers including WCF services, MSMQ queuing, and ASP.NET MVC framework dependencies. While the business logic is relatively straightforward, the technological foundations require significant rearchitecture.

**Estimated Effort:** 80-160 hours (8-13 weeks with proper testing)

---

## Current Architecture

### Technology Stack

| Component | Technology | Version | Status |
|-----------|-----------|---------|--------|
| Web Application | ASP.NET MVC | 5.2.9 | ❌ Legacy |
| Framework | .NET Framework | 4.8.1 | ❌ Windows-only |
| Service Layer | WCF | 4.8.1 | ❌ Not cloud-native |
| Messaging | MSMQ | - | ❌ Windows-only |
| Session State | In-Memory | - | ⚠️ Not distributed |
| Data Storage | In-Memory | - | ⚠️ No persistence |
| Project Format | Legacy .csproj | - | ⚠️ Outdated |

### Application Components

#### 1. ProductCatalog (Web Application)
- **Purpose:** Customer-facing product catalog and shopping cart
- **Technology:** ASP.NET MVC 5.2.9 on .NET Framework 4.8.1
- **Key Features:**
  - Product browsing and search
  - Shopping cart management (session-based)
  - Order submission via MSMQ
  - Razor views with Bootstrap 5 UI
- **Dependencies:**
  - WCF client for ProductService
  - System.Messaging for MSMQ
  - System.Web for ASP.NET pipeline

#### 2. ProductServiceLibrary (WCF Service)
- **Purpose:** Service layer for product data operations
- **Technology:** WCF Service Library on .NET Framework 4.8.1
- **Key Features:**
  - 9 service operations for product management
  - In-memory product repository
  - Category management
  - CRUD operations for products
- **Service Contract:**
  - `GetAllProducts()` - Retrieve all products
  - `GetProductById()` - Get single product
  - `GetProductsByCategory()` - Filter by category
  - `SearchProducts()` - Search functionality
  - `GetCategories()` - List categories
  - `CreateProduct()`, `UpdateProduct()`, `DeleteProduct()` - CRUD operations
  - `GetProductsByPriceRange()` - Price filtering

#### 3. OrderProcessor (Console Application)
- **Purpose:** Background order processing
- **Technology:** Console App on .NET Framework 4.8.1
- **Key Features:**
  - Continuous MSMQ queue monitoring
  - Order deserialization and processing
  - Simulated payment, inventory, and shipping operations
  - Console-based status output

---

## Critical Blockers for Azure Container Apps

### 🚫 High-Severity Blockers

#### 1. WCF Services
**Impact:** CRITICAL  
**Reason:** WCF is not supported in .NET Core/.NET 5+ and is Windows-specific. Cannot run in Linux containers.

**Current Implementation:**
```csharp
// ProductServiceLibrary uses WCF
[ServiceContract]
public interface IProductService
{
    [OperationContract]
    List<Product> GetAllProducts();
    // ... 8 more operations
}
```

**Migration Required:**
- Replace WCF with REST API using ASP.NET Core Web API
- Alternative: Use gRPC for high-performance service-to-service communication
- Update client in ProductCatalog to use HttpClient or gRPC client

**Effort:** Medium (2-3 weeks)

#### 2. MSMQ (Microsoft Message Queuing)
**Impact:** CRITICAL  
**Reason:** MSMQ is Windows-only and not available in containers or cloud platforms. Not cloud-native.

**Current Implementation:**
```csharp
// OrderQueueService.cs uses MSMQ
using (MessageQueue queue = new MessageQueue(_queuePath))
{
    queue.Send(message);
}
```

**Migration Required:**
- Replace with **Azure Service Bus** (recommended for Azure)
- Alternative: Azure Storage Queues, RabbitMQ, or Apache Kafka
- Update OrderQueueService to use Azure.Messaging.ServiceBus SDK
- Migrate OrderProcessor to .NET Worker Service with Service Bus consumer

**Effort:** Medium (1-2 weeks)

#### 3. ASP.NET MVC (Non-Core)
**Impact:** CRITICAL  
**Reason:** ASP.NET MVC 5 is built on System.Web, which is Windows-only and not available in .NET Core/.NET 5+.

**Current Implementation:**
- Uses `System.Web.Mvc` namespace
- Razor views with legacy syntax
- Session state via `System.Web.SessionState`
- Global.asax for application lifecycle

**Migration Required:**
- Migrate to ASP.NET Core MVC
- Update Razor views to ASP.NET Core syntax
- Replace System.Web dependencies with ASP.NET Core equivalents
- Update routing, filters, and middleware pipeline

**Effort:** High (3-4 weeks)

#### 4. .NET Framework 4.8.1
**Impact:** HIGH  
**Reason:** Requires Windows containers, which are significantly larger, slower to start, and more expensive than Linux containers.

**Current State:**
- All projects target .NET Framework 4.8.1
- Uses legacy .csproj format
- Windows Server container base images required

**Migration Required:**
- Migrate all projects to .NET 9.0 (or .NET 8 LTS)
- Convert to SDK-style project format
- Update NuGet packages to .NET 9-compatible versions
- Test compatibility and fix breaking changes

**Effort:** Medium-High (2-3 weeks)

### ⚠️ Medium-Severity Issues

#### 5. In-Memory Session State
**Impact:** MEDIUM  
**Reason:** In-memory sessions don't work with multiple container instances. Session data is lost when containers restart.

**Current Implementation:**
```csharp
// HomeController.cs uses in-memory session
Session["Cart"] = cart;
var cart = Session["Cart"] as List<CartItem>;
```

**Migration Required:**
- Implement distributed session state with Redis
- Use Azure Cache for Redis
- Update session configuration in Startup.cs
- Consider stateless patterns where possible

**Effort:** Low (1 week)

#### 6. No Data Persistence
**Impact:** MEDIUM  
**Reason:** In-memory product data is lost on container restart. Not suitable for production.

**Current Implementation:**
```csharp
// ProductRepository.cs stores data in memory
private static List<Product> _products = new List<Product>();
```

**Migration Required:**
- Implement database persistence layer
- Choose database: Azure SQL, PostgreSQL, or Cosmos DB
- Implement Entity Framework Core
- Add repository pattern with EF Core
- Create database migrations

**Effort:** Medium (1-2 weeks)

---

## Target Architecture for Azure Container Apps

### Proposed Service Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     Azure Container Apps                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌───────────────────────────┐         ┌──────────────────────┐ │
│  │  product-catalog-web      │────────▶│  product-service-api │ │
│  │  (ASP.NET Core MVC)       │  HTTP   │  (ASP.NET Core API)  │ │
│  │  .NET 9.0                 │         │  .NET 9.0            │ │
│  │  External Ingress:443     │         │  Internal Only:8080  │ │
│  │  Scale: 1-10 replicas     │         │  Scale: 1-5 replicas │ │
│  └───────────────────────────┘         └──────────────────────┘ │
│              │                                     │              │
│              │ Publish                    Read/Write             │
│              ▼                                     ▼              │
│  ┌───────────────────────────┐         ┌──────────────────────┐ │
│  │  Azure Service Bus        │         │  Azure SQL Database  │ │
│  │  (Order Queue)            │         │  or PostgreSQL       │ │
│  └───────────────────────────┘         └──────────────────────┘ │
│              │                                                    │
│              │ Consume                                            │
│              ▼                                                    │
│  ┌───────────────────────────┐         ┌──────────────────────┐ │
│  │  order-processor          │────────▶│  Azure Redis Cache   │ │
│  │  (.NET Worker Service)    │         │  (Session State)     │ │
│  │  .NET 9.0                 │         └──────────────────────┘ │
│  │  No Ingress               │                                   │
│  │  Scale: 1-3 replicas      │         ┌──────────────────────┐ │
│  └───────────────────────────┘         │ Application Insights │ │
│                                         │  (Monitoring/Logs)   │ │
│                                         └──────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### Target Services

#### 1. product-catalog-web (Container App)
- **Framework:** ASP.NET Core MVC on .NET 9.0
- **Base Image:** `mcr.microsoft.com/dotnet/aspnet:9.0-alpine`
- **Ingress:** External HTTPS on port 443
- **Scaling:** 1-10 replicas (CPU/Memory based)
- **Environment Variables:**
  - `ProductServiceUrl`: Internal URL to product-service-api
  - `ServiceBusConnectionString`: Connection to Azure Service Bus
  - `RedisConnectionString`: Connection to Azure Cache for Redis

#### 2. product-service-api (Container App)
- **Framework:** ASP.NET Core Web API on .NET 9.0
- **Base Image:** `mcr.microsoft.com/dotnet/aspnet:9.0-alpine`
- **Ingress:** Internal only on port 8080
- **Scaling:** 1-5 replicas
- **Environment Variables:**
  - `DatabaseConnectionString`: Connection to Azure SQL or PostgreSQL
  - `ApplicationInsights__InstrumentationKey`: For telemetry

#### 3. order-processor (Container App)
- **Framework:** .NET Worker Service on .NET 9.0
- **Base Image:** `mcr.microsoft.com/dotnet/runtime:9.0-alpine`
- **Ingress:** None (background worker)
- **Scaling:** 1-3 replicas (queue length based)
- **Environment Variables:**
  - `ServiceBusConnectionString`: Connection to Azure Service Bus
  - `DatabaseConnectionString`: For order persistence

### Azure Resources Required

| Resource | Purpose | SKU/Tier |
|----------|---------|----------|
| Azure Container Apps Environment | Host all containers | Consumption |
| Azure Service Bus Namespace | Message queue for orders | Standard |
| Azure Cache for Redis | Distributed session state | Basic/Standard |
| Azure SQL Database / PostgreSQL | Product and order data | Basic/Standard |
| Azure Container Registry | Store container images | Basic |
| Application Insights | Monitoring and logging | - |
| Azure Key Vault | Secrets management | Standard |

---

## Migration Plan

### Phase 1: Assessment and Planning (1 week)
**Status:** ✅ COMPLETE (this document)

- [x] Analyze current architecture
- [x] Identify legacy patterns and blockers
- [x] Create detailed migration plan
- [x] Document target architecture
- [x] Estimate effort and timeline

### Phase 2: Core Framework Migration (2-3 weeks)

#### 2.1 Migrate ProductServiceLibrary to ASP.NET Core Web API
- [ ] Create new ASP.NET Core Web API project (.NET 9.0)
- [ ] Convert WCF service to REST API controllers
- [ ] Implement RESTful endpoints for all operations
- [ ] Add OpenAPI/Swagger documentation
- [ ] Test API functionality

**API Endpoints:**
```
GET    /api/products              - GetAllProducts
GET    /api/products/{id}         - GetProductById
GET    /api/products/category/{category} - GetProductsByCategory
GET    /api/products/search?term={term}  - SearchProducts
GET    /api/categories            - GetCategories
POST   /api/products              - CreateProduct
PUT    /api/products/{id}         - UpdateProduct
DELETE /api/products/{id}         - DeleteProduct
GET    /api/products/price?min={min}&max={max} - GetProductsByPriceRange
```

#### 2.2 Migrate ProductCatalog to ASP.NET Core MVC
- [ ] Create new ASP.NET Core MVC project (.NET 9.0)
- [ ] Migrate controllers and action methods
- [ ] Update Razor views to ASP.NET Core syntax
- [ ] Migrate static files (CSS, JS, images)
- [ ] Update routing configuration
- [ ] Replace System.Web dependencies
- [ ] Implement HttpClient for API calls (replace WCF client)
- [ ] Configure distributed session state with Redis
- [ ] Test web application functionality

### Phase 3: Messaging and Background Processing (1-2 weeks)

#### 3.1 Replace MSMQ with Azure Service Bus
- [ ] Provision Azure Service Bus namespace
- [ ] Create queue for orders
- [ ] Update OrderQueueService to use Azure.Messaging.ServiceBus
- [ ] Update message serialization (JSON instead of XML)
- [ ] Implement error handling and retry policies
- [ ] Test message send/receive

#### 3.2 Migrate OrderProcessor to Worker Service
- [ ] Create .NET Worker Service project (.NET 9.0)
- [ ] Implement IHostedService for background processing
- [ ] Integrate Azure Service Bus consumer
- [ ] Migrate order processing logic
- [ ] Add structured logging
- [ ] Test background processing

### Phase 4: Data Persistence (1-2 weeks)

#### 4.1 Design Database Schema
- [ ] Design product and category tables
- [ ] Design order and order item tables
- [ ] Create ER diagram
- [ ] Document relationships and constraints

#### 4.2 Implement Entity Framework Core
- [ ] Add EF Core NuGet packages
- [ ] Create DbContext and entity models
- [ ] Configure entity relationships
- [ ] Create initial migration
- [ ] Implement repository pattern
- [ ] Replace in-memory repository with EF Core
- [ ] Add seed data
- [ ] Test database operations

### Phase 5: Containerization (1 week)

#### 5.1 Create Dockerfiles
- [ ] Create Dockerfile for product-catalog-web
- [ ] Create Dockerfile for product-service-api
- [ ] Create Dockerfile for order-processor
- [ ] Optimize image sizes (multi-stage builds)
- [ ] Test local container builds

#### 5.2 Docker Compose Setup
- [ ] Create docker-compose.yml for local development
- [ ] Include SQL Server/PostgreSQL container
- [ ] Include Redis container
- [ ] Configure networking between containers
- [ ] Test end-to-end locally

### Phase 6: Azure Container Apps Deployment (1-2 weeks)

#### 6.1 Provision Azure Resources
- [ ] Create Azure resource group
- [ ] Provision Azure Container Apps environment
- [ ] Provision Azure Service Bus namespace and queue
- [ ] Provision Azure Cache for Redis
- [ ] Provision Azure SQL Database or PostgreSQL
- [ ] Create Azure Container Registry
- [ ] Configure Application Insights
- [ ] Set up Azure Key Vault for secrets

#### 6.2 Deploy Container Apps
- [ ] Push container images to ACR
- [ ] Deploy product-service-api Container App
- [ ] Deploy product-catalog-web Container App
- [ ] Deploy order-processor Container App
- [ ] Configure ingress rules and custom domains
- [ ] Configure scaling rules
- [ ] Set up environment variables and secrets
- [ ] Test deployed applications

#### 6.3 Configure Monitoring and Logging
- [ ] Enable Application Insights integration
- [ ] Configure structured logging
- [ ] Set up alerts and dashboards
- [ ] Implement health check endpoints
- [ ] Configure distributed tracing

### Phase 7: Testing and Production Readiness (1-2 weeks)

#### 7.1 Comprehensive Testing
- [ ] Functional testing of all features
- [ ] Integration testing across services
- [ ] Load testing and performance validation
- [ ] Security scanning and vulnerability assessment
- [ ] Disaster recovery testing

#### 7.2 Production Hardening
- [ ] Implement authentication and authorization
- [ ] Configure SSL/TLS certificates
- [ ] Set up backup and recovery procedures
- [ ] Create runbooks for operations
- [ ] Update documentation
- [ ] Conduct security review
- [ ] Performance optimization
- [ ] Final production deployment

---

## Effort Estimation

### Summary by Component

| Component | Current | Target | Complexity | Effort (hours) |
|-----------|---------|--------|------------|----------------|
| ProductCatalog | ASP.NET MVC 5 | ASP.NET Core MVC | High | 40-60 |
| ProductServiceLibrary | WCF | ASP.NET Core API | Medium | 20-30 |
| OrderProcessor | Console + MSMQ | Worker + Service Bus | Medium | 15-20 |
| Data Persistence | In-Memory | EF Core + Database | Medium | 15-25 |
| Containerization | None | Docker + ACR | Low | 10-15 |
| Azure Deployment | None | Container Apps | Medium | 20-30 |
| **TOTAL** | - | - | **High** | **120-180** |

### Timeline Breakdown

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| 1. Assessment and Planning | 1 week | None |
| 2. Core Framework Migration | 2-3 weeks | Phase 1 |
| 3. Messaging & Background | 1-2 weeks | Phase 2 |
| 4. Data Persistence | 1-2 weeks | Phase 2 |
| 5. Containerization | 1 week | Phases 2-4 |
| 6. Azure Deployment | 1-2 weeks | Phase 5 |
| 7. Testing & Production | 1-2 weeks | Phase 6 |
| **Total Duration** | **8-13 weeks** | - |

---

## Risk Assessment

### High-Risk Areas

#### 1. Message Loss During MSMQ Migration
**Risk Level:** HIGH  
**Impact:** Orders could be lost during transition

**Mitigation Strategy:**
- Implement dual-write pattern during transition period
- Run parallel systems with message reconciliation
- Extensive testing with synthetic orders
- Rollback plan with MSMQ as fallback
- Monitor queue depths in both systems

#### 2. Session State Loss
**Risk Level:** MEDIUM  
**Impact:** Users may lose shopping cart data

**Mitigation Strategy:**
- Implement distributed session state early
- Consider cookie-based cart as fallback
- Graceful degradation for missing sessions
- Clear communication to users during migration

#### 3. WCF Service Contract Breaking Changes
**Risk Level:** MEDIUM  
**Impact:** API compatibility issues between old and new services

**Mitigation Strategy:**
- Maintain API contract compatibility
- Version API endpoints
- Use feature flags for gradual rollout
- Comprehensive integration testing

### Medium-Risk Areas

#### 4. Performance Degradation
**Risk Level:** MEDIUM  
**Impact:** Application may be slower in containers

**Mitigation Strategy:**
- Performance testing before and after migration
- Proper resource allocation (CPU, memory)
- Container optimization (image size, startup time)
- Application Performance Monitoring (APM)

#### 5. Database Migration Issues
**Risk Level:** LOW  
**Impact:** In-memory to database transition could reveal data issues

**Mitigation Strategy:**
- Design robust schema with proper constraints
- Extensive testing with various data scenarios
- Database backups and rollback procedures
- Gradual rollout with monitoring

---

## Benefits of Migration

### Technical Benefits

1. **Cross-Platform Compatibility**
   - Run on Linux containers (lighter and faster)
   - No Windows Server licensing costs
   - Better container density

2. **Modern .NET 9.0 Performance**
   - 30-40% performance improvements over .NET Framework
   - Smaller memory footprint
   - Faster startup times

3. **Cloud-Native Architecture**
   - Designed for distributed systems
   - Native container support
   - Better scalability and resilience

4. **Improved Development Experience**
   - Modern tooling and IDE support
   - Active community and ecosystem
   - Regular updates and security patches

5. **Better Observability**
   - Built-in distributed tracing
   - Structured logging
   - Rich telemetry support

### Business Benefits

1. **Reduced Infrastructure Costs**
   - Linux containers cost ~50% less than Windows containers
   - Better resource utilization
   - Pay-per-use pricing model

2. **Improved Scalability**
   - Auto-scaling based on demand
   - Handle traffic spikes automatically
   - Global distribution capabilities

3. **Enhanced Reliability**
   - Built-in health checks and auto-healing
   - Multi-region deployment options
   - Improved disaster recovery

4. **Faster Time to Market**
   - Modern CI/CD pipelines
   - Rapid deployment and rollback
   - Feature flag support

5. **Future-Proof Technology**
   - Long-term support (.NET 8 LTS, .NET 9)
   - Active Microsoft investment
   - Growing ecosystem

---

## Recommendations

### Immediate Actions

1. ✅ **Complete this assessment** (DONE)
2. ⏭️ **Set up development environment** with .NET 9 SDK
3. ⏭️ **Provision Azure resources** for development/staging
4. ⏭️ **Start Phase 2** - Framework migration

### Technology Choices

| Decision | Recommendation | Rationale |
|----------|---------------|-----------|
| Target Framework | .NET 9.0 | Latest features, best performance |
| Service Communication | REST API (HTTP/JSON) | Simple, widely supported, good for this use case |
| Message Queue | Azure Service Bus | Native Azure integration, reliable, managed |
| Session State | Azure Cache for Redis | Distributed, fast, Azure-managed |
| Database | Azure SQL Database | Compatible, managed, good tooling |
| Monitoring | Application Insights | Deep Azure integration, comprehensive telemetry |

### Alternative Considerations

- **gRPC instead of REST**: Consider if performance is critical
- **PostgreSQL instead of Azure SQL**: If preferring open source
- **Azure Storage Queues**: Simpler alternative to Service Bus for basic queuing
- **.NET 8 LTS**: If long-term stability preferred over latest features

---

## Success Criteria

### Technical Metrics

- ✅ All applications run on .NET 9.0
- ✅ All services containerized and deployed to Azure Container Apps
- ✅ No WCF or MSMQ dependencies
- ✅ Data persisted in database (not in-memory)
- ✅ Distributed session state implemented
- ✅ Health checks implemented for all services
- ✅ Application Insights monitoring active

### Performance Metrics

- Container startup time < 10 seconds
- API response time < 200ms (p95)
- Order processing time < 5 seconds
- Auto-scaling responds within 2 minutes
- Application availability > 99.9%

### Business Metrics

- Infrastructure cost reduction of 30-50%
- Deployment frequency increased 10x
- Mean time to recovery (MTTR) < 15 minutes
- Zero data loss during migration
- User experience maintained or improved

---

## Conclusion

The ProductCatalogApp requires a comprehensive modernization effort to deploy to Azure Container Apps. The primary challenges are:

1. **WCF Services** → REST API migration
2. **MSMQ** → Azure Service Bus migration  
3. **ASP.NET MVC** → ASP.NET Core migration
4. **.NET Framework 4.8.1** → .NET 9.0 migration

While the complexity is rated **7/10 (HIGH)**, the benefits significantly outweigh the effort:

- **50% cost reduction** from Linux containers
- **Modern, maintainable codebase**
- **Cloud-native scalability and resilience**
- **Future-proof technology stack**

**Estimated Total Effort:** 120-180 hours (8-13 weeks)

With proper planning, phased execution, and risk mitigation, this migration will position the application for long-term success in the cloud.

---

## Next Steps

1. Review and approve this assessment
2. Allocate resources and establish timeline
3. Set up Azure subscriptions and development environments
4. Begin Phase 2: Core Framework Migration
5. Regular progress reviews and adjustments

---

**Assessment Completed By:** GitHub Copilot Agent  
**Date:** 2026-01-10  
**Version:** 1.0
