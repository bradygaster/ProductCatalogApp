# Build stage
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8.1 AS build
WORKDIR /src

# Copy csproj files first for layer caching
COPY ProductServiceLibrary/ProductServiceLibrary.csproj ProductServiceLibrary/

# Restore NuGet packages
RUN nuget restore ProductServiceLibrary/ProductServiceLibrary.csproj

# Copy remaining source code
COPY ProductServiceLibrary/ ProductServiceLibrary/

# Build the service library
WORKDIR /src/ProductServiceLibrary
RUN msbuild ProductServiceLibrary.csproj /p:Configuration=Release /p:OutputPath=/app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/framework/runtime:4.8.1 AS runtime
WORKDIR /app

# Copy built service library
COPY --from=build /app/out .

# Expose WCF service port (when self-hosted)
EXPOSE 8733

# IMPORTANT: This is a WCF service library that requires a host application to run.
# This Dockerfile is provided for reference and build verification only.
# The service is consumed by the ProductCatalog web application and does not
# run independently. To use this service, it must be hosted in IIS, a Windows Service,
# or a console application host.
