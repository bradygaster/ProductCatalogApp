# ProductCatalogApp Modernization Assessment

**Assessment Date:** January 13, 2026  
**Repository:** bradygaster/ProductCatalogApp  
**Target:** .NET 10 + Azure Container Apps

---

## Executive Summary

The ProductCatalogApp is a legacy .NET Framework 4.8.1 application built with ASP.NET MVC 5, WCF services, and MSMQ messaging. To modernize this application for .NET 10 and deploy it to Azure Container Apps, a comprehensive migration is required involving framework upgrades, architectural changes, and infrastructure modernization.

**Complexity Score:** 8/10 (High)  
**Estimated Effort:** 60-80 hours  
**Estimated Azure Cost:** $86-151/month

### Key Findings

✅ **Strengths:**
- Small, manageable codebase with clear separation of concerns
- Modern frontend dependencies (Bootstrap 5, jQuery 3.7)
- No database complexity (in-memory repository)
- Well-structured MVC patterns

⚠️ **Critical Challenges:**
- WCF services not supported in .NET 10 (requires complete rewrite to Web API)
- MSMQ Windows-specific messaging prevents containerization
- Session-based state management incompatible with stateless containers
- Legacy project format requires conversion to SDK-style
- Heavy System.Web dependencies need replacement

---

## Current Architecture

### Application Components

#### 1. ProductCatalog (Web Application)
- **Type:** ASP.NET MVC 5 Web Application
- **Framework:** .NET Framework 4.8.1
- **Purpose:** Customer-facing product catalog with shopping cart
- **Key Features:**
  - Browse products from WCF service
  - Shopping cart with session state
  - Order submission via MSMQ
  - Responsive UI with Bootstrap 5

**Technologies:**
- ASP.NET MVC 5.2.9
- Razor view engine
- Bootstrap 5.2.3
- jQuery 3.7.0
- Newtonsoft.Json 13.0.3

**Dependencies:**
- WCF client proxy for ProductService
- System.Messaging for MSMQ
- System.Web for session management

#### 2. ProductServiceLibrary (WCF Service)
- **Type:** WCF Service Library
- **Framework:** .NET Framework 4.8.1
- **Purpose:** Product data service layer
- **Endpoints:** BasicHttpBinding on localhost:8733

**Service Operations:**
- GetAllProducts()
- GetProductById(int id)
- GetProductsByCategory(string category)
- SearchProducts(string term)
- GetCategories()
- CreateProduct(Product)
- UpdateProduct(Product)
- DeleteProduct(int id)
- GetProductsByPriceRange(decimal min, decimal max)

**Data Storage:** In-memory repository with sample data

#### 3. OrderProcessor (Console Application)
- **Type:** Console Application
- **Framework:** .NET Framework 4.8.1 (inferred)
- **Purpose:** Background order processing worker
- **Status:** Not included in solution file

**Functionality:**
- Polls MSMQ queue for orders
- Processes orders with simulated steps:
  - Payment validation
  - Inventory updates
  - Shipping label creation
  - Confirmation email sending

### Current Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                       User Browser                           │
└───────────────────────────┬─────────────────────────────────┘
                            │ HTTP
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                   ProductCatalog (MVC 5)                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │ Controllers  │  │ Razor Views  │  │ Session (Cart)   │  │
│  └──────┬───────┘  └──────────────┘  └──────────────────┘  │
│         │                                                     │
│         ├─────────────────────────────────┐                 │
└─────────┼─────────────────────────────────┼─────────────────┘
          │ WCF                              │ MSMQ
          ▼                                  ▼
┌──────────────────────────┐    ┌───────────────────────────┐
│  ProductServiceLibrary   │    │   MSMQ Queue              │
│     (WCF Service)        │    │   (ProductCatalogOrders)  │
│  ┌────────────────────┐  │    └───────────┬───────────────┘
│  │ ProductRepository  │  │                │
│  │  (In-Memory)       │  │                │ Poll
│  └────────────────────┘  │                ▼
└──────────────────────────┘    ┌───────────────────────────┐
                                │   OrderProcessor          │
                                │   (Console App)           │
                                └───────────────────────────┘
```

### Infrastructure

**Hosting Environment:**
- IIS/IIS Express for web application
- Self-hosted WCF service
- Windows OS required (MSMQ dependency)
- No containerization support

---

## Target Architecture

### Modernized Components

#### 1. ProductCatalog.Web (ASP.NET Core MVC)
- **Framework:** .NET 10
- **Container:** Linux container
- **Features:**
  - ASP.NET Core MVC with Razor Pages
  - HttpClient for API calls
  - Distributed session with Redis
  - Configuration via appsettings.json
  - Health checks
  - Application Insights

#### 2. ProductCatalog.Api (ASP.NET Core Web API)
- **Framework:** .NET 10
- **Container:** Linux container
- **Features:**
  - RESTful API endpoints
  - Swagger/OpenAPI documentation
  - In-memory repository (same data)
  - Dependency injection
  - Health checks
  - Application Insights

#### 3. ProductCatalog.Worker (Worker Service)
- **Framework:** .NET 10
- **Container:** Linux container
- **Features:**
  - Azure Service Bus message consumer
  - Background order processing
  - Retry logic and error handling
  - Health checks
  - Application Insights

### Target Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                       User Browser                           │
└───────────────────────────┬─────────────────────────────────┘
                            │ HTTPS
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              Azure Container Apps Environment                │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  ProductCatalog.Web (Container)                     │   │
│  │  ┌────────────┐  ┌──────────────┐  ┌────────────┐  │   │
│  │  │Controllers │  │ Razor Views  │  │ HttpClient │  │   │
│  │  └─────┬──────┘  └──────────────┘  └─────┬──────┘  │   │
│  └────────┼───────────────────────────────────┼────────┘   │
│           │                                    │             │
│           │ Session State                      │ HTTP/REST   │
│           ▼                                    ▼             │
│  ┌────────────────────┐         ┌──────────────────────┐   │
│  │ Azure Redis Cache  │         │ ProductCatalog.Api   │   │
│  │ (Distributed)      │         │    (Container)       │   │
│  └────────────────────┘         │ ┌──────────────────┐ │   │
│                                  │ │REST Controllers  │ │   │
│                                  │ │In-Memory Repo    │ │   │
│                                  │ └──────────────────┘ │   │
│                                  └──────────────────────┘   │
│                                                               │
│  ┌──────────────────┐            ┌──────────────────────┐   │
│  │ Web App sends    │───────────▶│ Azure Service Bus    │   │
│  │ order messages   │            │ (orders queue)       │   │
│  └──────────────────┘            └──────────┬───────────┘   │
│                                              │                │
│                                              │ Consume        │
│                                              ▼                │
│                                  ┌──────────────────────┐   │
│                                  │ ProductCatalog.Worker│   │
│                                  │    (Container)       │   │
│                                  │ ┌──────────────────┐ │   │
│                                  │ │Order Processor   │ │   │
│                                  │ │Service Bus SDK   │ │   │
│                                  │ └──────────────────┘ │   │
│                                  └──────────────────────┘   │
└───────────────────────────────────────────────────────────────┘
                            │
                            ▼
                ┌────────────────────────┐
                │ Application Insights   │
                │ (Monitoring & Logging) │
                └────────────────────────┘
```

### Azure Infrastructure

**Core Services:**
- **Azure Container Registry:** Store container images
- **Azure Container Apps Environment:** Host all containers
- **Azure Service Bus:** Message queue for order processing
- **Azure Redis Cache:** Distributed session and caching
- **Application Insights:** Monitoring and logging
- **Azure Key Vault:** Secrets management

---

## Legacy Patterns & Migration Needs

### 1. WCF Services ⚠️ **HIGH IMPACT**

**Current State:**
- ProductServiceLibrary uses WCF with BasicHttpBinding
- ProductCatalog consumes via WCF client proxy
- Service contracts defined with [ServiceContract] and [OperationContract]

**Issues:**
- WCF not supported in .NET Core/.NET 10
- System.ServiceModel dependencies incompatible
- SOAP-based communication not cloud-native

**Migration Path:**
```
WCF Service → ASP.NET Core Web API
- Convert service contracts to REST controllers
- Map operations to HTTP endpoints (GET, POST, PUT, DELETE)
- Use JSON serialization instead of SOAP
- Replace WCF client proxy with HttpClient
```

**Effort:** 16-20 hours

### 2. MSMQ Message Queuing ⚠️ **HIGH IMPACT**

**Current State:**
- System.Messaging used for order queue
- Windows-specific private queue: `.\Private$\ProductCatalogOrders`
- XmlMessageFormatter for serialization

**Issues:**
- MSMQ only available on Windows
- Prevents Linux containerization
- Not available in Azure PaaS services

**Migration Path:**
```
MSMQ → Azure Service Bus
- Replace MessageQueue with ServiceBusClient
- Convert to JSON message serialization
- Implement retry policies and dead-letter handling
- Use managed identity for authentication
```

**Effort:** 10-15 hours

### 3. Session State Management ⚠️ **HIGH IMPACT**

**Current State:**
- Shopping cart stored in `Session["Cart"]`
- In-memory session state (System.Web)
- Tied to single server instance

**Issues:**
- In-memory state lost when container restarts
- Doesn't work with multiple container replicas
- Prevents horizontal scaling

**Migration Path:**
```
In-Memory Session → Distributed Session
- Implement IDistributedCache with Redis
- Configure session state provider
- Serialize cart to JSON
- Use sliding expiration
```

**Effort:** 4-6 hours

---

## Complexity Assessment

### Overall Complexity: 8/10 (High)

**Factors Increasing Complexity:**

1. **Major Framework Upgrade (Impact: +3)**
   - .NET Framework 4.8.1 → .NET 10
   - Significant API changes
   - Namespace migrations

2. **WCF to Web API Migration (Impact: +2)**
   - Complete rewrite of service layer
   - Contract redesign from SOAP to REST

3. **MSMQ to Service Bus Migration (Impact: +2)**
   - Platform-specific to cloud-native messaging
   - Different message patterns

4. **Session State Redesign (Impact: +1)**
   - In-memory to distributed cache
   - Multi-instance coordination

**Factors Reducing Complexity:**

1. **Small Codebase (Impact: -1)**
   - Limited number of controllers and views
   - Simple domain model

2. **Clear Architecture (Impact: -1)**
   - Well-separated concerns
   - Standard MVC patterns

---

## Migration Phases

### Phase 1: Foundation & Project Modernization (8-10 hours)

**Tasks:**
1. Create new .NET 10 solution structure
2. Convert to SDK-style projects
3. Setup NuGet packages with PackageReference
4. Create Dockerfiles for each project
5. Create docker-compose.yml

**Deliverables:**
- ✅ Modern .NET 10 solution structure
- ✅ SDK-style project files
- ✅ Dockerfiles for all components

### Phase 2: Service Layer Migration (16-20 hours)

**Tasks:**
1. Create ProductCatalog.Api project
2. Migrate domain models
3. Create REST API controllers
4. Add Swagger documentation
5. Implement health checks
6. Test all endpoints

**Deliverables:**
- ✅ RESTful API service
- ✅ OpenAPI/Swagger documentation
- ✅ Containerized API service

### Phase 3: Web Application Migration (20-25 hours)

**Tasks:**
1. Create ProductCatalog.Web project
2. Migrate controllers and models
3. Update Razor views
4. Setup distributed session with Redis
5. Replace WCF client with HttpClient
6. Test all user flows

**Deliverables:**
- ✅ ASP.NET Core MVC web application
- ✅ Distributed session with Redis
- ✅ HttpClient integration with API

### Phase 4: Message Queue Migration (10-15 hours)

**Tasks:**
1. Setup Azure Service Bus
2. Update web app order submission
3. Create Worker Service
4. Implement order processing
5. Test end-to-end flow

**Deliverables:**
- ✅ Azure Service Bus integration
- ✅ Worker Service for order processing
- ✅ End-to-end message flow

### Phase 5: Azure Container Apps Deployment (6-10 hours)

**Tasks:**
1. Create Azure resources (ACR, Container Apps, Service Bus, Redis)
2. Push containers to ACR
3. Deploy Container Apps
4. Configure environment variables
5. Setup CI/CD pipeline
6. Test deployed application

**Deliverables:**
- ✅ All Azure resources provisioned
- ✅ Containers deployed to Azure
- ✅ CI/CD pipeline working

---

## Estimated Costs

### Monthly Azure Costs (Moderate Traffic)

| Service | SKU | Est. Cost |
|---------|-----|-----------|
| Container Apps | Consumption | $50-100 |
| Container Registry | Basic | $5 |
| Service Bus | Standard | $10 |
| Redis Cache | Basic C0 | $16 |
| Application Insights | Standard | $5-20 |
| **Total** | | **$86-151** |

---

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| WCF to REST API complexity | High | Design API contracts carefully, use integration tests |
| Session state loss | High | Implement distributed cache early, test thoroughly |
| MSMQ to Service Bus compatibility | Medium | Design clear message contracts, test serialization |

---

## Recommendations

### Critical Priority

1. **✅ Migrate WCF to ASP.NET Core Web API**
   - Required for .NET 10
   - Enables REST-based communication

2. **✅ Replace MSMQ with Azure Service Bus**
   - Required for containerization
   - Cloud-native messaging

3. **✅ Implement Distributed Session State**
   - Critical for stateless containers
   - Use Azure Redis Cache

### High Priority

4. **Convert to SDK-Style Projects**
   - Modern project format for .NET 10

5. **Add Docker Support**
   - Required for Azure Container Apps

6. **Setup Monitoring and Logging**
   - Application Insights for observability

---

## Success Criteria

✅ **Functional Requirements:**
- All functionality preserved
- Web app accessible via public endpoint
- Order processing working via Service Bus
- Cart persists across container restarts

✅ **Technical Requirements:**
- All components on .NET 10
- All components containerized
- No Windows dependencies
- Session state distributed

✅ **Operational Requirements:**
- CI/CD pipeline working
- Application Insights collecting telemetry
- Auto-scaling configured

---

## Next Steps

1. **Review and Approve Assessment**
2. **Setup Development Environment**
3. **Begin Phase 1: Foundation**
4. **Proceed Through Phases Sequentially**

---

**Assessment Complete**  
*Ready for migration plan generation and task creation*
