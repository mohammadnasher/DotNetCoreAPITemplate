# =================================================================
# Multi-stage Docker build for .NET 9 Web API
# =================================================================
# This Dockerfile creates an optimized production image with security best practices
# Stages: restore, build, publish, final

# =================================================================
# Stage 1: Base image for building
# =================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj file and restore dependencies
# This layer is cached unless dependencies change
COPY ["DotNetCoreAPITemplate.csproj", "./"]
RUN dotnet restore "DotNetCoreAPITemplate.csproj"

# Copy source code and build application
COPY . .
RUN dotnet build "DotNetCoreAPITemplate.csproj" -c Release -o /app/build --no-restore

# =================================================================
# Stage 2: Publish the application
# =================================================================
FROM build AS publish
RUN dotnet publish "DotNetCoreAPITemplate.csproj" -c Release -o /app/publish --no-build --no-restore

# =================================================================
# Stage 3: Final runtime image
# =================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Create non-root user for security
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Set working directory
WORKDIR /app

# Install curl for health checks (if needed)
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application from publish stage
COPY --from=publish /app/publish .

# Create logs directory and set permissions
RUN mkdir -p logs && chown -R appuser:appuser logs

# Set up environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Create appsettings.Production.json with container-specific settings
RUN echo '{ \
  "Logging": { \
    "LogLevel": { \
      "Default": "Information", \
      "Microsoft.AspNetCore": "Warning", \
      "Microsoft.EntityFrameworkCore": "Warning" \
    } \
  }, \
  "ConnectionStrings": { \
    "DefaultConnection": "Server=sqlserver;Database=DotNetCoreAPITemplateDb;User Id=sa;Password=${DB_PASSWORD};TrustServerCertificate=true;MultipleActiveResultSets=true" \
  }, \
  "AllowedHosts": "*", \
  "CorsSettings": { \
    "AllowedOrigins": ["*"], \
    "AllowCredentials": false \
  } \
}' > appsettings.Production.json

# Expose port
EXPOSE 8080

# Switch to non-root user
USER appuser

# Add health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

# Set entry point
ENTRYPOINT ["dotnet", "DotNetCoreAPITemplate.dll"]

# Labels for metadata
LABEL maintainer="your-email@company.com"
LABEL version="1.0.0"
LABEL description="DotNet Core API Template - Production Ready Web API"
LABEL org.opencontainers.image.source="https://github.com/yourorg/dotnet-core-api-template"
LABEL org.opencontainers.image.documentation="https://github.com/yourorg/dotnet-core-api-template/README.md"
LABEL org.opencontainers.image.licenses="MIT"