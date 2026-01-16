# Docker Support for ProductCatalogApp

This document describes how to run the ProductCatalog application using Docker containers.

## Prerequisites

- Docker Desktop for Windows (Windows containers must be enabled)
- At least 8GB of RAM allocated to Docker
- Windows 10/11 or Windows Server 2019/2022

## Configuration

Before starting, set up your environment variables:

1. Copy `.env.example` to `.env`:
   ```powershell
   Copy-Item .env.example .env
   ```

2. **IMPORTANT:** Edit `.env` and set a strong password (required for anything other than quick local testing):
   ```
   SQL_SA_PASSWORD=YourVerySecurePassword123!
   ```

**Security Warning:** The default password `YourStrong@Passw0rd` is well-known and should NEVER be used in any environment accessible from a network. Always change it before use.

**Important:** Never commit the `.env` file to version control. It's already included in `.gitignore`.

## Quick Start

### Build and Run with Docker Compose

From the solution root directory, run:

```powershell
docker-compose up -d
```

This will:
1. Start a SQL Server 2022 container
2. Build and start the ProductCatalog web application
3. Set up networking between services

### Access the Application

Once running, access the application at:
- HTTP: http://localhost:8080
- HTTPS: https://localhost:44320

### Stop the Application

```powershell
docker-compose down
```

To also remove volumes:
```powershell
docker-compose down -v
```

## Building Individual Images

### ProductCatalog Web Application

```powershell
docker build -t productcatalog-web:latest -f ProductCatalog/Dockerfile .
```

### Running the Web Application Standalone

```powershell
docker run -d -p 8080:80 -p 44320:443 productcatalog-web:latest
```

## Configuration

### Environment Variables

Configure the application using environment variables in `docker-compose.override.yml`:

- `ConnectionStrings__DefaultConnection` - SQL Server connection string
- `OrderQueuePath` - MSMQ queue path for order processing
- `ASPNETCORE_ENVIRONMENT` - Environment name (Development, Production, etc.)

### Database Connection

The SQL Server configuration uses environment variables:
- **Server**: sqlserver (container name)
- **Port**: 1433
- **Database**: ProductCatalog
- **User**: sa
- **Password**: Set via `SQL_SA_PASSWORD` environment variable (defaults to `YourStrong@Passw0rd`)

To use a custom password:
1. Create a `.env` file from `.env.example`
2. Set `SQL_SA_PASSWORD=YourCustomPassword`
3. Run `docker-compose up`

**Security Note:** Always use strong passwords and never commit the `.env` file to version control.

## Architecture

The Docker setup includes:

1. **ProductCatalog Web App** (.NET Framework 4.8.1)
   - Multi-stage build using SDK and ASP.NET runtime images
   - Runs on IIS in Windows container
   - Exposed on ports 80 (HTTP) and 443 (HTTPS)

2. **SQL Server 2022** (Linux container)
   - Persistent storage using Docker volumes
   - Health checks for startup synchronization

3. **Network**
   - Bridge network for inter-container communication
   - Containers can reference each other by service name

## Important Notes

### Windows Containers Required

This application uses .NET Framework 4.8.1, which requires Windows containers. Make sure Docker Desktop is configured to use Windows containers:

```powershell
# Switch to Windows containers
& "C:\Program Files\Docker\Docker\DockerCli.exe" -SwitchDaemon
```

### Resource Requirements

Windows containers require significant resources:
- Minimum 8GB RAM recommended
- 20GB+ disk space for base images

### Development vs Production

The included `docker-compose.override.yml` is for local development only. For production:
1. Remove or replace the override file
2. Use Docker secrets or Azure Key Vault for password management
3. Configure proper SSL certificates
4. Set appropriate resource limits
5. Use external database services when possible
6. Never pass passwords directly in environment variables that may be logged

**Production Security:**
```yaml
# Example using Docker secrets (Docker Swarm or Kubernetes)
secrets:
  - sql_sa_password
  
# Reference in service:
environment:
  - SA_PASSWORD_FILE=/run/secrets/sql_sa_password
```

## Troubleshooting

### Container fails to start

Check Docker logs:
```powershell
docker-compose logs productcatalog-web
```

### Cannot connect to SQL Server

Verify SQL Server is healthy:
```powershell
docker-compose ps
docker-compose logs sqlserver
```

### Port conflicts

If ports 8080, 44320, or 1433 are already in use, modify the port mappings in `docker-compose.override.yml`.

## Next Steps

After the application is modernized to .NET 8.0, the Dockerfiles can be updated to use:
- `mcr.microsoft.com/dotnet/sdk:8.0` (build stage)
- `mcr.microsoft.com/dotnet/aspnet:8.0` (runtime stage)
- Linux containers for better performance and resource usage
