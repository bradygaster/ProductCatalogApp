# Build stage
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8.1 AS build
WORKDIR /src

# Copy csproj files first for layer caching
COPY ProductCatalog/ProductCatalog.csproj ProductCatalog/
COPY ProductServiceLibrary/ProductServiceLibrary.csproj ProductServiceLibrary/
COPY ProductCatalog/packages.config ProductCatalog/

# Restore NuGet packages
RUN nuget restore ProductCatalog/ProductCatalog.csproj -PackagesDirectory packages

# Copy remaining source code
COPY . .

# Build the web application
WORKDIR /src/ProductCatalog
RUN msbuild ProductCatalog.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:WebPublishMethod=FileSystem /p:PublishUrl=/app/out

# Runtime stage
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8.1 AS runtime
WORKDIR /inetpub/wwwroot

# Copy published application
COPY --from=build /app/out .

# Expose HTTP port
EXPOSE 80
EXPOSE 443
