# Migration Task Breakdown

This document outlines all individual tasks required for the ProductCatalogApp modernization to .NET 10 and Azure Container Apps deployment. Each task should be created as a separate GitHub issue.

## Task Template
```
Title: [Phase] Task Name
Labels: modernization, phase-X
Dependencies: #issue_number (if applicable)
Complexity: 1-5 (1=simple, 5=complex)
```

---

## Phase 1: Foundation (3-5 days)

### Task 1.1: Convert ProductCatalog to SDK-Style Project
**Complexity:** 3/5  
**Dependencies:** None  
**Description:**
- Convert `ProductCatalog.csproj` from legacy format to SDK-style
- Remove unnecessary MSBuild imports
- Simplify project file structure
- Ensure project builds successfully

**Acceptance Criteria:**
- [ ] ProductCatalog.csproj uses SDK-style format
- [ ] Project builds without errors
- [ ] All existing files are included

### Task 1.2: Convert ProductServiceLibrary to SDK-Style Project
**Complexity:** 2/5  
**Dependencies:** None  
**Description:**
- Convert `ProductServiceLibrary.csproj` to SDK-style
- Remove WCF-specific project type GUIDs
- Simplify project references

**Acceptance Criteria:**
- [ ] ProductServiceLibrary.csproj uses SDK-style format
- [ ] Project builds without errors

### Task 1.3: Migrate ProductCatalog to .NET 10
**Complexity:** 4/5  
**Dependencies:** Task 1.1  
**Description:**
- Update TargetFramework to net10.0
- Replace packages.config with PackageReference
- Update NuGet packages to .NET 10 compatible versions
- Resolve compilation errors

**Acceptance Criteria:**
- [ ] TargetFramework is net10.0
- [ ] No packages.config file
- [ ] Project builds successfully on .NET 10

### Task 1.4: Create Configuration System for ProductCatalog
**Complexity:** 2/5  
**Dependencies:** Task 1.3  
**Description:**
- Create appsettings.json
- Migrate Web.config app settings
- Set up IConfiguration in Program.cs
- Update code to use IConfiguration

**Acceptance Criteria:**
- [ ] appsettings.json created with all settings
- [ ] Code uses IConfiguration instead of ConfigurationManager
- [ ] Application runs with new configuration

---

## Phase 2: Service Layer Modernization (3-5 days)

### Task 2.1: Create ProductCatalog.Api Project
**Complexity:** 3/5  
**Dependencies:** Task 1.2  
**Description:**
- Create new ASP.NET Core Web API project
- Set up project structure (Controllers, Models, Services)
- Configure Swagger/OpenAPI documentation
- Set up dependency injection

**Acceptance Criteria:**
- [ ] New ProductCatalog.Api project created
- [ ] Project targets .NET 10
- [ ] Swagger UI accessible
- [ ] Basic health check endpoint works

### Task 2.2: Migrate Product Models and Repository
**Complexity:** 2/5  
**Dependencies:** Task 2.1  
**Description:**
- Copy Product and Category models
- Migrate ProductRepository to new project
- Update namespace references
- Add any missing data annotations

**Acceptance Criteria:**
- [ ] Models migrated successfully
- [ ] Repository works in new project
- [ ] Unit tests pass (if applicable)

### Task 2.3: Implement Product API Endpoints
**Complexity:** 3/5  
**Dependencies:** Task 2.2  
**Description:**
- Create ProductsController
- Implement all endpoints matching WCF operations:
  - GET /api/products (GetAllProducts)
  - GET /api/products/{id} (GetProductById)
  - GET /api/products/category/{category} (GetProductsByCategory)
  - GET /api/products/search (SearchProducts)
  - GET /api/products/price-range (GetProductsByPriceRange)
  - GET /api/categories (GetCategories)
  - POST /api/products (CreateProduct)
  - PUT /api/products/{id} (UpdateProduct)
  - DELETE /api/products/{id} (DeleteProduct)
- Add proper HTTP status codes
- Add input validation

**Acceptance Criteria:**
- [ ] All API endpoints implemented
- [ ] Endpoints return correct data
- [ ] Proper error handling in place
- [ ] API documented in Swagger

### Task 2.4: Update ProductCatalog Web to Use New API
**Complexity:** 4/5  
**Dependencies:** Task 2.3  
**Description:**
- Remove WCF Service Reference
- Create HttpClient-based service client
- Update HomeController to use new client
- Configure API base URL in appsettings.json
- Test all product operations

**Acceptance Criteria:**
- [ ] WCF references removed
- [ ] All product operations work through API
- [ ] Error handling works correctly
- [ ] Web app functions as before

### Task 2.5: Add API Authentication (Optional)
**Complexity:** 3/5  
**Dependencies:** Task 2.3  
**Description:**
- Add JWT authentication to API
- Configure authentication in web app
- Secure API endpoints
- Add API key or OAuth flow

**Acceptance Criteria:**
- [ ] API endpoints secured
- [ ] Web app authenticates correctly
- [ ] Unauthorized requests blocked

---

## Phase 3: Web Application Migration (5-8 days)

### Task 3.1: Create ASP.NET Core MVC Project Structure
**Complexity:** 3/5  
**Dependencies:** Task 1.4  
**Description:**
- Create new ASP.NET Core MVC project
- Set up Program.cs with middleware pipeline
- Configure routing and static files
- Set up dependency injection

**Acceptance Criteria:**
- [ ] New ASP.NET Core MVC project created
- [ ] Basic routing works
- [ ] Static files served correctly

### Task 3.2: Migrate Models and ViewModels
**Complexity:** 2/5  
**Dependencies:** Task 3.1  
**Description:**
- Copy CartItem model
- Copy Order and OrderItem models
- Update namespaces
- Add data annotations for validation

**Acceptance Criteria:**
- [ ] All models migrated
- [ ] Models serialize/deserialize correctly
- [ ] Validation works

### Task 3.3: Migrate Controllers
**Complexity:** 4/5  
**Dependencies:** Task 3.2, Task 2.4  
**Description:**
- Migrate HomeController to ASP.NET Core
- Update action methods for ASP.NET Core patterns
- Replace TempData/Session usage patterns
- Update return types and attributes

**Acceptance Criteria:**
- [ ] All controller actions migrated
- [ ] Routing works correctly
- [ ] Actions return proper results

### Task 3.4: Migrate Razor Views
**Complexity:** 3/5  
**Dependencies:** Task 3.3  
**Description:**
- Copy all views to new project
- Update _Layout.cshtml for ASP.NET Core
- Update _ViewStart.cshtml
- Fix any Razor syntax differences
- Update bundling references

**Acceptance Criteria:**
- [ ] All views render correctly
- [ ] Layout and shared views work
- [ ] Client-side libraries load properly

### Task 3.5: Set Up Distributed Session State with Redis
**Complexity:** 3/5  
**Dependencies:** Task 3.3  
**Description:**
- Add Microsoft.Extensions.Caching.StackExchangeRedis package
- Configure Redis session state
- Update cart management to use distributed sessions
- Set up Redis connection configuration
- Test session persistence

**Acceptance Criteria:**
- [ ] Redis session provider configured
- [ ] Shopping cart persists across requests
- [ ] Sessions work across multiple instances
- [ ] Fallback behavior for Redis unavailability

### Task 3.6: Migrate Client-Side Assets
**Complexity:** 2/5  
**Dependencies:** Task 3.4  
**Description:**
- Copy wwwroot content
- Update asset references in views
- Configure bundling and minification
- Update CSS and JavaScript paths

**Acceptance Criteria:**
- [ ] All CSS files load correctly
- [ ] All JavaScript files work
- [ ] Bootstrap and jQuery function properly
- [ ] No console errors

### Task 3.7: Set Up Logging and Error Handling
**Complexity:** 2/5  
**Dependencies:** Task 3.1  
**Description:**
- Configure Serilog or built-in logging
- Add exception handling middleware
- Create error pages
- Add application insights (optional)

**Acceptance Criteria:**
- [ ] Logging configured
- [ ] Errors logged properly
- [ ] Error pages display correctly
- [ ] No unhandled exceptions

---

## Phase 4: Messaging Modernization (2-4 days)

### Task 4.1: Provision Azure Service Bus Resources
**Complexity:** 2/5  
**Dependencies:** None  
**Description:**
- Create Azure Service Bus namespace
- Create queue named "product-catalog-orders"
- Configure queue settings (TTL, dead-letter, etc.)
- Get connection string
- Document configuration

**Acceptance Criteria:**
- [ ] Service Bus namespace created
- [ ] Queue created and configured
- [ ] Connection string available
- [ ] Access policies configured

### Task 4.2: Create Service Bus Queue Service
**Complexity:** 3/5  
**Dependencies:** Task 4.1  
**Description:**
- Add Azure.Messaging.ServiceBus package
- Create new OrderQueueService using Service Bus SDK
- Implement SendOrder method
- Implement error handling and retry logic
- Add logging

**Acceptance Criteria:**
- [ ] OrderQueueService uses Service Bus
- [ ] Messages sent successfully
- [ ] Error handling works
- [ ] Retries configured

### Task 4.3: Update ProductCatalog Web to Use Service Bus
**Complexity:** 2/5  
**Dependencies:** Task 4.2, Task 3.3  
**Description:**
- Replace MSMQ OrderQueueService with Service Bus version
- Update configuration for connection string
- Update HomeController.SubmitOrder
- Test order submission

**Acceptance Criteria:**
- [ ] Orders sent to Service Bus successfully
- [ ] No MSMQ dependencies
- [ ] Order submission flow works
- [ ] Confirmation page displays

### Task 4.4: Migrate OrderProcessor to Service Bus
**Complexity:** 3/5  
**Dependencies:** Task 4.2  
**Description:**
- Update OrderProcessor to .NET 10
- Replace System.Messaging with Service Bus SDK
- Update message processing loop
- Implement proper shutdown handling
- Add structured logging

**Acceptance Criteria:**
- [ ] OrderProcessor uses Service Bus
- [ ] Messages received and processed
- [ ] Graceful shutdown works
- [ ] Error handling and dead-letter queue configured

### Task 4.5: Convert OrderProcessor to Worker Service
**Complexity:** 3/5  
**Dependencies:** Task 4.4  
**Description:**
- Create .NET Worker Service project
- Move processing logic to BackgroundService
- Configure dependency injection
- Set up health checks
- Add appsettings.json configuration

**Acceptance Criteria:**
- [ ] Worker Service runs continuously
- [ ] Health checks respond
- [ ] Proper cancellation token handling
- [ ] Configuration from appsettings.json

---

## Phase 5: Containerization & Deployment (2-3 days)

### Task 5.1: Create Dockerfile for ProductCatalog.Web
**Complexity:** 2/5  
**Dependencies:** Task 3.7  
**Description:**
- Create multi-stage Dockerfile
- Configure build and runtime stages
- Set up proper port exposure
- Add health check
- Test local container build

**Acceptance Criteria:**
- [ ] Dockerfile builds successfully
- [ ] Container runs locally
- [ ] Application accessible in container
- [ ] Health check works

### Task 5.2: Create Dockerfile for ProductCatalog.Api
**Complexity:** 2/5  
**Dependencies:** Task 2.3  
**Description:**
- Create multi-stage Dockerfile
- Configure build and runtime stages
- Set up proper port exposure
- Add health check
- Test local container build

**Acceptance Criteria:**
- [ ] Dockerfile builds successfully
- [ ] Container runs locally
- [ ] API accessible in container
- [ ] Health check works

### Task 5.3: Create Dockerfile for OrderProcessor Worker
**Complexity:** 2/5  
**Dependencies:** Task 4.5  
**Description:**
- Create multi-stage Dockerfile
- Configure build and runtime stages
- Set up health check
- Test local container build

**Acceptance Criteria:**
- [ ] Dockerfile builds successfully
- [ ] Container runs locally
- [ ] Worker processes messages
- [ ] Health check works

### Task 5.4: Create Docker Compose for Local Development
**Complexity:** 3/5  
**Dependencies:** Task 5.1, Task 5.2, Task 5.3  
**Description:**
- Create docker-compose.yml
- Configure all services
- Set up service dependencies
- Add Redis service
- Add environment variables
- Test full stack locally

**Acceptance Criteria:**
- [ ] All services start with docker-compose up
- [ ] Services communicate correctly
- [ ] Redis connection works
- [ ] Full application flow works locally

### Task 5.5: Create Azure Container Apps Environment
**Complexity:** 2/5  
**Dependencies:** Task 4.1  
**Description:**
- Create Azure Container Apps environment
- Configure networking
- Set up log analytics workspace
- Configure secrets and environment variables
- Document infrastructure

**Acceptance Criteria:**
- [ ] Container Apps environment created
- [ ] Log analytics configured
- [ ] Network configured properly

### Task 5.6: Deploy ProductCatalog.Api to Azure Container Apps
**Complexity:** 3/5  
**Dependencies:** Task 5.2, Task 5.5  
**Description:**
- Create Container App for API
- Configure container registry
- Build and push container image
- Configure scaling rules
- Set up ingress (internal only)
- Test API accessibility

**Acceptance Criteria:**
- [ ] Container App deployed
- [ ] API accessible within environment
- [ ] Health checks passing
- [ ] Scaling works

### Task 5.7: Deploy ProductCatalog.Web to Azure Container Apps
**Complexity:** 3/5  
**Dependencies:** Task 5.1, Task 5.6  
**Description:**
- Create Container App for web application
- Build and push container image
- Configure scaling rules
- Set up ingress (external)
- Configure custom domain (optional)
- Test web application

**Acceptance Criteria:**
- [ ] Container App deployed
- [ ] Web app accessible externally
- [ ] Can communicate with API
- [ ] Shopping cart works with Redis

### Task 5.8: Deploy OrderProcessor Worker to Azure Container Apps
**Complexity:** 3/5  
**Dependencies:** Task 5.3, Task 5.5  
**Description:**
- Create Container App for worker
- Build and push container image
- Configure scaling rules (1 instance)
- No ingress (background service)
- Test order processing

**Acceptance Criteria:**
- [ ] Container App deployed
- [ ] Worker processing messages
- [ ] Can connect to Service Bus
- [ ] Orders processed successfully

### Task 5.9: Configure Azure Redis Cache
**Complexity:** 2/5  
**Dependencies:** Task 5.7  
**Description:**
- Provision Azure Redis Cache
- Configure access policy
- Update web app connection string
- Test session state

**Acceptance Criteria:**
- [ ] Redis Cache provisioned
- [ ] Connection string configured
- [ ] Session state works in Azure

### Task 5.10: Set Up CI/CD Pipeline
**Complexity:** 4/5  
**Dependencies:** Task 5.8  
**Description:**
- Create GitHub Actions workflow
- Configure container registry authentication
- Set up build and push for all images
- Configure deployment to Container Apps
- Add environment variables as secrets
- Test automated deployment

**Acceptance Criteria:**
- [ ] Pipeline builds all containers
- [ ] Images pushed to registry
- [ ] Automatic deployment works
- [ ] Rollback capability exists

### Task 5.11: Configure Monitoring and Logging
**Complexity:** 3/5  
**Dependencies:** Task 5.8  
**Description:**
- Configure Application Insights
- Set up alerts for errors
- Configure log streaming
- Create Azure Dashboard
- Document monitoring setup

**Acceptance Criteria:**
- [ ] Application Insights configured
- [ ] Logs visible in Log Analytics
- [ ] Alerts configured
- [ ] Dashboard created

### Task 5.12: Load Testing and Performance Optimization
**Complexity:** 3/5  
**Dependencies:** Task 5.11  
**Description:**
- Create load test scenarios
- Run performance tests
- Analyze bottlenecks
- Optimize as needed
- Document performance metrics

**Acceptance Criteria:**
- [ ] Load tests completed
- [ ] Performance meets requirements
- [ ] No memory leaks
- [ ] Auto-scaling works under load

---

## Summary

**Total Tasks:** 36  
**Estimated Duration:** 15-25 days  
**Phases:** 5

**Task Complexity Distribution:**
- Simple (1-2): 13 tasks
- Medium (3): 17 tasks  
- Complex (4-5): 6 tasks

**Dependencies Flow:**
```
Phase 1 → Phase 2 → Phase 3
              ↓         ↓
         Phase 4 → Phase 5
```

---

## Task Labels to Use

- `modernization` - All tasks
- `phase-1-foundation` - Foundation tasks
- `phase-2-services` - Service layer tasks
- `phase-3-web` - Web application tasks
- `phase-4-messaging` - Messaging tasks
- `phase-5-deployment` - Containerization and deployment tasks
- `complexity-low` - Simple tasks (1-2)
- `complexity-medium` - Medium tasks (3)
- `complexity-high` - Complex tasks (4-5)
- `blocked` - Tasks waiting on dependencies
- `in-progress` - Currently being worked on
- `needs-review` - Completed, needs review
- `done` - Completed and verified

---

## Issue Creation Instructions

For each task above, create a GitHub issue with:

1. **Title:** [Phase X.Y] Task Name
2. **Description:** Copy the task description
3. **Labels:** Add appropriate labels
4. **Milestone:** Create milestones for each phase
5. **Dependencies:** Reference prerequisite task issue numbers
6. **Assignee:** Assign to appropriate developer
7. **Checklist:** Include acceptance criteria as task list

**Example Issue:**
```markdown
## Task 1.1: Convert ProductCatalog to SDK-Style Project

**Phase:** 1 - Foundation  
**Complexity:** 3/5  
**Dependencies:** None

### Description
Convert `ProductCatalog.csproj` from legacy format to SDK-style format to prepare for .NET 10 migration.

### Tasks
- [ ] Backup current .csproj file
- [ ] Convert to SDK-style format
- [ ] Remove unnecessary MSBuild imports
- [ ] Test that project builds
- [ ] Verify all files are included

### Acceptance Criteria
- [ ] ProductCatalog.csproj uses SDK-style format
- [ ] Project builds without errors
- [ ] All existing files are included
- [ ] No functionality regression

### Resources
- [SDK-style project migration guide](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview)
```
