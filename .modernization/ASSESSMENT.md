# Modernization Assessment: ProductCatalogApp

**Assessment Date:** January 14, 2026  
**Repository:** bradygaster/ProductCatalogApp  
**Target:** Upgrade to .NET 10 and deploy to Azure Container Apps

---

## Executive Summary

The ProductCatalogApp is a legacy .NET Framework 4.8.1 application consisting of an ASP.NET MVC 5 web application and a WCF service library. To achieve the goal of upgrading to .NET 10 and deploying to Azure Container Apps, **a complete rewrite is necessary** due to fundamental incompatibilities with the target platform.

### Key Findings

| Category | Current State | Cloud Readiness |
|----------|--------------|-----------------|
| **Framework** | .NET Framework 4.8.1 | ❌ Not Compatible |
| **Web Framework** | ASP.NET MVC 5.2.9 | ❌ Legacy |
| **Service Layer** | WCF | ❌ Not Supported in .NET Core/10 |
| **Messaging** | MSMQ | ❌ Windows-Only (Blocker) |
| **Data Storage** | In-Memory | ❌ Not Persistent |
| **Containerization** | None | ❌ Not Ready |
| **Estimated Effort** | 6-8 weeks | 🟡 Medium-High Complexity |

**Overall Cloud Readiness Score: 2/10** (Low)

---

## Current Architecture

### Project Structure

```
ProductCatalogApp/
├── ProductCatalog/                    # ASP.NET MVC 5 Web Application
│   ├── Controllers/
│   │   └── HomeController.cs         # MVC controllers
│   ├── Models/
│   │   ├── CartItem.cs
│   │   └── Order.cs
│   ├── Services/
│   │   └── OrderQueueService.cs      # MSMQ integration
│   ├── Views/                         # Razor views
│   ├── Web.config                     # IIS configuration
│   └── ProductCatalog.csproj          # .NET Framework 4.8.1
│
└── ProductServiceLibrary/             # WCF Service Library
    ├── IProductService.cs             # WCF service contract
    ├── ProductService.cs              # Service implementation
    ├── ProductRepository.cs           # In-memory data
    ├── Product.cs, Category.cs        # Data models
    └── ProductServiceLibrary.csproj   # .NET Framework 4.8.1
```

### Technology Stack

#### Backend
- **.NET Framework 4.8.1** - Legacy framework, incompatible with modern cloud platforms
- **WCF (Windows Communication Foundation)** - Legacy SOAP-based services, not supported in .NET Core/10
- **MSMQ (Microsoft Message Queuing)** - Windows-specific messaging, cannot run in containers
- **In-Memory Repository** - No database, all data in static collections

#### Frontend
- **ASP.NET MVC 5.2.9** - Legacy web framework
- **Razor Views (.cshtml)** - Template engine (compatible with .NET Core)
- **jQuery 3.7.0** - JavaScript library (functional but legacy pattern)
- **Bootstrap 5.2.3** - CSS framework (modern, compatible)

#### Hosting
- **IIS/IIS Express** - Windows-specific web server
- **No containerization** - Not ready for Azure Container Apps

---

## Critical Legacy Patterns

### 🔴 **LP001: WCF Services (HIGH SEVERITY)**

**Description:** The application uses Windows Communication Foundation (WCF) for its service layer with basicHttpBinding.

**Location:** `ProductServiceLibrary` project

**Impact:**
- WCF is **not supported in .NET Core or .NET 10**
- SOAP-based communication is outdated
- Requires complete rewrite to REST APIs

**Recommendation:** Migrate to **ASP.NET Core Web API** with RESTful endpoints
- Convert service contracts to API controllers
- Use JSON instead of XML serialization
- Implement OpenAPI/Swagger for documentation

**Effort:** High (2-3 weeks)

---

### 🔴 **LP002: MSMQ Messaging (CRITICAL SEVERITY)**

**Description:** Uses Microsoft Message Queuing (MSMQ) for asynchronous order processing.

**Location:** `ProductCatalog/Services/OrderQueueService.cs`

**Impact:**
- MSMQ is **Windows-specific** and not available in Linux containers
- **Blocker for Azure Container Apps deployment**
- System.Messaging not available in .NET Core/10

**Recommendation:** Replace with **Azure Service Bus Queues**
- Cloud-native messaging service
- Cross-platform support
- Better reliability and scalability
- Alternative: RabbitMQ, Azure Event Hubs, or Apache Kafka

**Effort:** Medium (1-2 weeks)

---

### 🔴 **LP003: ASP.NET MVC 5 (HIGH SEVERITY)**

**Description:** Web application built on ASP.NET MVC 5, which is part of .NET Framework.

**Location:** `ProductCatalog` project

**Impact:**
- ASP.NET MVC 5 cannot run on .NET Core/10
- IIS-specific dependencies
- Web.config not used in .NET Core

**Recommendation:** Migrate to **ASP.NET Core 10 MVC or Razor Pages**
- Port controllers and views
- Update configuration to appsettings.json
- Use Kestrel web server (built-in)

**Effort:** High (1-2 weeks)

---

### 🔴 **LP004: In-Memory Data Storage (HIGH SEVERITY)**

**Description:** All product and category data stored in static in-memory collections.

**Location:** `ProductServiceLibrary/ProductRepository.cs`

```csharp
private static List<Product> _products = new List<Product>();
private static readonly object _lock = new object();
```

**Impact:**
- No data persistence
- Data lost on restart
- Not scalable across multiple instances
- Locks don't work in distributed environments

**Recommendation:** Implement proper database
- **Azure SQL Database** (recommended for relational data)
- **Azure Cosmos DB** (for NoSQL approach)
- Use **Entity Framework Core** for data access
- Add database migrations

**Effort:** Medium (1-2 weeks)

---

### 🟡 **LP005: In-Memory Session State (MEDIUM SEVERITY)**

**Description:** Shopping cart stored in ASP.NET in-memory session state.

**Location:** `ProductCatalog/Controllers/HomeController.cs`

```csharp
var cart = Session["Cart"] as List<CartItem>;
```

**Impact:**
- Sessions not shared across container instances
- Users lose cart when scaled horizontally
- Not cloud-ready

**Recommendation:** Use distributed session provider
- **Azure Cache for Redis** (recommended)
- Configure Redis distributed caching in ASP.NET Core
- Alternative: Store cart in database with user ID

**Effort:** Low (2-3 days)

---

### 🟡 **LP006: Web.config Configuration (LOW SEVERITY)**

**Description:** Configuration stored in XML-based Web.config file.

**Location:** `ProductCatalog/Web.config`

**Impact:**
- Web.config not used in .NET Core/10
- Hard-coded values not suitable for cloud
- No environment-specific configuration

**Recommendation:** Migrate to modern configuration
- Use `appsettings.json` and `appsettings.{Environment}.json`
- Use **environment variables** for container configuration
- Store secrets in **Azure Key Vault**
- Use **managed identities** for Azure authentication

**Effort:** Low (1-2 days)

---

### 🔴 **LP009: No Containerization (CRITICAL SEVERITY)**

**Description:** No Docker support or container configuration present.

**Location:** Repository root

**Impact:**
- Cannot deploy to Azure Container Apps
- No container registry setup
- No container orchestration

**Recommendation:** Create containerization infrastructure
- Create **multi-stage Dockerfile** for .NET 10
- Build optimized container images
- Set up **Azure Container Registry**
- Create docker-compose for local development

**Effort:** Medium (1 week)

---

## Modernization Strategy

### Approach: Complete Rewrite

Due to the fundamental incompatibilities between .NET Framework 4.8.1 and .NET 10, combined with the use of Windows-specific technologies (WCF, MSMQ), **a complete rewrite is necessary**.

### Why Rewrite Instead of Migrate?

1. **WCF has no migration path** - Must be rewritten as REST API
2. **MSMQ is Windows-only** - Cannot run in containers
3. **ASP.NET MVC 5 structure** is significantly different from ASP.NET Core
4. **Small codebase** - Easier to rewrite than large applications
5. **No existing tests** - No regression risk from rewrite

---

## Recommended Architecture (.NET 10 + Azure Container Apps)

```
┌─────────────────────────────────────────────────────────────┐
│                     Azure Container Apps                     │
├──────────────────────────┬──────────────────────────────────┤
│   Web App Container      │    API Container                 │
│   (ASP.NET Core MVC)     │    (ASP.NET Core Web API)        │
│   - Razor Pages/Views    │    - REST endpoints              │
│   - Client-side logic    │    - Business logic              │
└──────────┬───────────────┴──────────────┬──────────────────┘
           │                               │
           ├───────────────────────────────┘
           │
    ┌──────┴───────┐        ┌──────────────┐        ┌─────────────┐
    │ Azure Service│        │ Azure SQL    │        │Azure Cache  │
    │    Bus       │        │   Database   │        │ for Redis   │
    │ (Queues)     │        │ (Products)   │        │ (Sessions)  │
    └──────────────┘        └──────────────┘        └─────────────┘
           │
           │
    ┌──────┴───────────┐    ┌────────────────┐
    │ Background Worker│    │  Application   │
    │   Container      │    │   Insights     │
    │ (Order Processor)│    │  (Monitoring)  │
    └──────────────────┘    └────────────────┘
```

### New Technology Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | .NET 10 |
| **Web API** | ASP.NET Core Web API (REST) |
| **Web App** | ASP.NET Core MVC / Razor Pages |
| **Database** | Azure SQL Database + Entity Framework Core |
| **Messaging** | Azure Service Bus Queues |
| **Caching** | Azure Cache for Redis |
| **Hosting** | Azure Container Apps |
| **Monitoring** | Application Insights |
| **CI/CD** | GitHub Actions |
| **Container Registry** | Azure Container Registry |

---

## Migration Phases

### **Phase 1: Foundation Setup (Week 1)**

**Objective:** Create new .NET 10 solution structure

- [ ] Create new .NET 10 solution
- [ ] Set up ASP.NET Core Web API project
- [ ] Set up ASP.NET Core Web App project (MVC)
- [ ] Configure dependency injection
- [ ] Set up structured logging
- [ ] Configure Application Insights

**Deliverables:** New solution with empty projects, builds successfully

---

### **Phase 2: Data Layer Migration (Week 2)**

**Objective:** Implement persistent data storage

- [ ] Choose database (Azure SQL Database recommended)
- [ ] Create data models (Product, Category, Order, OrderItem)
- [ ] Set up Entity Framework Core
- [ ] Create database migrations
- [ ] Implement repository pattern with EF Core
- [ ] Seed sample data
- [ ] Add connection string configuration

**Deliverables:** Working database with CRUD operations

---

### **Phase 3: Service Layer Migration (Week 3)**

**Objective:** Convert WCF to REST API

- [ ] Design REST API endpoints
  - `GET /api/products` - Get all products
  - `GET /api/products/{id}` - Get product by ID
  - `GET /api/products/category/{category}` - Get by category
  - `GET /api/categories` - Get categories
  - `POST /api/products` - Create product
  - `PUT /api/products/{id}` - Update product
  - `DELETE /api/products/{id}` - Delete product
- [ ] Implement API controllers
- [ ] Add input validation
- [ ] Implement error handling
- [ ] Add Swagger/OpenAPI documentation
- [ ] Add API versioning
- [ ] Write unit tests

**Deliverables:** Fully functional REST API

---

### **Phase 4: Messaging Migration (Week 4)**

**Objective:** Replace MSMQ with Azure Service Bus

- [ ] Create Azure Service Bus namespace
- [ ] Create "orders" queue
- [ ] Implement Service Bus sender (from web app)
- [ ] Implement Service Bus receiver (background worker)
- [ ] Add retry policies and error handling
- [ ] Implement dead-letter queue handling
- [ ] Test order flow end-to-end

**Deliverables:** Working async order processing

---

### **Phase 5: Web Application Migration (Week 5)**

**Objective:** Migrate ASP.NET MVC 5 to ASP.NET Core

- [ ] Port Views to ASP.NET Core Razor
- [ ] Update Controllers to use API client
- [ ] Implement distributed sessions with Redis
- [ ] Update configuration to appsettings.json
- [ ] Update frontend JavaScript (remove jQuery if desired)
- [ ] Test shopping cart functionality
- [ ] Test order submission flow
- [ ] Add health check endpoints

**Deliverables:** Working web application

---

### **Phase 6: Containerization (Week 6)**

**Objective:** Create Docker containers

- [ ] Create multi-stage Dockerfile for API
- [ ] Create multi-stage Dockerfile for Web App
- [ ] Create Dockerfile for background worker
- [ ] Create docker-compose for local development
- [ ] Test containers locally
- [ ] Optimize image sizes
- [ ] Create Azure Container Registry
- [ ] Push images to ACR

**Deliverables:** Containerized applications

---

### **Phase 7: Azure Deployment (Week 7)**

**Objective:** Deploy to Azure Container Apps

- [ ] Create Azure Container Apps Environment
- [ ] Deploy API container app
- [ ] Deploy Web App container app
- [ ] Deploy background worker container app
- [ ] Configure Azure Service Bus integration
- [ ] Set up Redis Cache
- [ ] Configure Application Insights
- [ ] Set up managed identities
- [ ] Configure environment variables
- [ ] Set up custom domain (optional)
- [ ] Configure scaling rules

**Deliverables:** Running application in Azure

---

### **Phase 8: CI/CD & Polish (Week 8)**

**Objective:** Automate deployment and finalize

- [ ] Create GitHub Actions workflow
  - Build containers
  - Run tests
  - Push to ACR
  - Deploy to Container Apps
- [ ] Set up monitoring and alerts
- [ ] Configure log aggregation
- [ ] Performance testing
- [ ] Security scanning
- [ ] Create runbooks
- [ ] Final testing and validation

**Deliverables:** Fully automated deployment pipeline

---

## Required Azure Services

| Service | Purpose | Priority | Estimated Cost |
|---------|---------|----------|----------------|
| **Azure Container Apps** | Host web app, API, and worker | Critical | $30-100/month |
| **Azure Service Bus** | Message queue (replace MSMQ) | Critical | $10-50/month |
| **Azure SQL Database** | Persistent data storage | Critical | $5-200/month |
| **Azure Cache for Redis** | Distributed sessions | High | $15-100/month |
| **Azure Container Registry** | Store container images | Critical | $5-20/month |
| **Application Insights** | Monitoring and diagnostics | High | $0-50/month |
| **Azure Key Vault** | Secrets management | Medium | $1-5/month |

**Estimated Total:** $70-525/month (depending on scale and tier)

---

## Risks & Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| Complete rewrite required | High | Certain | Accept; no migration path exists |
| No existing automated tests | High | Certain | Create comprehensive test suite first |
| Learning curve for Azure Container Apps | Medium | High | Review documentation; start simple |
| WCF to REST API contract changes | Medium | Certain | Document APIs; version appropriately |
| Cost overruns on Azure services | Medium | Medium | Start with basic tiers; monitor costs |
| Data model changes during migration | Low | Medium | Design database schema carefully |

---

## Recommendations

### Critical Priority

1. ✅ **Create comprehensive test plan** before starting migration to ensure feature parity
2. ✅ **Choose database early** - Recommend Azure SQL Database for this use case
3. ✅ **Set up Azure Service Bus** before removing MSMQ to understand messaging patterns
4. ✅ **Implement API first**, then migrate web UI to consume it (decoupled approach)

### High Priority

5. ✅ **Use Infrastructure as Code** - Create Bicep or Terraform templates for Azure resources
6. ✅ **Implement health checks** from the start for Container Apps monitoring
7. ✅ **Set up Application Insights early** for debugging during development
8. ✅ **Consider API authentication** - Implement Azure AD B2C or JWT tokens

### Medium Priority

9. ✅ **Consider using Azure Developer CLI (azd)** templates for faster setup
10. ✅ **Implement unit tests** as you build new components
11. ✅ **Use managed identities** instead of connection strings for Azure services
12. ✅ **Set up staging environment** in Container Apps for testing

### Low Priority (Future Enhancements)

13. ⭐ **Modernize frontend** - Consider Blazor Server/WASM or React for richer UX
14. ⭐ **Add authentication** - Implement user login with Azure AD B2C
15. ⭐ **Implement caching** - Add response caching in API for better performance
16. ⭐ **Add search** - Implement Azure Cognitive Search for product search

---

## Effort Estimation

| Phase | Duration | Team Size |
|-------|----------|-----------|
| Foundation Setup | 1 week | 1-2 devs |
| Data Layer | 1 week | 1-2 devs |
| Service Layer (API) | 1 week | 1-2 devs |
| Messaging | 1 week | 1 dev |
| Web Application | 1 week | 1-2 devs |
| Containerization | 1 week | 1 dev |
| Azure Deployment | 1 week | 1-2 devs |
| CI/CD & Polish | 1 week | 1-2 devs |

**Total Estimated Effort:** 6-8 weeks with 2-3 developers

**Complexity:** High  
**Risk Level:** Medium-High  
**Confidence:** 75%

---

## Success Criteria

The modernization will be considered successful when:

- ✅ Application runs on .NET 10
- ✅ All components containerized and running in Azure Container Apps
- ✅ No Windows-specific dependencies (MSMQ, WCF)
- ✅ Data persisted in cloud database
- ✅ Sessions work across multiple instances
- ✅ Order processing uses Azure Service Bus
- ✅ CI/CD pipeline automates deployment
- ✅ Application Insights provides monitoring
- ✅ All original features work (feature parity)
- ✅ Health checks and readiness probes configured
- ✅ Application scales horizontally

---

## Next Steps

1. **Review this assessment** with stakeholders
2. **Get approval** for rewrite approach and timeline
3. **Provision Azure resources** (Service Bus, SQL Database, Container Registry)
4. **Set up development environment** with .NET 10 SDK
5. **Create new repository structure** or branch for rewrite
6. **Begin Phase 1** - Foundation Setup

---

## Appendix: Code Examples

### Example: WCF to REST API Migration

**Before (WCF):**
```csharp
[ServiceContract]
public interface IProductService
{
    [OperationContract]
    List<Product> GetAllProducts();
    
    [OperationContract]
    Product GetProductById(int productId);
}
```

**After (REST API):**
```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAllProducts()
    {
        // Implementation
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProductById(int id)
    {
        // Implementation
    }
}
```

### Example: MSMQ to Azure Service Bus Migration

**Before (MSMQ):**
```csharp
using (MessageQueue queue = new MessageQueue(_queuePath))
{
    queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Order) });
    Message message = new Message(order);
    queue.Send(message);
}
```

**After (Azure Service Bus):**
```csharp
await using var client = new ServiceBusClient(connectionString);
ServiceBusSender sender = client.CreateSender("orders");
var message = new ServiceBusMessage(JsonSerializer.Serialize(order));
await sender.SendMessageAsync(message);
```

---

## Document Version

- **Version:** 1.0
- **Date:** January 14, 2026
- **Status:** Initial Assessment
- **Author:** GitHub Copilot (Modernization Agent)

---

*End of Assessment*
