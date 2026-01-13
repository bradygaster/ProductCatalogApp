# Migration Task Issues

This document contains detailed specifications for each migration task issue that should be created in GitHub.

---

## Issue 1: Migrate ProductServiceLibrary to ASP.NET Core Web API (.NET 10)

**Title:** Migrate ProductServiceLibrary from WCF to ASP.NET Core Web API (.NET 10)

**Labels:** enhancement, migration, .net10, api

**Priority:** High (P0)

**Complexity:** 9/10

**Estimated Effort:** 12-16 hours

**Dependencies:** None (Foundation task)

**Description:**

### Objective
Convert the ProductServiceLibrary WCF service to a modern ASP.NET Core Web API targeting .NET 10.

### Current State
- WCF Service Library (.NET Framework 4.8.1)
- SOAP-based communication
- Service contract: IProductService
- Operations: GetAllProducts, GetProductById, GetProductsByCategory, SearchProducts, GetCategories, CreateProduct, UpdateProduct, DeleteProduct, GetProductsByPriceRange

### Tasks
- [ ] Create new ASP.NET Core Web API project targeting .NET 10
- [ ] Port data models (Product, Category) to new project
- [ ] Implement ProductRepository in new project
- [ ] Create REST controllers replacing WCF service operations
  - [ ] ProductsController (GET, POST, PUT, DELETE endpoints)
  - [ ] CategoriesController
- [ ] Add Swagger/OpenAPI documentation
- [ ] Implement error handling and logging
- [ ] Add input validation with Data Annotations
- [ ] Create unit tests for API endpoints
- [ ] Test API locally before integration

### Acceptance Criteria
- [ ] All WCF operations available as REST endpoints
- [ ] Swagger documentation accessible at /swagger
- [ ] Unit tests pass with >80% coverage
- [ ] API returns proper HTTP status codes
- [ ] Error responses include useful messages

### Technical Notes
- Use Microsoft.AspNetCore.Mvc for controllers
- Use Swashbuckle.AspNetCore for Swagger
- Consider using DTOs separate from domain models
- Implement ILogger for logging

---

## Issue 2: Migrate ProductCatalog to ASP.NET Core MVC (.NET 10)

**Title:** Migrate ProductCatalog from ASP.NET MVC 5 to ASP.NET Core MVC (.NET 10)

**Labels:** enhancement, migration, .net10, web

**Priority:** High (P0)

**Complexity:** 8/10

**Estimated Effort:** 16-20 hours

**Dependencies:** Issue #1 (Product API must be available)

**Description:**

### Objective
Migrate the ProductCatalog web application from ASP.NET MVC 5 to ASP.NET Core MVC targeting .NET 10, replacing WCF client with HTTP client calls to the new REST API.

### Current State
- ASP.NET MVC 5 application (.NET Framework 4.8.1)
- WCF service client for product operations
- Session-based shopping cart
- Bootstrap 5 UI with jQuery

### Tasks
- [ ] Create new ASP.NET Core MVC project targeting .NET 10
- [ ] Port Models (CartItem, Order, OrderItem)
- [ ] Port Controllers (HomeController with all actions)
- [ ] Port Views (Index, Cart, OrderConfirmation, About, Contact)
- [ ] Update _Layout.cshtml for ASP.NET Core
- [ ] Replace WCF client with HttpClient/IHttpClientFactory
  - [ ] Create IProductApiClient interface
  - [ ] Implement ProductApiClient service
  - [ ] Configure HttpClient with base URL
- [ ] Configure middleware pipeline (routing, static files, etc.)
- [ ] Port static content (CSS, JavaScript, images)
- [ ] Update bundling and minification (use WebOptimizer)
- [ ] Configure session state (prepare for Redis in next issue)
- [ ] Test all user flows (browse, add to cart, checkout)

### Acceptance Criteria
- [ ] All pages render correctly
- [ ] Shopping cart functionality works
- [ ] Product browsing and filtering work
- [ ] UI matches original design
- [ ] No WCF dependencies remain
- [ ] Application runs on .NET 10

### Technical Notes
- Use IHttpClientFactory for HTTP calls
- Keep session state configuration flexible for Redis migration
- Update Razor view syntax where needed (minimal changes)
- Use appsettings.json for configuration
- Implement proper error handling for API calls

---

## Issue 3: Replace MSMQ with Azure Service Bus

**Title:** Replace MSMQ with Azure Service Bus for order processing

**Labels:** enhancement, migration, azure, messaging

**Priority:** High (P0)

**Complexity:** 7/10

**Estimated Effort:** 10-14 hours

**Dependencies:** Issue #2 (Web app must be on .NET 10)

**Description:**

### Objective
Replace Windows-specific MSMQ messaging with cloud-native Azure Service Bus for order queue processing.

### Current State
- OrderQueueService uses System.Messaging (MSMQ)
- Messages serialized as XML
- Queue path: .\Private$\ProductCatalogOrders
- OrderProcessor console app reads from MSMQ

### Tasks
- [ ] Provision Azure Service Bus namespace (Standard tier)
- [ ] Create queue named "orders"
- [ ] Update OrderQueueService class
  - [ ] Replace System.Messaging with Azure.Messaging.ServiceBus
  - [ ] Update SendOrder to use ServiceBusSender
  - [ ] Update ReceiveOrder to use ServiceBusReceiver
  - [ ] Change serialization from XML to JSON
- [ ] Add Azure Service Bus connection string to configuration
- [ ] Implement retry policies using ServiceBusRetryOptions
- [ ] Add error handling and dead-letter queue support
- [ ] Update OrderProcessor to consume from Service Bus
- [ ] Test message sending and receiving locally
- [ ] Document new queue configuration

### Acceptance Criteria
- [ ] Orders successfully sent to Azure Service Bus
- [ ] OrderProcessor receives and processes orders
- [ ] Failed messages moved to dead-letter queue
- [ ] No MSMQ dependencies remain
- [ ] Configuration uses Azure connection strings
- [ ] Retry logic handles transient failures

### Technical Notes
- Use Azure.Messaging.ServiceBus NuGet package
- Store connection string in Azure Key Vault for production
- Implement exponential backoff for retries
- Consider message sessions for ordered processing
- Add Application Insights correlation

### Azure Resources
```bash
# Create Service Bus namespace
az servicebus namespace create \
  --resource-group <rg-name> \
  --name <namespace-name> \
  --location <location> \
  --sku Standard

# Create queue
az servicebus queue create \
  --resource-group <rg-name> \
  --namespace-name <namespace-name> \
  --name orders \
  --max-size 1024
```

---

## Issue 4: Implement Distributed Session State with Azure Cache for Redis

**Title:** Replace in-memory session state with Azure Cache for Redis

**Labels:** enhancement, migration, azure, scalability

**Priority:** High (P0)

**Complexity:** 6/10

**Estimated Effort:** 6-8 hours

**Dependencies:** Issue #2 (Web app must be migrated)

**Description:**

### Objective
Replace in-memory session state with distributed session state using Azure Cache for Redis to enable stateless containers and horizontal scaling.

### Current State
- Shopping cart stored in Session["Cart"]
- In-memory session state (ASP.NET Session)
- Session lost on app restart or scale-out

### Tasks
- [ ] Provision Azure Cache for Redis (Basic or Standard tier)
- [ ] Add Microsoft.Extensions.Caching.StackExchangeRedis package
- [ ] Configure Redis session state provider in Program.cs
- [ ] Add Redis connection string to appsettings.json
- [ ] Test session persistence across app restarts
- [ ] Verify cart data survives container restart
- [ ] Configure session timeout appropriately
- [ ] Add health check for Redis connectivity
- [ ] Document Redis configuration

### Acceptance Criteria
- [ ] Shopping cart persists across app restarts
- [ ] Session data stored in Redis, not in-memory
- [ ] Cart survives container scaling operations
- [ ] Session timeout configured appropriately (20-30 minutes)
- [ ] Health check reports Redis status
- [ ] No data loss during normal operations

### Technical Notes
- Use Microsoft.Extensions.Caching.StackExchangeRedis
- Configure session options in Program.cs:
  ```csharp
  builder.Services.AddStackExchangeRedisCache(options =>
  {
      options.Configuration = builder.Configuration.GetConnectionString("Redis");
  });
  builder.Services.AddSession(options =>
  {
      options.IdleTimeout = TimeSpan.FromMinutes(30);
      options.Cookie.HttpOnly = true;
      options.Cookie.IsEssential = true;
  });
  ```
- Store Redis connection string in Azure Key Vault
- Consider using Redis for other caching needs

### Azure Resources
```bash
# Create Redis cache
az redis create \
  --resource-group <rg-name> \
  --name <cache-name> \
  --location <location> \
  --sku Basic \
  --vm-size c0
```

---

## Issue 5: Create OrderProcessor as .NET 10 Console App

**Title:** Create OrderProcessor as proper .NET 10 console application

**Labels:** enhancement, migration, .net10, backend

**Priority:** Medium (P1)

**Complexity:** 5/10

**Estimated Effort:** 8-10 hours

**Dependencies:** Issue #3 (Azure Service Bus must be configured)

**Description:**

### Objective
Convert the standalone OrderProcessor files into a properly structured .NET 10 console application that processes orders from Azure Service Bus.

### Current State
- Standalone Program.cs and App.config files
- No proper project structure
- Uses MSMQ for message consumption
- .NET Framework 4.8.1

### Tasks
- [ ] Create new .NET 10 Console Application project
- [ ] Add Azure.Messaging.ServiceBus package
- [ ] Port Order and OrderItem models
- [ ] Implement ServiceBusProcessor for message handling
- [ ] Add structured logging with ILogger
- [ ] Implement graceful shutdown handling
- [ ] Add configuration using appsettings.json
- [ ] Implement order processing workflow
  - [ ] Payment validation (simulated)
  - [ ] Inventory update (simulated)
  - [ ] Shipping label creation (simulated)
  - [ ] Confirmation email (simulated/logged)
- [ ] Add health monitoring
- [ ] Create Dockerfile (for Issue #6)
- [ ] Add error handling and retry logic
- [ ] Document deployment and operation

### Acceptance Criteria
- [ ] Proper .csproj project structure
- [ ] Consumes messages from Azure Service Bus
- [ ] Processes orders successfully
- [ ] Logs all operations with structured logging
- [ ] Handles errors gracefully
- [ ] Supports graceful shutdown (SIGTERM)
- [ ] Ready for containerization

### Technical Notes
- Use Microsoft.Extensions.Hosting for host builder
- Implement IHostedService for background processing
- Use ILogger for structured logging
- Configure long-running process with proper cancellation tokens
- Consider using Azure Functions as alternative

### Example Structure
```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<OrderProcessorService>();
builder.Services.AddSingleton<IOrderProcessor, OrderProcessor>();

var host = builder.Build();
await host.RunAsync();
```

---

## Issue 6: Containerize Applications with Docker

**Title:** Create Docker containers for all applications

**Labels:** enhancement, docker, containerization, devops

**Priority:** High (P0)

**Complexity:** 5/10

**Estimated Effort:** 10-12 hours

**Dependencies:** Issues #1, #2, #5 (All apps must be on .NET 10)

**Description:**

### Objective
Create optimized Docker containers for the Product API, Web App, and Order Processor to enable deployment on Azure Container Apps.

### Current State
- Applications run on Windows/.NET Framework
- No containerization
- IIS hosting for web applications

### Tasks

#### Product API
- [ ] Create Dockerfile for Product API
- [ ] Use multi-stage build (build, publish, runtime)
- [ ] Configure HTTPS in container
- [ ] Add health check endpoint
- [ ] Optimize image size
- [ ] Test container locally

#### Web App
- [ ] Create Dockerfile for Web App
- [ ] Use multi-stage build
- [ ] Configure static file serving
- [ ] Add health check endpoint
- [ ] Test container locally

#### Order Processor
- [ ] Create Dockerfile for Order Processor
- [ ] Use multi-stage build
- [ ] Configure long-running process
- [ ] Add health monitoring
- [ ] Test container locally

#### Integration
- [ ] Create docker-compose.yml for local testing
- [ ] Configure networking between containers
- [ ] Test full application flow in containers
- [ ] Document container configuration
- [ ] Create .dockerignore files
- [ ] Add container health checks

### Acceptance Criteria
- [ ] All three applications have working Dockerfiles
- [ ] Images use multi-stage builds for optimization
- [ ] Images are < 200MB each (runtime)
- [ ] Health checks configured for all containers
- [ ] docker-compose.yml works for local testing
- [ ] All containers start successfully
- [ ] Full application flow works in containers

### Technical Notes

#### Dockerfile Template (API/Web)
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ProjectName.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectName.dll"]
```

#### Health Check Endpoints
- Web App: `/health`
- Product API: `/health`
- Order Processor: Custom health check logic

---

## Issue 7: Configure Azure Container Apps Deployment

**Title:** Deploy applications to Azure Container Apps

**Labels:** enhancement, azure, deployment, devops

**Priority:** High (P0)

**Complexity:** 6/10

**Estimated Effort:** 12-16 hours

**Dependencies:** Issue #6 (Containers must be ready), Issues #3, #4 (Azure resources provisioned)

**Description:**

### Objective
Deploy all containerized applications to Azure Container Apps with proper configuration, scaling, and monitoring.

### Prerequisites
- Azure subscription
- Azure Container Registry provisioned
- Azure Service Bus namespace created (Issue #3)
- Azure Cache for Redis created (Issue #4)
- Docker images built (Issue #6)

### Tasks

#### Infrastructure Setup
- [ ] Create Azure Container Apps environment
- [ ] Create Azure Container Registry (ACR)
- [ ] Push Docker images to ACR
- [ ] Configure virtual network (optional)
- [ ] Set up Log Analytics workspace

#### Product API Deployment
- [ ] Create Container App for Product API
- [ ] Configure ingress (external, port 8080)
- [ ] Set environment variables
- [ ] Configure scaling rules (HTTP-based)
- [ ] Enable Application Insights

#### Web App Deployment
- [ ] Create Container App for Web App
- [ ] Configure ingress (external, port 8080)
- [ ] Set environment variables (API URL, Redis connection)
- [ ] Configure scaling rules
- [ ] Enable Application Insights

#### Order Processor Deployment
- [ ] Create Container App for Order Processor
- [ ] Configure as background worker (no ingress)
- [ ] Set environment variables (Service Bus connection)
- [ ] Configure scaling rules (queue-based)
- [ ] Enable Application Insights

#### CI/CD Pipeline
- [ ] Create GitHub Actions workflow
- [ ] Configure build and push to ACR
- [ ] Configure automatic deployment
- [ ] Add environment-specific configurations
- [ ] Test automated deployment

#### Monitoring and Observability
- [ ] Configure Application Insights dashboards
- [ ] Set up alerts for errors and performance
- [ ] Configure log aggregation
- [ ] Test distributed tracing
- [ ] Document monitoring setup

### Acceptance Criteria
- [ ] All three applications deployed successfully
- [ ] Web App accessible via HTTPS
- [ ] Product API responding to requests
- [ ] Order Processor processing messages
- [ ] Auto-scaling configured and tested
- [ ] CI/CD pipeline working
- [ ] Monitoring and alerts operational
- [ ] Health checks passing
- [ ] No errors in Application Insights

### Technical Notes

#### Azure CLI Commands
```bash
# Create Container Apps environment
az containerapp env create \
  --name myenv \
  --resource-group myrg \
  --location eastus

# Create Container Registry
az acr create \
  --resource-group myrg \
  --name myacr \
  --sku Basic

# Create Container App (example)
az containerapp create \
  --name product-api \
  --resource-group myrg \
  --environment myenv \
  --image myacr.azurecr.io/product-api:latest \
  --target-port 8080 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 10
```

#### Scaling Rules
- Web App: HTTP concurrency-based (100 requests per instance)
- Product API: HTTP concurrency-based (100 requests per instance)
- Order Processor: Queue length-based (5 messages per instance)

#### Environment Variables
Manage via Azure Key Vault references:
- `ServiceBus__ConnectionString`
- `Redis__ConnectionString`
- `ProductApi__BaseUrl`
- `ApplicationInsights__ConnectionString`

### GitHub Actions Workflow (Example)
```yaml
name: Deploy to Azure Container Apps

on:
  push:
    branches: [main]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Build and push images
        run: |
          # Build and push logic
      - name: Deploy to Container Apps
        run: |
          # Deployment logic
```

---

## Task Dependencies Graph

```
Issue #1 (Product API)
    ↓
Issue #2 (Web App) ────→ Issue #3 (Service Bus)
    ↓                           ↓
Issue #4 (Redis)           Issue #5 (Order Processor)
    ↓                           ↓
    └──────→ Issue #6 (Containerization) ←────┘
                    ↓
            Issue #7 (Azure Deployment)
```

## Execution Order

1. **Week 1-2:** Issues #1, #2 (Foundation and Web App)
2. **Week 2-3:** Issues #3, #4 (Messaging and State)
3. **Week 3-4:** Issue #5 (Order Processor)
4. **Week 4-5:** Issue #6 (Containerization)
5. **Week 5-6:** Issue #7 (Azure Deployment)

---

## Summary

These 7 issues represent the complete modernization path from .NET Framework 4.8.1 to .NET 10 with Azure Container Apps deployment. Each issue is designed to be:

- **Independent where possible** to enable parallel work
- **Well-defined** with clear acceptance criteria
- **Testable** at each stage
- **Documented** for future reference

Total estimated effort: **60-86 hours** across all issues.

---

**Document Version:** 1.0  
**Created:** 2026-01-13  
**For Repository:** bradygaster/ProductCatalogApp
