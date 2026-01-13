# ProductCatalogApp Modernization Assessment

**Assessment Date:** January 13, 2026  
**Repository:** bradygaster/ProductCatalogApp  
**Target:** .NET 10 + Azure Container Apps  
**Complexity Score:** 7/10

---

## Executive Summary

The ProductCatalogApp is a demonstration e-commerce application built on **.NET Framework 4.8.1** using legacy technologies including **ASP.NET MVC 5**, **WCF services**, and **MSMQ messaging**. While the application demonstrates good architectural separation of concerns, it is tightly coupled to Windows-specific technologies that are incompatible with modern containerized, cross-platform deployment scenarios.

### Key Findings

- **Current Stack:** .NET Framework 4.8.1, ASP.NET MVC 5, WCF, MSMQ
- **Target Stack:** .NET 10, ASP.NET Core, REST API/gRPC, Azure Service Bus
- **Migration Complexity:** 7/10 (Moderate-High)
- **Estimated Effort:** 80-120 hours (2-3 weeks)
- **Critical Blockers:** WCF services, MSMQ messaging, Windows-only dependencies

---

## Current State Analysis

### Application Architecture

The application consists of three distinct components:

#### 1. **ProductCatalog** (Web Frontend)
- **Type:** ASP.NET MVC 5 web application
- **Framework:** .NET Framework 4.8.1
- **Key Technologies:**
  - ASP.NET MVC 5.2.9 with Razor views
  - Bootstrap 5.2.3 for UI
  - jQuery 3.7.0
  - WCF client for service communication
  - MSMQ for order submission
  - Session state for shopping cart

**Purpose:** Provides the user interface for browsing products, managing shopping cart, and submitting orders.

**Legacy Patterns:**
- Uses `Global.asax` for application startup
- WCF `ProductServiceClient` for all product operations
- Session-based cart storage (won't work in distributed environments)
- Synchronous controller actions
- MSMQ dependency through `OrderQueueService`

#### 2. **ProductServiceLibrary** (WCF Service)
- **Type:** WCF Service Library
- **Framework:** .NET Framework 4.8.1
- **Key Technologies:**
  - WCF ServiceContract
  - SOAP-based communication
  - In-memory data storage

**Purpose:** Provides business logic and data access for product catalog operations via SOAP/WCF.

**Legacy Patterns:**
- WCF `[ServiceContract]` and `[OperationContract]` attributes
- SOAP-only communication (no REST)
- Static in-memory collections for data storage
- Lock-based concurrency (`lock (_lock)`)
- No dependency injection

**Data Model:**
- 22 products across 7 categories
- All data stored in static collections (non-persistent)
- Simple CRUD operations

#### 3. **OrderProcessor** (Background Service)
- **Type:** Console application
- **Framework:** .NET Framework 4.8.1
- **Key Technologies:**
  - MSMQ message queue consumer
  - Console-based execution

**Purpose:** Background worker that processes orders from the MSMQ queue.

**Legacy Patterns:**
- Polling-based MSMQ message consumption
- Thread-based background processing
- No structured hosting (not a Windows Service)
- Console-only output (no structured logging)

**Note:** This project is not included in the main solution file.

### Technology Stack Summary

| Component | Technology | Version | Status |
|-----------|-----------|---------|--------|
| Runtime | .NET Framework | 4.8.1 | Legacy |
| Web Framework | ASP.NET MVC | 5.2.9 | Legacy |
| Service Framework | WCF | Built-in | Legacy |
| Messaging | MSMQ | Built-in | Windows-only |
| UI Framework | Bootstrap | 5.2.3 | Modern |
| JavaScript | jQuery | 3.7.0 | Legacy (but functional) |
| Serialization | Newtonsoft.Json | 13.0.3 | Modern |

### Windows Dependencies

The application has critical dependencies on Windows-specific technologies:

1. **IIS (Internet Information Services)** - Web hosting
2. **MSMQ (Microsoft Message Queuing)** - Asynchronous messaging
3. **Windows Server** - Required runtime platform
4. **WCF hosting** - Requires Windows activation services

These dependencies prevent the application from running in Linux containers, which are the target for Azure Container Apps.

### Project File Format

All projects use the **legacy .csproj format** with:
- `packages.config` for NuGet dependencies
- Full framework assembly references
- Verbose project file syntax
- ToolsVersion="15.0"

Modern SDK-style projects offer significant advantages including:
- Simplified syntax
- Automatic package restore
- Multi-targeting support
- Better tooling integration

---

## Target State Design

### Modern Architecture

The modernized application should be redesigned as containerized microservices:

```
┌─────────────────────────────────────────────────────────┐
│                 Azure Container Apps                     │
├─────────────────┬──────────────────┬────────────────────┤
│  ProductCatalog │  ProductCatalog  │  OrderProcessor    │
│  .Web           │  .Api            │  .Worker           │
│                 │                  │                    │
│  ASP.NET Core   │  ASP.NET Core    │  Worker Service    │
│  MVC 10         │  Web API 10      │  .NET 10           │
│                 │                  │                    │
│  ├─HttpClient   │  ├─REST/gRPC     │  ├─Service Bus    │
│  ├─Redis Cache  │  ├─Database      │  │  Consumer       │
│  └─Service Bus  │  └─Dependency    │  └─Hosted         │
│     Client      │      Injection   │      Service       │
└─────────────────┴──────────────────┴────────────────────┘
         │                  │                  │
         │                  ▼                  │
         │         ┌──────────────────┐        │
         │         │  Azure SQL DB /  │        │
         │         │  Cosmos DB       │        │
         │         └──────────────────┘        │
         │                                     │
         └──────────────┬──────────────────────┘
                        ▼
              ┌──────────────────┐
              │ Azure Service    │
              │ Bus              │
              └──────────────────┘
```

### Proposed Components

#### 1. ProductCatalog.Web (Frontend)
**Technology:** ASP.NET Core 10 MVC

**Changes Required:**
- Migrate from ASP.NET MVC 5 to ASP.NET Core MVC
- Replace `Global.asax` with `Program.cs` and `Startup.cs` (or minimal hosting)
- Replace WCF client with `HttpClient` or typed HTTP clients
- Implement distributed session using Redis
- Add health checks
- Configure for containerization

**Modern Patterns:**
- Dependency injection throughout
- Configuration via `appsettings.json` and environment variables
- Async/await for all I/O operations
- Structured logging (ILogger)

#### 2. ProductCatalog.Api (Service Layer)
**Technology:** ASP.NET Core 10 Web API

**Changes Required:**
- Create new Web API project
- Convert WCF service contracts to REST API controllers
- Implement OpenAPI/Swagger documentation
- Add persistent database (replace in-memory storage)
- Entity Framework Core or Dapper for data access
- Add authentication/authorization if needed

**API Design:**
```
GET    /api/products              - Get all products
GET    /api/products/{id}         - Get product by ID
GET    /api/products/category/{category} - Get by category
GET    /api/products/search?q={term} - Search products
GET    /api/categories            - Get all categories
POST   /api/products              - Create product
PUT    /api/products/{id}         - Update product
DELETE /api/products/{id}         - Delete product
```

#### 3. OrderProcessor.Worker (Background Service)
**Technology:** .NET 10 Worker Service

**Changes Required:**
- Convert console app to Worker Service with `IHostedService`
- Replace MSMQ with Azure Service Bus client
- Implement structured logging
- Add graceful shutdown handling
- Add retry policies and error handling
- Configure for containerization

**Modern Patterns:**
- `BackgroundService` base class
- Dependency injection
- Health checks
- Graceful shutdown via `CancellationToken`

### Azure Services Required

| Service | Purpose | Tier Recommendation |
|---------|---------|-------------------|
| **Azure Container Apps** | Host all three services | Consumption |
| **Azure Service Bus** | Replace MSMQ messaging | Standard |
| **Azure Container Registry** | Store Docker images | Basic |
| **Azure SQL Database** | Product catalog storage | Basic/Standard |
| **Azure Cache for Redis** | Distributed session/cache | Basic |
| **Azure Application Insights** | Monitoring and logging | Pay-as-you-go |
| **Azure Key Vault** | Secrets management | Standard |

**Estimated Monthly Cost:** $150-300 for development/testing environment

---

## Migration Complexity Analysis

### Overall Score: 7/10

**Scale:** 1 (trivial) to 10 (extremely complex)

### Complexity Breakdown

#### 1. Framework Migration (Score: 6/10)

**Challenges:**
- .NET Framework → .NET 10 is a major version jump
- ASP.NET MVC → ASP.NET Core MVC requires view updates
- Project file format modernization
- Package compatibility checks

**Mitigating Factors:**
- Small codebase (~17 C# files, 8 views)
- No complex Windows-specific APIs (beyond WCF/MSMQ)
- Modern NuGet packages already in use (Bootstrap 5, jQuery 3.7)

#### 2. Architecture Changes (Score: 8/10)

**Challenges:**
- **WCF to REST API** - High impact, requires complete rewrite of service layer
- **MSMQ to Azure Service Bus** - Different API, different semantics
- **In-memory to persistent storage** - Requires database design and implementation
- **Session state to distributed cache** - Requires Redis integration

**Critical Path Items:**
- WCF has no direct migration path in .NET Core
- MSMQ is Windows-only, no Linux equivalent
- Service Bus has different message handling patterns

#### 3. Platform Migration (Score: 7/10)

**Challenges:**
- Windows-only → Cross-platform (Linux containers)
- IIS → Kestrel web server
- Containerization (Dockerfiles, container orchestration)
- Azure infrastructure setup

**Mitigating Factors:**
- Azure Container Apps simplifies container orchestration
- Extensive documentation available
- Standard containerization patterns

#### 4. Code Modernization (Score: 5/10)

**Challenges:**
- Update to async/await patterns
- Implement dependency injection
- Add structured logging
- Update to modern C# features

**Mitigating Factors:**
- Business logic is straightforward
- No complex algorithms or state machines
- Clear separation of concerns already exists

### Risk Assessment

| Risk | Impact | Likelihood | Mitigation |
|------|--------|-----------|-----------|
| WCF migration complexity | High | Medium | Incremental migration, support both protocols during transition |
| MSMQ behavioral differences | High | Medium | Comprehensive testing, implement proper error handling |
| Data loss in containers | High | High | Implement persistent storage immediately |
| Learning curve (Container Apps) | Medium | Low | Use Microsoft documentation and samples |
| No existing tests | Medium | High | Create integration tests during migration |

---

## Migration Strategy

### Recommended Approach: Incremental Modernization

Rather than a "big bang" rewrite, we recommend a phased approach that allows for testing and validation at each stage.

### Phase 1: Foundation & Project Modernization (16-24 hours)

**Objectives:**
- Upgrade to .NET 10
- Modernize project files
- Update dependencies

**Tasks:**
1. Convert projects to SDK-style format
2. Update NuGet package references
3. Replace `packages.config` with PackageReference
4. Update to .NET 10 target framework
5. Address compilation errors
6. Modernize C# language usage (nullable reference types, pattern matching, etc.)

**Deliverables:**
- All projects compile on .NET 10
- Tests (if any) pass
- Code runs locally

### Phase 2: Service Layer Migration (20-30 hours)

**Objectives:**
- Replace WCF with REST API
- Implement persistent storage

**Tasks:**
1. Create new ASP.NET Core 10 Web API project
2. Design REST API endpoints (maintain functional parity)
3. Implement controllers with same operations as WCF
4. Choose and implement database (Azure SQL or Cosmos DB)
5. Implement Entity Framework Core or Dapper
6. Migrate data models
7. Add Swagger/OpenAPI documentation
8. Implement health checks
9. Add logging and error handling

**Deliverables:**
- Fully functional REST API
- Database with product catalog data
- API documentation
- API can be tested independently

### Phase 3: Web Application Migration (20-30 hours)

**Objectives:**
- Migrate ASP.NET MVC to ASP.NET Core MVC
- Replace WCF client with HTTP client

**Tasks:**
1. Create new ASP.NET Core 10 MVC project
2. Migrate views (most should work with minimal changes)
3. Update controllers to ASP.NET Core syntax
4. Replace WCF client with `HttpClient` or typed clients
5. Implement distributed session with Redis
6. Update dependency injection
7. Migrate static files (CSS, JavaScript, images)
8. Update routing configuration
9. Test all user flows

**Deliverables:**
- Fully functional web application
- All pages render correctly
- Cart functionality works with distributed session
- Integration with API verified

### Phase 4: Messaging & Background Processing (16-24 hours)

**Objectives:**
- Replace MSMQ with Azure Service Bus
- Modernize order processor

**Tasks:**
1. Create Azure Service Bus namespace and queue
2. Update web app to send messages to Service Bus
3. Create new .NET 10 Worker Service project
4. Implement `BackgroundService` for message processing
5. Add Service Bus message receiver
6. Implement retry policies and dead-letter handling
7. Add logging and monitoring
8. Test end-to-end order flow

**Deliverables:**
- Working order submission from web to Service Bus
- Worker service processing messages
- Error handling and retry logic
- Logging operational

### Phase 5: Containerization & Deployment (12-16 hours)

**Objectives:**
- Containerize all applications
- Deploy to Azure Container Apps

**Tasks:**
1. Create Dockerfile for Web application
2. Create Dockerfile for API service
3. Create Dockerfile for Worker service
4. Test containers locally (Docker Compose)
5. Setup Azure Container Registry
6. Push images to ACR
7. Create Azure Container Apps environment
8. Deploy all three apps
9. Configure environment variables and secrets
10. Setup Application Insights
11. Configure scaling rules
12. Create CI/CD pipeline (GitHub Actions)
13. Documentation for deployment

**Deliverables:**
- All services containerized
- Running on Azure Container Apps
- Automated deployment pipeline
- Monitoring and logging configured

---

## Effort Estimation

### Total Effort: 80-120 hours

**Team Composition:** 1-2 developers with:
- Experience in .NET Core/ASP.NET Core
- Understanding of REST API design
- Familiarity with Azure services
- Docker/container knowledge

### Timeline: 2-3 weeks

Assumes:
- Dedicated focus on migration
- Access to Azure subscription
- Decisions made promptly
- No major scope changes

### Breakdown by Phase

| Phase | Hours | % of Total |
|-------|-------|-----------|
| Phase 1: Foundation | 16-24 | 20% |
| Phase 2: Service Layer | 20-30 | 25% |
| Phase 3: Web App | 20-30 | 25% |
| Phase 4: Messaging | 16-24 | 20% |
| Phase 5: Containerization | 12-16 | 10% |
| **Total** | **84-124** | **100%** |

---

## Critical Recommendations

### Priority: CRITICAL

1. **Implement Persistent Storage Immediately**
   - Current in-memory storage will lose all data on container restart
   - Implement Azure SQL Database, Cosmos DB, or PostgreSQL
   - Use Entity Framework Core for data access

2. **Replace MSMQ with Azure Service Bus**
   - MSMQ does not exist in Linux containers
   - Azure Service Bus provides enterprise-grade messaging
   - Plan for different message semantics (at-least-once vs exactly-once)

3. **Replace WCF with REST API**
   - WCF is not supported in .NET Core/.NET 5+
   - REST APIs are industry standard
   - Consider gRPC for high-performance scenarios

### Priority: HIGH

4. **Implement Distributed Session Management**
   - ASP.NET session state doesn't work across multiple instances
   - Use Azure Cache for Redis
   - Serialize cart data appropriately

5. **Add Automated Tests**
   - No existing tests make migration risky
   - Create integration tests for critical paths
   - Add unit tests for business logic

6. **Implement Structured Logging**
   - Use `ILogger<T>` throughout
   - Configure Application Insights
   - Add correlation IDs for tracing requests

### Priority: MEDIUM

7. **Add Health Checks**
   - Container orchestration relies on health checks
   - Implement `/health` and `/health/ready` endpoints
   - Check database connectivity, Service Bus, Redis

8. **Implement Configuration Management**
   - Use Azure Key Vault for secrets
   - Externalize configuration
   - Use managed identities where possible

9. **Plan for API Versioning**
   - Design API with versioning from the start
   - Use URL versioning (`/api/v1/products`) or header versioning

### Priority: LOW

10. **Consider Frontend Modernization**
    - Current jQuery/Bootstrap setup is functional
    - Could be modernized to React, Vue, or Blazor in future
    - Not required for initial migration

---

## Cost Considerations

### Development Costs
- **Developer Time:** 80-120 hours × developer rate
- **Azure Subscription:** Existing subscription assumed

### Ongoing Operational Costs (Monthly)

**Development/Testing Environment:**
- Azure Container Apps: ~$20-40 (minimal load)
- Azure Service Bus: ~$10 (Standard tier)
- Azure SQL Database: ~$5-15 (Basic tier)
- Azure Cache for Redis: ~$15 (Basic tier)
- Azure Container Registry: ~$5 (Basic)
- Application Insights: ~$5-20 (pay-per-use)
- **Total: ~$60-105/month**

**Production Environment:**
- Azure Container Apps: ~$50-150 (with autoscaling)
- Azure Service Bus: ~$10-50 (Standard tier)
- Azure SQL Database: ~$50-150 (Standard tier)
- Azure Cache for Redis: ~$25-75 (Standard tier)
- Azure Container Registry: ~$5 (Basic)
- Application Insights: ~$20-50 (pay-per-use)
- **Total: ~$160-480/month**

**Note:** Costs scale with usage. Container Apps uses consumption-based pricing, so low-traffic applications cost very little.

---

## Success Criteria

The migration will be considered successful when:

- [ ] All applications migrated to .NET 10
- [ ] Applications containerized and running on Azure Container Apps
- [ ] MSMQ replaced with Azure Service Bus
- [ ] WCF replaced with REST API
- [ ] Persistent storage implemented
- [ ] Distributed session management operational
- [ ] All existing functionality preserved and tested
- [ ] Automated deployment pipeline established
- [ ] Monitoring and logging operational
- [ ] Health checks implemented and working
- [ ] Documentation updated
- [ ] Team trained on new architecture

---

## Alternative Approaches Considered

### 1. "Lift and Shift" to Windows Containers
**Verdict:** Not recommended
- Windows containers are larger and more expensive
- Still tied to Windows licensing
- Doesn't achieve goal of modernization
- Misses performance benefits of .NET 10

### 2. Complete Rewrite in Different Technology
**Verdict:** Not recommended
- Unnecessarily expensive
- Business logic works fine
- .NET 10 provides excellent foundation

### 3. Keep on .NET Framework, just containerize
**Verdict:** Not possible
- MSMQ doesn't work in containers (even Windows containers)
- IIS dependencies complicate containerization
- Misses modernization goals

### 4. Recommended: Incremental Modernization
**Verdict:** Recommended ✓
- Balances risk and reward
- Allows for testing at each phase
- Achieves all modernization goals
- Most cost-effective approach

---

## Next Steps

1. **Review and approve this assessment**
2. **Obtain Azure subscription access** (if not already available)
3. **Setup development environment** (Azure DevOps/GitHub, local Docker)
4. **Create project backlog** from the migration phases
5. **Begin Phase 1** (Foundation & Project Modernization)
6. **Schedule regular check-ins** to track progress

---

## Appendix: Technical Details

### Current Project Structure
```
ProductCatalogApp/
├── ProductCatalog/                 # ASP.NET MVC 5 web app
│   ├── Controllers/
│   ├── Views/
│   ├── Models/
│   ├── Services/
│   └── Content/
├── ProductServiceLibrary/          # WCF service
│   ├── IProductService.cs
│   ├── ProductService.cs
│   ├── ProductRepository.cs
│   └── Models/
├── OrderProcessor/                 # Console app (not in solution)
│   └── Program.cs
└── ProductCatalogApp.slnx          # Solution file
```

### Proposed Project Structure
```
ProductCatalogApp/
├── src/
│   ├── ProductCatalog.Web/         # ASP.NET Core MVC
│   │   ├── Controllers/
│   │   ├── Views/
│   │   ├── wwwroot/
│   │   ├── Program.cs
│   │   └── Dockerfile
│   ├── ProductCatalog.Api/         # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Data/
│   │   ├── Program.cs
│   │   └── Dockerfile
│   ├── OrderProcessor.Worker/      # .NET Worker Service
│   │   ├── Worker.cs
│   │   ├── Program.cs
│   │   └── Dockerfile
│   └── ProductCatalog.Shared/      # Shared models/contracts
│       └── Models/
├── tests/
│   ├── ProductCatalog.Api.Tests/
│   ├── ProductCatalog.Web.Tests/
│   └── OrderProcessor.Worker.Tests/
├── infrastructure/
│   ├── bicep/                      # Azure infrastructure as code
│   └── docker-compose.yml          # Local development
└── .github/
    └── workflows/                  # CI/CD pipelines
```

### Key NuGet Packages Required

**For Web and API:**
- Microsoft.AspNetCore.App (included in .NET 10)
- Microsoft.EntityFrameworkCore (for database)
- Azure.Identity
- Azure.Messaging.ServiceBus
- StackExchange.Redis (for session)
- Swashbuckle.AspNetCore (for API docs)

**For Worker:**
- Microsoft.Extensions.Hosting
- Azure.Messaging.ServiceBus
- Azure.Identity

---

## Conclusion

The ProductCatalogApp is a well-structured demonstration application that, while using legacy technologies, has a clean architecture that will facilitate modernization. The primary challenges are replacing Windows-specific technologies (WCF, MSMQ) with cloud-native alternatives (REST API, Azure Service Bus) and containerizing for Azure Container Apps deployment.

With a complexity score of **7/10** and an estimated effort of **80-120 hours**, this migration is moderately complex but achievable within 2-3 weeks by a skilled development team. The phased approach minimizes risk while ensuring each component is thoroughly tested before proceeding.

The modernized application will benefit from:
- **Cross-platform support** (runs on Linux containers)
- **Improved performance** (native .NET 10 performance)
- **Better scalability** (Azure Container Apps autoscaling)
- **Modern development practices** (dependency injection, async/await, structured logging)
- **Cloud-native integration** (Azure services, managed identities, Application Insights)

**Recommendation:** Proceed with the incremental modernization approach as outlined in this assessment.

---

*Assessment completed by: Copilot for GitHub*  
*Date: January 13, 2026*
