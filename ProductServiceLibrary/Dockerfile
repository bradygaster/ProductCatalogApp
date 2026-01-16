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

# Expose WCF service port
EXPOSE 8733

# Note: This service library requires a host application to run.
# This Dockerfile is provided for completeness, but the service is typically
# consumed by the ProductCatalog web application.
