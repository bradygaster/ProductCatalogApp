# ProductCatalogApp Modernization Assessment

**Assessment Date:** January 17, 2026  
**Repository:** bradygaster/ProductCatalogApp  
**Branch:** modernize/assess  
**Assessment Version:** 1.0

---

## Executive Summary

The ProductCatalogApp is a legacy .NET Framework 4.8.1 application that requires modernization to take advantage of modern cloud-native technologies and deployment to Azure Container Apps. This assessment identifies the current state, target state, and migration path for upgrading the application to .NET 10 with cloud-ready architecture.

**Key Findings:**
- Application currently uses .NET Framework 4.8.1 with ASP.NET MVC 5
- WCF service for product operations (Windows-specific technology)
- MSMQ for order queue processing (Windows-specific)
- Legacy project format and dependencies
- Not containerized or cloud-ready

**Recommended Approach:**
- Upgrade to .NET 10
- Migrate ASP.NET MVC 5 → ASP.NET Core MVC
- Convert WCF → gRPC
- Replace MSMQ → Azure Service Bus
- Containerize all components
- Deploy to Azure Container Apps

**Estimated Effort:** 4-6 weeks

---

## Current State Analysis

### Application Architecture

The ProductCatalogApp consists of two main projects:

1. **ProductCatalog** - ASP.NET MVC 5 Web Application
   - Serves as the user-facing web interface
   - Consumes ProductServiceLibrary via WCF
   - Handles shopping cart and order submission
   - Uses MSMQ for order processing

2. **ProductServiceLibrary** - WCF Service Library
   - Provides product catalog operations
   - Implements CRUD operations for products
   - Uses in-memory data store (ProductRepository)

### Technology Stack

#### Framework & Runtime
- **.NET Framework:** 4.8.1 (Windows-only)
- **Target Framework Moniker:** net481
- **Project Format:** Legacy .csproj (not SDK-style)

#### Web Application (ProductCatalog)
- **Framework:** ASP.NET MVC 5
- **Key Dependencies:**
  - System.Web.Mvc 5.2.9
  - Microsoft.AspNet.Mvc 5.2.9
  - Microsoft.AspNet.WebPages 3.2.9
  - Microsoft.AspNet.Razor 3.2.9
  - System.Web.Optimization 1.1.3
  - Newtonsoft.Json 13.0.3
  - Bootstrap 5.x
  - jQuery 3.7.0

- **Architecture Patterns:**
  - MVC pattern with Controllers, Views, Models
  - Session-based state management for shopping cart
  - WCF service consumption via generated proxy
  - MSMQ integration for order processing

#### Service Layer (ProductServiceLibrary)
- **Technology:** Windows Communication Foundation (WCF)
- **Service Contract:** IProductService
- **Binding:** basicHttpBinding
- **Key Dependencies:**
  - System.ServiceModel
  - System.Runtime.Serialization

#### Messaging Infrastructure
- **Technology:** Microsoft Message Queuing (MSMQ)
- **Queue Path:** `.\Private$\ProductCatalogOrders`
- **Implementation:** OrderQueueService.cs
- **Dependencies:** System.Messaging
- **Operations:**
  - Send orders to queue
  - Receive orders from queue
  - Queue message count tracking

### Code Analysis

#### Legacy Patterns Identified

1. **System.Web Dependencies**
   - Global.asax for application startup
   - Web.config for configuration
   - Session state management
   - Controllers inherit from System.Web.Mvc.Controller

2. **WCF Service Implementation**
   - ServiceContract and OperationContract attributes
   - FaultException for error handling
   - App.config for service configuration
   - Connected Services with generated proxy code

3. **MSMQ Integration**
   ```csharp
   // ProductCatalog/Services/OrderQueueService.cs
   - MessageQueue.Create()
   - MessageQueue.Send()
   - MessageQueue.Receive()
   - XmlMessageFormatter
   ```

4. **Legacy Project Format**
   - Uses ToolsVersion="15.0"
   - Package references via packages.config
   - Assembly references in .csproj
   - Build targets from MSBuildExtensionsPath

### Dependencies Analysis

#### NuGet Packages (packages.config)
- Microsoft.AspNet.Mvc@5.2.9
- Microsoft.AspNet.WebPages@3.2.9
- Microsoft.AspNet.Razor@3.2.9
- Microsoft.AspNet.Web.Optimization@1.1.3
- Microsoft.CodeDom.Providers.DotNetCompilerPlatform@2.0.1
- Microsoft.Web.Infrastructure@2.0.0
- Newtonsoft.Json@13.0.3
- WebGrease@1.6.0
- Antlr@3.5.0.2
- bootstrap@5.x
- jQuery@3.7.0

#### System References (Windows-specific)
- System.Web (ASP.NET Framework)
- System.ServiceModel (WCF)
- System.Messaging (MSMQ)
- System.Web.Mvc
- System.Web.Optimization

---

## Target State

### Modern Architecture

The modernized application will be restructured into multiple containerized services:

1. **ProductCatalog.Web** - ASP.NET Core MVC Web Application
   - Modern ASP.NET Core 10 MVC application
   - gRPC client for product service
   - Azure Service Bus integration for orders
   - Distributed caching for session state

2. **ProductCatalog.GrpcService** - gRPC Service
   - Modern service implementation using gRPC
   - Protocol Buffers for contract definition
   - Efficient binary serialization
   - Cross-platform compatible

3. **ProductCatalog.OrderProcessor** - Worker Service
   - Background service for order processing
   - Consumes messages from Azure Service Bus
   - Can scale independently
   - Implements retry and error handling

4. **ProductCatalog.Contracts** - Shared Library
   - Common models and DTOs
   - Shared between all services
   - Protocol Buffers definitions

### Technology Stack

#### Framework & Runtime
- **.NET:** 10.0
- **Target Framework Moniker:** net10.0
- **Runtime:** linux-x64 (for containers)
- **Project Format:** SDK-style

#### Web Application
- **Framework:** ASP.NET Core MVC
- **Key Changes:**
  - Replace Global.asax with Program.cs
  - Replace Web.config with appsettings.json
  - Use dependency injection
  - Implement distributed caching
  - Modern middleware pipeline

#### Service Layer
- **Technology:** gRPC (ASP.NET Core)
- **Benefits:**
  - Cross-platform support
  - High performance
  - Strong typing with Protocol Buffers
  - Streaming support
  - Modern tooling

#### Messaging Infrastructure
- **Technology:** Azure Service Bus
- **Configuration:**
  - Queue: `product-catalog-orders`
  - Connection via managed identity or connection string
  - Built-in retry policies
  - Dead-letter queue support
- **Benefits:**
  - Fully managed cloud service
  - High availability and reliability
  - Advanced features (sessions, transactions, etc.)
  - No Windows dependency

#### Containerization
- **Container Runtime:** Docker
- **Base Images:**
  - Runtime: mcr.microsoft.com/dotnet/aspnet:10.0
  - Build: mcr.microsoft.com/dotnet/sdk:10.0
- **Orchestration:** Azure Container Apps

#### Azure Services
1. **Azure Container Apps**
   - Host all containerized services
   - Auto-scaling capabilities
   - Integrated networking
   - HTTPS ingress

2. **Azure Service Bus**
   - Replace MSMQ
   - Namespace with queue
   - Standard or Premium tier

3. **Azure Container Registry**
   - Store container images
   - Integrated with Container Apps
   - Secure image scanning

4. **Azure Redis Cache** (Optional)
   - Distributed session state
   - Output caching
   - Data caching

---

## Migration Path

### Phase 1: Project Structure & Framework Upgrade (Week 1)

**Tasks:**
1. Convert legacy .csproj to SDK-style format
2. Update target framework to net10.0
3. Migrate packages.config to PackageReference
4. Remove legacy build targets
5. Update solution structure

**Deliverables:**
- SDK-style project files
- .NET 10 compatible projects
- Updated solution structure

### Phase 2: Web Application Migration (Weeks 2-3)

**Tasks:**
1. Create new ASP.NET Core MVC project
2. Migrate controllers to ASP.NET Core
3. Update views for ASP.NET Core
4. Convert Web.config to appsettings.json
5. Replace Global.asax with Program.cs
6. Implement dependency injection
7. Update routing configuration
8. Migrate session state to distributed cache
9. Update authentication/authorization if present
10. Test web application functionality

**Deliverables:**
- Functional ASP.NET Core MVC web application
- Updated views and controllers
- Configuration in appsettings.json
- Passing unit and integration tests

### Phase 3: WCF to gRPC Migration (Weeks 2-3, parallel)

**Tasks:**
1. Define Protocol Buffers (.proto files) for service contracts
2. Create gRPC service project
3. Implement gRPC service methods
4. Migrate ProductRepository logic
5. Update web application to use gRPC client
6. Remove WCF dependencies
7. Test gRPC service functionality

**Deliverables:**
- .proto definitions
- Functional gRPC service
- gRPC client integration in web app
- Passing service tests

**Example Service Contract:**
```protobuf
syntax = "proto3";

package ProductCatalog;

service ProductService {
  rpc GetAllProducts (Empty) returns (ProductList);
  rpc GetProductById (ProductIdRequest) returns (Product);
  rpc GetProductsByCategory (CategoryRequest) returns (ProductList);
  rpc CreateProduct (Product) returns (Product);
  rpc UpdateProduct (Product) returns (UpdateResult);
  rpc DeleteProduct (ProductIdRequest) returns (DeleteResult);
}
```

### Phase 4: MSMQ to Azure Service Bus Migration (Week 4)

**Tasks:**
1. Install Azure.Messaging.ServiceBus package
2. Create Azure Service Bus namespace and queue
3. Create new OrderQueueService implementation
4. Replace MSMQ send operations with Service Bus
5. Create Worker Service project for order processing
6. Implement Service Bus message receiver
7. Add error handling and retry logic
8. Update configuration for connection strings
9. Test end-to-end message flow

**Deliverables:**
- Service Bus-based OrderQueueService
- Worker Service for order processing
- Configuration for Service Bus
- Message processing tests

### Phase 5: Containerization (Week 5)

**Tasks:**
1. Create Dockerfile for web application
2. Create Dockerfile for gRPC service
3. Create Dockerfile for worker service
4. Add .dockerignore files
5. Create docker-compose.yml for local development
6. Test local container builds
7. Optimize container images (multi-stage builds)
8. Document container configuration

**Deliverables:**
- Dockerfiles for all services
- docker-compose.yml for local dev
- Container build scripts
- Documentation

**Example Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ProductCatalog.Web/ProductCatalog.Web.csproj", "ProductCatalog.Web/"]
RUN dotnet restore "ProductCatalog.Web/ProductCatalog.Web.csproj"
COPY . .
RUN dotnet build "ProductCatalog.Web/ProductCatalog.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProductCatalog.Web/ProductCatalog.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductCatalog.Web.dll"]
```

### Phase 6: Azure Container Apps Deployment (Week 6)

**Tasks:**
1. Create Azure Container Apps environment
2. Configure Azure Container Registry
3. Build and push container images
4. Deploy web application container
5. Deploy gRPC service container
6. Deploy worker service container
7. Configure environment variables and secrets
8. Set up service-to-service communication
9. Configure ingress rules
10. Set up monitoring and logging
11. Test deployed application
12. Document deployment process

**Deliverables:**
- Deployed application on Azure Container Apps
- Infrastructure as Code (Bicep/Terraform)
- Deployment documentation
- Monitoring dashboards

---

## Migration Tasks Detail

### 1. Convert to SDK-Style Projects
**Category:** Project Structure  
**Effort:** Medium  
**Priority:** High  

**Description:**  
Convert legacy .csproj format to modern SDK-style format.

**Files Affected:**
- ProductCatalog/ProductCatalog.csproj
- ProductServiceLibrary/ProductServiceLibrary.csproj

**Benefits:**
- Simpler project files
- Built-in NuGet package management
- Better tooling support
- Required for .NET 10

### 2. Upgrade to .NET 10
**Category:** Framework Upgrade  
**Effort:** Low  
**Priority:** High  
**Dependencies:** Task #1  

**Description:**  
Update target framework from net481 to net10.0.

**Changes:**
- Update TargetFramework in all projects
- Update NuGet packages to .NET 10 compatible versions
- Fix any API breaking changes

### 3. Migrate ASP.NET MVC 5 to ASP.NET Core MVC
**Category:** Web Framework  
**Effort:** High  
**Priority:** High  
**Dependencies:** Task #2  

**Description:**  
Convert web application from System.Web to ASP.NET Core.

**Sub-tasks:**
- Replace Global.asax with Program.cs
- Convert Web.config to appsettings.json
- Update controllers (remove System.Web.Mvc dependency)
- Update views for ASP.NET Core
- Replace Session with distributed cache
- Update routing configuration
- Replace BundleConfig with TagHelpers or static files
- Update FilterConfig to use middleware

### 4. Migrate WCF to gRPC
**Category:** Services  
**Effort:** High  
**Priority:** High  
**Dependencies:** Task #2  

**Description:**  
Convert WCF service to modern gRPC service.

**Sub-tasks:**
- Define .proto files for service contracts
- Create gRPC service project
- Implement service methods
- Generate gRPC client code
- Update web app to use gRPC client
- Remove WCF dependencies and proxy code

### 5. Replace MSMQ with Azure Service Bus
**Category:** Messaging  
**Effort:** Medium  
**Priority:** High  
**Dependencies:** Task #2  

**Description:**  
Migrate from MSMQ to Azure Service Bus.

**Sub-tasks:**
- Install Azure.Messaging.ServiceBus package
- Create Service Bus namespace and queue in Azure
- Implement Service Bus sender in OrderQueueService
- Create Worker Service for message processing
- Implement message receiver with error handling
- Update configuration for connection strings
- Remove System.Messaging dependencies

### 6. Add Docker Support
**Category:** Containerization  
**Effort:** Medium  
**Priority:** High  
**Dependencies:** Tasks #3, #4, #5  

**Description:**  
Containerize all application components.

**Sub-tasks:**
- Create Dockerfile for web app
- Create Dockerfile for gRPC service
- Create Dockerfile for worker service
- Add .dockerignore files
- Create docker-compose.yml
- Optimize image sizes

### 7. Configure Azure Container Apps Deployment
**Category:** Deployment  
**Effort:** Medium  
**Priority:** Medium  
**Dependencies:** Task #6  

**Description:**  
Set up deployment to Azure Container Apps.

**Sub-tasks:**
- Create Container Apps environment
- Configure Container Registry
- Deploy containers
- Set up networking
- Configure environment variables
- Set up monitoring

### 8. Update NuGet Packages
**Category:** Dependencies  
**Effort:** Low  
**Priority:** High  
**Dependencies:** Tasks #2, #3, #4, #5  

**Description:**  
Update all packages to .NET 10 compatible versions.

---

## Risks and Challenges

### 1. Session State Migration
**Risk Level:** High  
**Impact:** Application behavior  

**Description:**  
Current application uses in-memory session state for shopping cart, which won't work in containerized/distributed environment.

**Mitigation:**
- Implement distributed caching using Azure Redis Cache
- Or refactor to use client-side state or database-backed state
- Ensure session serialization works correctly

### 2. WCF to gRPC Migration Complexity
**Risk Level:** Medium  
**Impact:** Service contract and client code  

**Description:**  
Service contracts need complete redefinition in Protocol Buffers format with potential data structure changes.

**Mitigation:**
- Create parallel gRPC service initially
- Thoroughly test all service operations
- Use gRPC reflection for debugging
- Consider creating adapter layer if needed

### 3. MSMQ to Service Bus Behavioral Differences
**Risk Level:** Medium  
**Impact:** Message processing reliability  

**Description:**  
Azure Service Bus has different features, guarantees, and behaviors compared to MSMQ.

**Mitigation:**
- Thoroughly test all messaging scenarios
- Implement proper error handling and retry logic
- Use dead-letter queues for failed messages
- Monitor message processing metrics

### 4. Container Image Size and Startup Time
**Risk Level:** Low  
**Impact:** Deployment speed and cost  

**Description:**  
Large container images can slow down deployments and increase costs.

**Mitigation:**
- Use multi-stage Docker builds
- Optimize layer caching
- Use smaller base images where possible
- Implement health checks for faster readiness detection

### 5. Breaking Changes in .NET 10
**Risk Level:** Low  
**Impact:** Code compilation and runtime behavior  

**Description:**  
.NET 10 may have breaking changes from .NET Framework 4.8.1.

**Mitigation:**
- Review .NET upgrade documentation
- Run comprehensive test suite
- Use static code analysis tools
- Test thoroughly in staging environment

---

## Effort Estimation

### Total Estimated Effort: 4-6 weeks

**Breakdown by Task:**

| Task | Effort | Priority |
|------|--------|----------|
| Project structure & framework upgrade | 1 week | High |
| Web application migration | 2 weeks | High |
| WCF to gRPC migration | 1.5 weeks | High |
| MSMQ to Service Bus migration | 1 week | High |
| Containerization | 0.5 weeks | High |
| Azure Container Apps setup | 1 week | Medium |
| Testing & validation | 1 week | High |

**Assumptions:**
- Team has experience with .NET Core and Azure
- Development and test environments are available
- Azure subscription and permissions are ready
- Stakeholder availability for requirement clarification

---

## Recommendations

### Immediate Actions
1. **Start with framework upgrade** - Get projects on .NET 10 first
2. **Set up development environment** - Ensure team has necessary tools
3. **Create Azure resources early** - Set up Service Bus and Container Registry
4. **Implement feature flags** - Enable gradual rollout of new features

### Best Practices
1. **Parallel migration** - Migrate web and services simultaneously where possible
2. **Incremental testing** - Test each component as it's migrated
3. **Maintain backward compatibility** - Keep old code working during migration
4. **Comprehensive logging** - Add structured logging early for debugging
5. **Health checks** - Implement health endpoints for all services
6. **Infrastructure as Code** - Use Bicep or Terraform for Azure resources

### Additional Enhancements
1. **Add unit and integration tests** - Ensure quality during migration
2. **Implement CI/CD pipeline** - Automate builds and deployments
3. **Add monitoring and alerting** - Use Application Insights
4. **Security hardening** - Implement authentication, authorization, secrets management
5. **Performance optimization** - Benchmark and optimize critical paths
6. **Documentation** - Document architecture, deployment, and operations

### Long-term Considerations
1. **Database migration** - Currently uses in-memory store; consider Azure SQL or Cosmos DB
2. **API gateway** - Consider Azure API Management for API orchestration
3. **Identity management** - Implement Azure AD B2C for user authentication
4. **CDN integration** - Use Azure CDN for static content
5. **Multi-region deployment** - Plan for geographic distribution
6. **Disaster recovery** - Implement backup and recovery procedures

---

## Success Criteria

The modernization will be considered successful when:

1. ✅ All projects target .NET 10
2. ✅ Web application runs on ASP.NET Core MVC
3. ✅ WCF service replaced with functional gRPC service
4. ✅ MSMQ replaced with Azure Service Bus
5. ✅ All components containerized with Docker
6. ✅ Application deployed to Azure Container Apps
7. ✅ All existing functionality preserved and tested
8. ✅ Performance meets or exceeds current application
9. ✅ Application can scale horizontally
10. ✅ Monitoring and logging operational

---

## Next Steps

1. **Review this assessment** with stakeholders
2. **Approve migration plan** and timeline
3. **Set up development environment** and Azure resources
4. **Create feature branch** for migration work
5. **Begin Phase 1** - Project structure changes
6. **Establish regular checkpoints** for progress review

---

## Appendix

### Tools and Technologies Required

**Development:**
- Visual Studio 2022 or later / VS Code
- .NET 10 SDK
- Docker Desktop
- Azure CLI
- gRPC tooling

**Azure Services:**
- Azure Container Apps
- Azure Service Bus
- Azure Container Registry
- Azure Application Insights (optional)
- Azure Redis Cache (optional)

**Testing:**
- xUnit or NUnit
- Moq or NSubstitute
- Testcontainers (for integration tests)

### Reference Documentation

- [.NET 10 Migration Guide](https://learn.microsoft.com/en-us/dotnet/core/migration/)
- [ASP.NET Core MVC Overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview)
- [gRPC for .NET](https://learn.microsoft.com/en-us/aspnet/core/grpc/)
- [Azure Service Bus Documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [Azure Container Apps Documentation](https://learn.microsoft.com/en-us/azure/container-apps/)
- [WCF to gRPC Migration Guide](https://learn.microsoft.com/en-us/dotnet/architecture/grpc-for-wcf-developers/)

### Contact Information

For questions about this assessment, contact the modernization team.

---

*End of Assessment*
