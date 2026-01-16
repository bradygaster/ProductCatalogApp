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

# Runtime stage (non-functional - for reference only)
# Note: WCF services require a host application and cannot run standalone
FROM mcr.microsoft.com/dotnet/framework/runtime:4.8.1 AS runtime
WORKDIR /app

# Copy built service library
COPY --from=build /app/out .

# IMPORTANT: This runtime stage is for demonstration purposes only.
# WCF service libraries require a host application to run (IIS, Windows Service, or console host).
# This Dockerfile can be used for:
# 1. Build verification in CI/CD pipelines
# 2. Creating a base layer for a custom host application
# 3. Reference implementation for hosting strategies
#
# The ProductCatalog web application consumes this service library.
