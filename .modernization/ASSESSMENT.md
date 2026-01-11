# Modernization Assessment: ProductCatalogApp

**Assessment Date:** January 11, 2026  
**Target Framework:** .NET 10  
**Target Platform:** Azure Container Apps  
**Overall Complexity:** 7/10  
**Estimated Effort:** 15-25 days

---

## Executive Summary

The ProductCatalogApp is a traditional .NET Framework 4.8.1 application consisting of three main components:

1. **ProductCatalog** - ASP.NET MVC 5 web application
2. **ProductServiceLibrary** - WCF service library for product operations
3. **OrderProcessor** - Console application for background order processing

The application demonstrates a classic 3-tier architecture with significant reliance on legacy Windows-specific technologies (MSMQ, WCF, IIS). Migration to .NET 10 and Azure Container Apps is **highly feasible** but requires substantial modernization effort, particularly around service communication (WCF → REST/gRPC) and messaging infrastructure (MSMQ → Azure Service Bus).

### Key Findings

✅ **Strengths:**
- Well-structured codebase with clear separation of concerns
- Simple, well-defined service interface
- Modern frontend frameworks (Bootstrap 5, jQuery 3.7)
- Straightforward business logic

⚠️ **Challenges:**
- Heavy reliance on WCF services (not supported in .NET Core/.NET 10)
- MSMQ dependency (Windows-specific, unavailable in containers)
- Session-based state management (not cloud-native)
- No existing test coverage
- In-memory data storage (not production-ready)

---

## Current Architecture

### Component Analysis

#### 1. ProductCatalog Web Application

**Technology Stack:**
- ASP.NET MVC 5.2.9
- .NET Framework 4.8.1
- Bootstrap 5.2.3
- jQuery 3.7.0
- Newtonsoft.Json 13.0.3

**Key Features:**
- Product catalog browsing
- Shopping cart functionality (session-based)
- Order submission via MSMQ
- WCF service client for product data

**Legacy Patterns:**
- `System.Web.Mvc` - Requires migration to ASP.NET Core MVC
- `System.Messaging` - MSMQ integration for order queuing
- Session state management - Not suitable for horizontal scaling
- WCF service reference - Needs replacement with HTTP client
- IIS-specific Web.config configuration

**Complexity Rating:** 7/10

**Files:** ~15 C# files, multiple views (cshtml)

#### 2. ProductServiceLibrary (WCF Service)

**Technology Stack:**
- WCF Service Library
- .NET Framework 4.8.1
- System.ServiceModel

**Service Operations:**
- `GetAllProducts()` - Retrieve all products
- `GetProductById(int)` - Get single product
- `GetProductsByCategory(string)` - Filter by category
- `SearchProducts(string)` - Search functionality
- `GetCategories()` - List categories
- `CreateProduct(Product)` - Add new product
- `UpdateProduct(Product)` - Update existing product
- `DeleteProduct(int)` - Remove product
- `GetProductsByPriceRange(decimal, decimal)` - Price filtering

**Data Storage:**
- In-memory `ProductRepository` with hardcoded sample data
- No persistent storage

**Legacy Patterns:**
- WCF ServiceContract/OperationContract attributes
- SOAP-based communication
- BasicHttpBinding configuration

**Complexity Rating:** 6/10

**Files:** 6 C# files

#### 3. OrderProcessor Console Application

**Technology Stack:**
- Console Application (.NET Framework 4.8.1 inferred)
- System.Messaging (MSMQ)

**Functionality:**
- Continuously polls MSMQ queue for orders
- Processes orders with simulated steps:
  - Payment validation
  - Inventory updates
  - Shipping label creation
  - Confirmation email sending
- Console-based output with detailed order information

**Legacy Patterns:**
- MSMQ message queue dependency
- Console application architecture (not containerizable as-is)
- Threading with manual polling

**Complexity Rating:** 5/10

**Files:** 3 files (Program.cs, App.config, README.md)

---

## Migration Strategy

### Recommended Approach: Incremental Modernization

The migration should follow a phased approach to minimize risk and allow for iterative testing:

### Phase 1: Foundation (5-8 days)

**Objective:** Establish modern project structure and convert service layer

**Tasks:**
1. Convert all projects to SDK-style format (`.csproj` modernization)
2. Create new .NET 10 projects:
   - ASP.NET Core Web API (replacing WCF)
   - ASP.NET Core MVC Web App
   - Worker Service (replacing console app)
3. Migrate product service logic to REST API
4. Update in-memory repository for compatibility

**Deliverables:**
- SDK-style project files
- REST API with Swagger documentation
- Initial Docker images

**Critical Path:** WCF → REST API migration (blocking for other phases)

### Phase 2: Web Application Migration (6-10 days)

**Objective:** Modernize the web frontend

**Tasks:**
1. Migrate ASP.NET MVC 5 to ASP.NET Core MVC
2. Update views to ASP.NET Core Razor syntax
3. Replace WCF client with `HttpClient` + typed client
4. Implement distributed session state with Redis
5. Migrate Web.config to appsettings.json
6. Add health check endpoints
7. Update dependency injection configuration
8. Migrate static files and bundling

**Deliverables:**
- ASP.NET Core MVC web application
- Redis session configuration
- Dockerfile for web app
- Health check endpoints

**Key Changes:**
```csharp
// Before (WCF Client)
using (var client = new ProductServiceClient())
{
    products = client.GetAllProducts().ToList();
}

// After (HTTP Client)
var products = await _productApiClient.GetAllProductsAsync();
```

### Phase 3: Messaging Infrastructure (4-6 days)

**Objective:** Replace MSMQ with Azure Service Bus

**Tasks:**
1. Set up Azure Service Bus namespace and queue
2. Create OrderQueueService adapter using `Azure.Messaging.ServiceBus`
3. Update OrderProcessor to Worker Service
4. Implement retry policies and dead-letter queue handling
5. Add structured logging
6. Configure managed identity authentication

**Deliverables:**
- Azure Service Bus integration
- Worker Service project
- Dockerfile for worker
- Error handling and retry logic

**Key Changes:**
```csharp
// Before (MSMQ)
using (MessageQueue queue = new MessageQueue(_queuePath))
{
    queue.Send(order);
}

// After (Azure Service Bus)
await _serviceBusSender.SendMessageAsync(
    new ServiceBusMessage(JsonSerializer.Serialize(order))
);
```

### Phase 4: Azure Container Apps Deployment (3-5 days)

**Objective:** Deploy to Azure Container Apps

**Tasks:**
1. Create Azure Container Registry
2. Create container images and push to ACR
3. Set up Azure Container Apps environment
4. Deploy web app container
5. Deploy API container
6. Deploy worker service container
7. Configure service discovery and networking
8. Set up Application Insights
9. Configure secrets and managed identities
10. Create deployment pipeline (GitHub Actions)

**Deliverables:**
- Infrastructure as Code (Bicep/ARM templates)
- Running application in Azure Container Apps
- CI/CD pipeline
- Monitoring and alerting setup

---

## Technology Mapping

### Web Framework

| Current | Target | Effort |
|---------|--------|--------|
| ASP.NET MVC 5 | ASP.NET Core MVC | High |
| System.Web.Mvc | Microsoft.AspNetCore.Mvc | High |
| Web.config | appsettings.json | Low |
| Global.asax | Program.cs + Startup | Low |
| BundleConfig | Built-in bundling or Vite | Low |

### Service Layer

| Current | Target | Effort |
|---------|--------|--------|
| WCF (SOAP) | REST API | High |
| ServiceContract | API Controllers | Medium |
| BasicHttpBinding | HTTP/HTTPS | Low |
| WCF Client | HttpClient | Medium |

### Messaging

| Current | Target | Effort |
|---------|--------|--------|
| MSMQ | Azure Service Bus | Medium |
| System.Messaging | Azure.Messaging.ServiceBus | Medium |
| Local queue | Cloud-based queue | Low |
| XmlMessageFormatter | JSON | Low |

### Background Processing

| Current | Target | Effort |
|---------|--------|--------|
| Console App | Worker Service | Medium |
| Manual polling | Background Service | Low |
| Thread.Sleep | Timer/Hosted Service | Low |

### State Management

| Current | Target | Effort |
|---------|--------|--------|
| Session (in-memory) | Redis Distributed Cache | Medium |
| Session["Cart"] | IDistributedCache | Low |

### Configuration

| Current | Target | Effort |
|---------|--------|--------|
| Web.config | appsettings.json | Low |
| App.config | appsettings.json | Low |
| ConfigurationManager | IConfiguration | Low |

---

## Azure Container Apps Readiness

### Blockers (Must Fix)

#### 1. MSMQ Dependency ⛔
**Issue:** MSMQ is Windows-specific and not available in Linux containers.

**Impact:** Critical - breaks order processing functionality

**Solution:** Replace with Azure Service Bus
- Set up Service Bus namespace
- Create queue: `productcatalog-orders`
- Use `Azure.Messaging.ServiceBus` NuGet package
- Update both sender (web app) and receiver (worker service)

**Code Impact:** Medium - well-encapsulated in `OrderQueueService`

#### 2. WCF Services ⛔
**Issue:** WCF is not supported in .NET Core/.NET 5+

**Impact:** Critical - entire service layer

**Solution:** Rewrite as ASP.NET Core Web API
- Create REST endpoints matching WCF operations
- Use JSON for serialization
- Implement OpenAPI/Swagger documentation
- Update client to use HttpClient

**Code Impact:** High - complete service rewrite

#### 3. System.Messaging ⛔
**Issue:** Not available in .NET Core

**Impact:** Critical - breaks order submission

**Solution:** Part of MSMQ → Service Bus migration

### Considerations (Recommended)

#### 1. Session State Management ⚠️
**Concern:** In-memory session state doesn't support horizontal scaling

**Impact:** High - cart data lost during scaling events

**Solution:** Implement Redis distributed cache
```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration["Redis:ConnectionString"];
});
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
```

#### 2. Health Checks ⚠️
**Concern:** No health endpoints for Container Apps probes

**Impact:** Medium - affects reliability and auto-scaling

**Solution:** Add health check endpoints
```csharp
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddRedis(redisConnectionString)
    .AddAzureServiceBusQueue(serviceBusConnectionString, queueName);

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
```

#### 3. Logging and Observability ⚠️
**Concern:** Console logging not suitable for containers

**Impact:** Medium - difficult troubleshooting

**Solution:** Structured logging with Application Insights
```csharp
builder.Services.AddApplicationInsightsTelemetry();
builder.Logging.AddConsole();
builder.Logging.AddApplicationInsights();
```

#### 4. Service Discovery ⚠️
**Concern:** Hardcoded WCF endpoints

**Impact:** Low - configuration issue

**Solution:** Use environment variables and Container Apps service discovery
```csharp
var apiBaseUrl = configuration["ProductApi:BaseUrl"];
// Injected via environment: https://product-api
```

#### 5. Persistent Storage ⚠️
**Concern:** In-memory data storage

**Impact:** High - data loss on restart

**Solution:** Add proper database
- Azure SQL Database
- Azure Cosmos DB
- Entity Framework Core

---

## Dependency Analysis

### Dependencies to Replace

#### High Priority

1. **System.Web.Mvc → Microsoft.AspNetCore.Mvc**
   - Effort: High
   - Breaking: Yes
   - Alternative: N/A - must migrate

2. **System.ServiceModel → ASP.NET Core Web API**
   - Effort: High
   - Breaking: Yes
   - Alternative: N/A - must migrate

3. **System.Messaging → Azure.Messaging.ServiceBus**
   - Effort: Medium
   - Breaking: Yes
   - Alternative: Azure Storage Queues (simpler but less features)

4. **System.Web → Microsoft.AspNetCore.Http**
   - Effort: Medium
   - Breaking: Yes
   - Alternative: N/A - must migrate

### Dependencies to Update

1. **Newtonsoft.Json (13.0.3)**
   - Current: Widely used, well-supported
   - Recommendation: Keep or migrate to System.Text.Json
   - Effort: Low (if keeping) / Medium (if migrating)

2. **Bootstrap (5.2.3)**
   - Current: Recent version
   - Recommendation: Update to 5.3.x
   - Effort: Minimal

3. **jQuery (3.7.0)**
   - Current: Latest version
   - Recommendation: Keep or evaluate if still needed
   - Effort: Low / Medium (if removing)

### New Dependencies to Add

1. **Azure.Messaging.ServiceBus** - For message queue
2. **Microsoft.Extensions.Caching.StackExchangeRedis** - For distributed sessions
3. **Azure.Identity** - For managed identity authentication
4. **Microsoft.ApplicationInsights.AspNetCore** - For monitoring
5. **Microsoft.Extensions.Hosting** - For Worker Service
6. **Microsoft.EntityFrameworkCore** - If adding database
7. **Swashbuckle.AspNetCore** - For API documentation

---

## Risks and Mitigation

### Technical Risks

| Risk | Severity | Probability | Impact | Mitigation |
|------|----------|-------------|--------|------------|
| WCF Migration Complexity | High | Low | High | Service interface is simple; straightforward REST mapping |
| MSMQ to Service Bus Issues | Medium | Low | High | Similar programming models; comprehensive Azure documentation |
| Session State Migration | Medium | Medium | Medium | Redis is mature; extensive ASP.NET Core support |
| Performance Degradation | Low | Low | Medium | .NET 10 typically faster; benchmark critical paths |
| Breaking Changes in Migration | Medium | Medium | High | Comprehensive integration testing |
| Learning Curve | Low | Medium | Low | ASP.NET Core MVC similar to MVC 5 |
| Cost Overruns | Medium | Medium | Medium | Start with basic tier; monitor and optimize |

### Operational Risks

| Risk | Severity | Mitigation |
|------|----------|------------|
| Insufficient Testing | High | Create comprehensive integration test suite |
| Production Issues | Medium | Implement staged rollout with blue-green deployment |
| Monitoring Gaps | Medium | Set up Application Insights early in migration |
| Documentation Debt | Low | Document as you migrate; update architecture diagrams |

---

## Recommendations

### Critical (Do First)

1. **✅ Migrate WCF Service to REST API**
   - Rationale: Foundation for entire application; other components depend on it
   - Duration: 5-8 days
   - Blockers: None

2. **✅ Replace MSMQ with Azure Service Bus**
   - Rationale: Critical blocker for containerization
   - Duration: 4-6 days
   - Blockers: None

3. **✅ Convert to SDK-Style Projects**
   - Rationale: Required for .NET 10; enables modern tooling
   - Duration: 1-2 days
   - Blockers: None

### High Priority

4. **📋 Implement Distributed Session State (Redis)**
   - Rationale: Required for horizontal scaling
   - Duration: 2-3 days
   - Blockers: None

5. **📋 Add Health Check Endpoints**
   - Rationale: Essential for Container Apps reliability
   - Duration: 1 day
   - Blockers: None

6. **📋 Implement Structured Logging**
   - Rationale: Critical for production troubleshooting
   - Duration: 2-3 days
   - Blockers: None

7. **📋 Create Infrastructure as Code**
   - Rationale: Repeatable deployments; environment parity
   - Duration: 3-4 days
   - Blockers: Application migration

### Medium Priority

8. **📋 Add Integration Tests**
   - Rationale: No existing test coverage; validate migration
   - Duration: 4-6 days
   - Blockers: None (can be done in parallel)

9. **📋 Implement Proper Data Persistence**
   - Rationale: In-memory storage not production-ready
   - Duration: 5-7 days
   - Blockers: None (can be done after initial migration)

10. **📋 Add Authentication/Authorization**
    - Rationale: Security requirement for production
    - Duration: 4-6 days
    - Blockers: Web app migration

### Low Priority

11. **📋 Evaluate jQuery Necessity**
    - Rationale: Modern browsers may not require jQuery
    - Duration: 2-3 days
    - Blockers: Web app migration

12. **📋 Implement API Versioning**
    - Rationale: Important for API evolution
    - Duration: 1-2 days
    - Blockers: API creation

---

## Technical Debt

### Existing Technical Debt

1. **In-Memory Product Repository**
   - Impact: Data loss on restart, not production-ready
   - Recommendation: Implement Azure SQL Database or Cosmos DB
   - Effort: Medium (5-7 days)
   - Priority: High (but can be done post-migration)

2. **No Authentication/Authorization**
   - Impact: Security vulnerability
   - Recommendation: Implement Azure AD B2C or ASP.NET Core Identity
   - Effort: Medium (4-6 days)
   - Priority: High (for production)

3. **No Automated Tests**
   - Impact: Risk of regression, difficult to validate migration
   - Recommendation: Add unit tests and integration tests
   - Effort: Medium (4-6 days)
   - Priority: High

4. **No Structured Logging**
   - Impact: Difficult to diagnose production issues
   - Recommendation: Implement Serilog or ILogger with Application Insights
   - Effort: Low (2-3 days)
   - Priority: High

5. **Hardcoded Configuration**
   - Impact: Difficult to manage across environments
   - Recommendation: Use environment variables and Azure Key Vault
   - Effort: Low (1-2 days)
   - Priority: Medium

### Migration Will Address

- ✅ Legacy project format
- ✅ WCF service architecture
- ✅ MSMQ dependency
- ✅ IIS-specific configuration
- ✅ Session state architecture

### Migration Won't Address (Separately Needed)

- ⚠️ Data persistence
- ⚠️ Authentication/Authorization
- ⚠️ Test coverage
- ⚠️ API versioning
- ⚠️ Input validation
- ⚠️ Error handling consistency

---

## Estimated Effort

### Development Time

| Phase | Tasks | Duration | Resources |
|-------|-------|----------|-----------|
| Phase 1: Foundation | Project conversion, WCF → REST API | 5-8 days | 1 developer |
| Phase 2: Web Migration | ASP.NET Core MVC migration | 6-10 days | 1 developer |
| Phase 3: Messaging | MSMQ → Service Bus, Worker Service | 4-6 days | 1 developer |
| Phase 4: Deployment | Azure Container Apps setup | 3-5 days | 1 developer + 1 DevOps |
| **Total** | | **18-29 days** | **1-2 people** |

**Realistic Estimate:** 15-25 days (with some parallel work and experienced team)

### Cost Estimates (Azure)

#### Monthly Operational Costs

| Service | Configuration | Est. Monthly Cost |
|---------|--------------|-------------------|
| Azure Container Apps | 3 containers (0.5 vCPU, 1GB RAM each) | $30-100 |
| Azure Service Bus | Basic tier + message volume | $10-50 |
| Azure Cache for Redis | Basic tier (C1 - 1GB) | $15-75 |
| Azure Container Registry | Basic tier | $5-20 |
| Application Insights | Based on ingestion volume | $0-50 |
| **Total** | | **$60-295/month** |

**Notes:**
- Costs vary significantly based on usage patterns
- Container Apps has generous free tier allowances
- Consider reserved instances for production (cost savings)
- Monitor and optimize based on actual usage

---

## Success Criteria

### Migration Complete When:

✅ All projects successfully build on .NET 10  
✅ Web application runs in Azure Container Apps  
✅ API responds to all endpoints correctly  
✅ Worker service processes messages from Azure Service Bus  
✅ Session state persists across container restarts (Redis)  
✅ Health checks return successful responses  
✅ Logs flow to Application Insights  
✅ Integration tests pass (to be created)  
✅ No dependency on Windows-specific technologies  
✅ Application performs at least as well as current version  

### Definition of Done:

- ✅ Code compiles without errors
- ✅ All manual testing scenarios pass
- ✅ Health checks configured and working
- ✅ Monitoring and alerting configured
- ✅ Documentation updated
- ✅ CI/CD pipeline functional
- ✅ Security scan completed (no critical issues)
- ✅ Performance baseline established
- ✅ Runbook created for operations team

---

## Next Steps

### Immediate Actions (This Week)

1. **Create Migration Plan Issues**
   - Break down each phase into granular tasks
   - Assign priorities and dependencies
   - Estimate effort for each task

2. **Set Up Development Environment**
   - Install .NET 10 SDK
   - Set up Azure subscription (if not already available)
   - Create development Azure Container Apps environment

3. **Create Git Branch Strategy**
   - Main migration branch: `feature/dotnet10-migration`
   - Sub-branches for each phase
   - Define merge strategy

4. **Baseline Current Application**
   - Document current functionality
   - Create test scenarios
   - Measure performance metrics

### Phase 1 Kickoff (Next Week)

1. Start WCF → REST API migration
2. Convert projects to SDK-style
3. Create initial Dockerfiles
4. Set up Azure Container Registry

---

## Conclusion

The ProductCatalogApp is an excellent candidate for modernization to .NET 10 and Azure Container Apps. While the migration requires significant effort (15-25 days), the application's simple architecture and well-defined interfaces make it straightforward.

**Key Success Factors:**
- Phased approach minimizes risk
- Clear migration path for each legacy technology
- Modern cloud-native benefits (scaling, reliability, cost efficiency)
- Improved developer experience with modern .NET

**Recommended Timeline:**
- **Weeks 1-2:** Foundation (REST API, project conversion)
- **Weeks 2-4:** Web application migration
- **Week 4:** Messaging infrastructure
- **Week 5:** Azure deployment and testing

The investment in modernization will result in:
- ✨ Better scalability and performance
- ✨ Lower operational complexity
- ✨ Improved developer productivity
- ✨ Modern cloud-native architecture
- ✨ Reduced licensing costs (no Windows Server)
- ✨ Better observability and monitoring

**Overall Assessment: PROCEED with migration** ✅

---

*Assessment completed by GitHub Copilot on January 11, 2026*
