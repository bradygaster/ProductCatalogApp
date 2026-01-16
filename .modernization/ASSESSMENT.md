# Modernization Assessment Report

**Repository:** bradygaster/ProductCatalogApp  
**Assessment Date:** January 16, 2026  
**Target Framework:** .NET 10  
**Target Platform:** Azure Container Apps

---

## Executive Summary

This assessment evaluates the ProductCatalogApp for modernization from .NET Framework 4.8.1 to .NET 10 with deployment to Azure Container Apps. The application currently uses legacy technologies including ASP.NET MVC 5, WCF services, and MSMQ messaging that require significant updates for cloud-native deployment.

**Complexity:** High  
**Estimated Effort:** 4-6 weeks  
**Risk Level:** Medium

---

## Current Architecture

### Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | .NET Framework | 4.8.1 |
| Web Framework | ASP.NET MVC | 5.2.9 |
| Service Layer | WCF | Framework provided |
| Messaging | MSMQ (System.Messaging) | Framework provided |
| Project Format | Legacy .csproj | VS 2015+ |

### Application Structure

The application consists of two projects:

1. **ProductCatalog** - ASP.NET MVC web application
   - Product browsing and shopping cart functionality
   - WCF client for product service communication
   - MSMQ integration for order processing
   - Session-based cart storage

2. **ProductServiceLibrary** - WCF Service Library
   - Product CRUD operations
   - Category management
   - Price range queries
   - In-memory data repository

---

## Target Architecture

### Modernized Technology Stack

| Component | Technology | Version | Replaces |
|-----------|-----------|---------|----------|
| Framework | .NET | 10.0 | .NET Framework 4.8.1 |
| Web Framework | ASP.NET Core MVC | 10.0 | ASP.NET MVC 5 |
| Service Layer | gRPC | Latest | WCF |
| Messaging | Azure Service Bus | Latest | MSMQ |
| Project Format | SDK-style .csproj | .NET 10 | Legacy .csproj |
| Compute Platform | Azure Container Apps | - | IIS/Windows Server |
| Caching | Redis / Azure Cache | Latest | In-memory Session |

---

## Legacy Patterns Identified

### 1. WCF Service Contract ⚠️ HIGH PRIORITY

**Current Implementation:**
- `ProductServiceLibrary` uses WCF with ServiceContract and OperationContract attributes
- SOAP-based communication protocol
- Windows-specific technology

**Impact:**
- WCF is not supported in .NET Core/.NET 5+
- Cannot run on Linux containers
- Requires complete service rewrite

**Migration Path:**
- Design gRPC service contracts using `.proto` files
- Implement gRPC service endpoints
- Generate strongly-typed client stubs
- Update client to use gRPC channels

**Files Affected:**
- `ProductServiceLibrary/IProductService.cs`
- `ProductServiceLibrary/ProductService.cs`
- `ProductCatalog/Connected Services/ProductServiceReference/*`

---

### 2. MSMQ Message Queue ⚠️ HIGH PRIORITY

**Current Implementation:**
- `OrderQueueService` uses System.Messaging for MSMQ operations
- Local message queue for order processing
- Windows-specific technology

**Impact:**
- MSMQ not available on Linux/containers
- Not cloud-native
- Limited scalability

**Migration Path:**
- Provision Azure Service Bus namespace
- Replace MessageQueue with ServiceBusClient
- Update message serialization (JSON instead of XML)
- Implement queue/topic receivers

**Files Affected:**
- `ProductCatalog/Services/OrderQueueService.cs`
- `ProductCatalog/Controllers/HomeController.cs` (usage)

---

### 3. ASP.NET MVC (Non-Core) ⚠️ HIGH PRIORITY

**Current Implementation:**
- Legacy ASP.NET MVC 5 application
- System.Web dependencies
- IIS-hosted web application

**Impact:**
- Cannot run on .NET Core/.NET 5+
- Windows/IIS dependency
- Limited cloud optimization

**Migration Path:**
- Migrate to ASP.NET Core MVC
- Replace System.Web dependencies
- Update controller action signatures
- Modernize view rendering
- Implement Program.cs and dependency injection

**Files Affected:**
- `ProductCatalog/Controllers/HomeController.cs`
- `ProductCatalog/Global.asax.cs`
- `ProductCatalog/App_Start/*`
- All views and layout files

---

### 4. Session State Storage ⚠️ MEDIUM PRIORITY

**Current Implementation:**
- In-memory session state for shopping cart
- Session["Cart"] usage in controller

**Impact:**
- Not scalable across multiple instances
- Lost on application restart
- Not suitable for container environments

**Migration Path:**
- Implement distributed cache (Redis/Azure Cache for Redis)
- Use IDistributedCache interface
- Store cart data in distributed cache with session key
- Configure cache connection string

**Files Affected:**
- `ProductCatalog/Controllers/HomeController.cs`

---

### 5. Legacy Project Format ⚠️ MEDIUM PRIORITY

**Current Implementation:**
- Old-style .csproj files with explicit file references
- Package.config for NuGet packages
- Complex project structure

**Impact:**
- Verbose and hard to maintain
- Not compatible with .NET 5+
- Requires manual file tracking

**Migration Path:**
- Convert to SDK-style project format
- Use PackageReference instead of packages.config
- Remove unnecessary file references (files are included by default)
- Simplify project structure

**Files Affected:**
- `ProductCatalog/ProductCatalog.csproj`
- `ProductServiceLibrary/ProductServiceLibrary.csproj`
- `ProductCatalog/packages.config`

---

### 6. Configuration Management ⚠️ LOW PRIORITY

**Current Implementation:**
- XML-based Web.config and App.config
- AppSettings for configuration values

**Impact:**
- Verbose XML format
- Not environment-friendly
- Limited support for modern config sources

**Migration Path:**
- Migrate to appsettings.json
- Use environment variables for secrets
- Implement IConfiguration interface
- Consider Azure App Configuration for advanced scenarios

**Files Affected:**
- `ProductCatalog/Web.config`
- `ProductServiceLibrary/App.config`

---

## Migration Strategy

### Phase 1: Project Structure Modernization (Week 1)

**Objectives:**
- Convert to SDK-style projects
- Upgrade to .NET 10
- Update dependencies

**Tasks:**
1. Create new SDK-style .csproj files
2. Update target framework to net10.0
3. Migrate NuGet packages to PackageReference
4. Remove System.Web references
5. Test project compilation

**Deliverables:**
- Updated project files
- .NET 10 compatible solution

---

### Phase 2: Web Application Migration (Week 2)

**Objectives:**
- Migrate ASP.NET MVC to ASP.NET Core MVC
- Update web infrastructure

**Tasks:**
1. Create Program.cs with web application builder
2. Migrate controllers to ASP.NET Core patterns
3. Update views and layouts for Core
4. Replace bundling/minification tools
5. Implement distributed cache for session
6. Test web application locally

**Deliverables:**
- Functional ASP.NET Core MVC application
- Updated controllers and views

---

### Phase 3: Service Layer Migration (Week 3)

**Objectives:**
- Replace WCF with gRPC
- Implement modern service layer

**Tasks:**
1. Design .proto files for product service
2. Generate gRPC service and client code
3. Implement gRPC service logic
4. Update web app to use gRPC client
5. Remove WCF dependencies
6. Test service communication

**Deliverables:**
- gRPC service implementation
- gRPC client integration

---

### Phase 4: Messaging Migration (Week 4)

**Objectives:**
- Replace MSMQ with Azure Service Bus
- Implement cloud messaging

**Tasks:**
1. Provision Azure Service Bus namespace
2. Create queue for orders
3. Implement ServiceBus sender
4. Implement ServiceBus receiver (optional worker)
5. Update OrderQueueService to use Service Bus
6. Test message flow

**Deliverables:**
- Azure Service Bus integration
- Updated order processing

---

### Phase 5: Containerization (Week 5)

**Objectives:**
- Prepare for Azure Container Apps deployment
- Create container images

**Tasks:**
1. Create Dockerfile for web application
2. Create Dockerfile for gRPC service
3. Add .dockerignore files
4. Build and test containers locally
5. Push images to Azure Container Registry
6. Create docker-compose for local testing

**Deliverables:**
- Docker images for all components
- Local container testing environment

---

### Phase 6: Deployment & Testing (Week 6)

**Objectives:**
- Deploy to Azure Container Apps
- Validate functionality

**Tasks:**
1. Create Azure Container Apps environment
2. Deploy web application container
3. Deploy gRPC service container
4. Configure networking and ingress
5. Set up Application Insights
6. Integration and performance testing
7. Documentation updates

**Deliverables:**
- Production deployment on Azure Container Apps
- Monitoring and logging setup
- Updated documentation

---

## Risks and Mitigation

### Risk 1: WCF to gRPC Protocol Incompatibility
**Impact:** High  
**Probability:** Medium  
**Mitigation:** 
- Design gRPC contracts to match WCF operations
- Implement comprehensive integration tests
- Consider running both services in parallel during transition

### Risk 2: MSMQ Message Loss During Migration
**Impact:** High  
**Probability:** Low  
**Mitigation:**
- Implement message backup before migration
- Drain MSMQ queues before cutover
- Set up message reconciliation process

### Risk 3: Session State Migration Issues
**Impact:** Medium  
**Probability:** Medium  
**Mitigation:**
- Implement gradual rollout
- Add cart persistence warnings
- Provide cart recovery mechanisms

### Risk 4: Performance Degradation
**Impact:** Medium  
**Probability:** Low  
**Mitigation:**
- Conduct performance testing before deployment
- Implement caching strategies
- Configure auto-scaling in Container Apps

---

## Benefits of Modernization

### Technical Benefits
- ✅ Cross-platform support (Linux containers)
- ✅ Improved performance and scalability
- ✅ Modern language features (C# 13)
- ✅ Better security and regular updates
- ✅ Reduced infrastructure overhead

### Business Benefits
- ✅ Lower hosting costs with container-based pricing
- ✅ Faster deployment cycles
- ✅ Better reliability and uptime
- ✅ Easier to hire developers (modern stack)
- ✅ Future-proof technology foundation

### Operational Benefits
- ✅ Simplified deployment (containers)
- ✅ Auto-scaling capabilities
- ✅ Better monitoring and diagnostics
- ✅ Infrastructure as code
- ✅ DevOps friendly

---

## Recommendations

### Immediate Actions
1. ✅ Review and approve this assessment
2. ✅ Allocate development resources (1-2 developers)
3. ✅ Provision Azure resources (Service Bus, Container Registry)
4. ✅ Set up development environment with .NET 10 SDK

### Short-term Actions (1-2 weeks)
1. Begin Phase 1: Project structure modernization
2. Set up CI/CD pipeline for .NET 10
3. Create development branch for modernization work
4. Establish testing strategy

### Long-term Actions (2-6 weeks)
1. Execute remaining migration phases
2. Conduct comprehensive testing
3. Deploy to Azure Container Apps
4. Monitor and optimize performance
5. Update documentation and runbooks

---

## Cost Considerations

### Azure Service Bus
- **Standard Tier:** ~$10/month for basic messaging operations
- **Estimated:** $10-50/month depending on message volume

### Azure Container Apps
- **Consumption Plan:** Pay-per-use
- **Estimated:** $20-100/month depending on traffic
- Auto-scaling reduces costs during low traffic

### Azure Container Registry
- **Basic Tier:** ~$5/month
- Sufficient for small to medium deployments

### Azure Cache for Redis (if implemented)
- **Basic Tier:** ~$15/month for 250MB
- **Standard Tier:** ~$55/month for 1GB with redundancy

**Total Estimated Monthly Cost:** $50-220/month (vs. traditional Windows Server hosting)

---

## Success Criteria

- ✅ Application runs on .NET 10
- ✅ Deployed to Azure Container Apps
- ✅ gRPC service replaces WCF
- ✅ Azure Service Bus replaces MSMQ
- ✅ All functionality working as expected
- ✅ Performance meets or exceeds current system
- ✅ Zero data loss during migration
- ✅ Monitoring and logging operational

---

## Next Steps

1. **Review Assessment** - Share with stakeholders and get approval
2. **Plan Sprint** - Break down Phase 1 into actionable tasks
3. **Set Up Environment** - Install .NET 10 SDK and tools
4. **Begin Migration** - Start with project structure modernization
5. **Track Progress** - Use this document to monitor migration phases

---

## Appendix

### Key Files Requiring Changes

**Web Application:**
- `ProductCatalog/ProductCatalog.csproj` - Convert to SDK-style
- `ProductCatalog/Global.asax.cs` - Migrate to Program.cs
- `ProductCatalog/Controllers/HomeController.cs` - Update to Core patterns
- `ProductCatalog/Services/OrderQueueService.cs` - Replace MSMQ with Service Bus
- `ProductCatalog/Web.config` - Migrate to appsettings.json

**Service Library:**
- `ProductServiceLibrary/ProductServiceLibrary.csproj` - Convert to SDK-style
- `ProductServiceLibrary/IProductService.cs` - Replace with .proto definition
- `ProductServiceLibrary/ProductService.cs` - Implement gRPC service
- `ProductServiceLibrary/App.config` - Migrate to appsettings.json

**New Files Required:**
- `Dockerfile` (web application)
- `Dockerfile` (gRPC service)
- `.dockerignore`
- `docker-compose.yml` (for local testing)
- `Program.cs` (ASP.NET Core)
- `products.proto` (gRPC contract)
- `appsettings.json` (configuration)
- `appsettings.Development.json` (dev config)

### References

- [Migrating from ASP.NET MVC to ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core/migration/mvc)
- [WCF to gRPC Migration Guide](https://learn.microsoft.com/dotnet/architecture/grpc-for-wcf-developers/)
- [Azure Service Bus Documentation](https://learn.microsoft.com/azure/service-bus-messaging/)
- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [.NET 10 Documentation](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview)

---

**Assessment completed by:** GitHub Copilot  
**Version:** 1.0
