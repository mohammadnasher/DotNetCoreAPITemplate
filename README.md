# 🚀 .NET Core API Template

A comprehensive, production-ready .NET 9 Web API template with industry best practices, designed to accelerate your development process and provide a solid foundation for scalable applications.

[![CI](https://github.com/yourorg/dotnet-core-api-template/actions/workflows/ci.yml/badge.svg)](https://github.com/yourorg/dotnet-core-api-template/actions/workflows/ci.yml)
[![CD](https://github.com/yourorg/dotnet-core-api-template/actions/workflows/cd.yml/badge.svg)](https://github.com/yourorg/dotnet-core-api-template/actions/workflows/cd.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)

## ✨ Features

### 🏗️ **Architecture & Structure**
- **Clean Architecture** with organized project structure
- **Service Layer Pattern** for business logic separation
- **Repository Pattern** ready for data access abstraction
- **Dependency Injection** with built-in .NET DI container
- **Entity Framework Core 9** with SQL Server support

### 📋 **API Features**
- **Comprehensive CRUD Operations** with sample entities
- **API Versioning** with multiple versioning strategies
- **Swagger/OpenAPI 3.0** documentation with XML comments
- **Request/Response Validation** with Data Annotations
- **Standardized Error Handling** with proper HTTP status codes
- **Health Checks** for monitoring and diagnostics

### 🔧 **Configuration & Environment**
- **Environment Variable Support** with strongly-typed configuration
- **Multiple Environment Configurations** (Development, Staging, Production)
- **CORS Configuration** for cross-origin requests
- **Flexible Connection String Management**
- **Comprehensive .env.example** with all settings documented

### 📊 **Logging & Monitoring**
- **Serilog Integration** with structured logging
- **Multiple Log Sinks** (Console, File, Seq support)
- **Performance Logging** with slow request detection
- **Request/Response Logging** with customizable enrichment
- **Environment-Specific Log Levels**
- **Log File Rotation** and retention policies

### 🗃️ **Database**
- **Entity Framework Core 9** with Code-First approach
- **Database Migrations** with comprehensive scripts
- **Connection Retry Policies** for resilience
- **Database Health Checks**
- **Automatic Database Creation** and seeding
- **Cross-platform Database Scripts** (Unix & Windows)

### 🐳 **Docker & Deployment**
- **Multi-stage Docker Build** for optimized images
- **Docker Compose** configurations for development and production
- **Security Best Practices** (non-root user, minimal base image)
- **Health Check Integration**
- **Multi-platform Support** (linux/amd64, linux/arm64)

### 🚀 **CI/CD & DevOps**
- **GitHub Actions Workflows** for CI/CD
- **Automated Testing** and code quality checks
- **Security Vulnerability Scanning**
- **Automated Dependency Updates**
- **Docker Image Building** and registry push
- **Automated Release Management**

### 🔒 **Security**
- **Security Headers** configuration ready
- **CORS Policy** management
- **Dependency Vulnerability Monitoring**
- **Secrets Management** best practices
- **Docker Security** with non-root execution

## 🚀 Quick Start

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)
- [Git](https://git-scm.com/downloads)

### 1. Clone and Setup
```bash
# Clone the repository
git clone https://github.com/yourorg/dotnet-core-api-template.git
cd dotnet-core-api-template

# Copy environment configuration
cp .env.example .env

# Restore dependencies
dotnet restore
```

### 2. Database Setup
```bash
# Install EF Tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create and migrate database
./scripts/db-migrate.sh add InitialCreate
./scripts/db-migrate.sh update

# Or on Windows
scripts\db-migrate.bat add InitialCreate
scripts\db-migrate.bat update
```

### 3. Run the Application
```bash
# Run locally
dotnet run

# Or with hot reload
dotnet watch run

# API will be available at:
# - HTTP: http://localhost:5000
# - HTTPS: https://localhost:5001
# - Swagger: https://localhost:5001/api-docs
```

### 4. Docker Setup (Optional)
```bash
# Build and run with Docker Compose
docker-compose up --build

# API will be available at:
# - HTTP: http://localhost:8080
# - Swagger: http://localhost:8080/api-docs
```

## 📖 API Documentation

### Swagger/OpenAPI
Once the application is running, access the interactive API documentation:
- **Development**: https://localhost:5001/api-docs
- **Docker**: http://localhost:8080/api-docs

### Available Endpoints

#### Health Checks
- `GET /health` - Overall application health
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe
- `GET /api/health/version` - Application version info

#### Sample CRUD API
- `GET /api/v1/test` - Get all sample entities (with pagination)
- `GET /api/v1/test/{id}` - Get sample entity by ID
- `POST /api/v1/test` - Create new sample entity
- `PUT /api/v1/test/{id}` - Update sample entity
- `DELETE /api/v1/test/{id}` - Delete sample entity
- `HEAD /api/v1/test/{id}` - Check if sample entity exists

#### API Versioning
The API supports multiple versioning strategies:
- **URL Path**: `/api/v1/test` or `/api/v2/test`
- **Query String**: `/api/test?version=1.0` or `/api/test?v=2.0`
- **Header**: `X-Version: 1.0` or `X-API-Version: 2.0`

## 🏗️ Project Structure

```
├── src/
│   ├── Configuration/     # Application configuration and extensions
│   ├── Controllers/       # API controllers
│   ├── Data/             # Database context and migrations
│   ├── Models/           # Domain models and DTOs
│   │   └── DTOs/         # Data Transfer Objects
│   └── Services/         # Business logic services
├── scripts/              # Build and database scripts
│   ├── build.sh          # Unix build script
│   ├── build.bat         # Windows build script
│   ├── db-migrate.sh     # Unix database migration script
│   └── db-migrate.bat    # Windows database migration script
├── .github/
│   └── workflows/        # GitHub Actions workflows
├── docs/                 # Project documentation
├── logs/                 # Application logs (created at runtime)
├── Dockerfile            # Docker configuration
├── docker-compose.yml    # Development Docker Compose
├── docker-compose.prod.yml # Production Docker Compose
├── .env.example          # Environment variables template
├── version.txt           # Current version
└── CHANGELOG.md          # Version history
```

## ⚙️ Configuration

### Environment Variables
All configuration can be overridden using environment variables. See [`.env.example`](.env.example) for a complete list of available settings.

#### Key Environment Variables:
```bash
# Database
ConnectionStrings__DefaultConnection="Server=...;Database=...;"

# API Settings
ApiSettings__Title="Your API Name"
ApiSettings__Version="v1"

# CORS
CorsSettings__AllowedOrigins__0="https://localhost:3000"
CorsSettings__AllowCredentials=true

# Logging
Serilog__MinimumLevel__Default=Information
```

### Configuration Files
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides

## 🗄️ Database

### Entity Framework Core
The template uses Entity Framework Core with the following features:
- **Code-First Migrations**
- **Automatic Timestamp Management**
- **Base Entity Classes**
- **Database Health Checks**
- **Connection Resilience**

### Migration Commands
```bash
# Add new migration
./scripts/db-migrate.sh add MigrationName

# Update database
./scripts/db-migrate.sh update

# View migration status
./scripts/db-migrate.sh status

# Reset database (development only)
./scripts/db-migrate.sh reset
```

### Sample Entity
The template includes a comprehensive sample entity demonstrating:
- Primary keys and indexes
- Data annotations and validation
- Different data types (strings, decimals, dates, enums)
- JSON columns for metadata
- Audit fields (CreatedAt, UpdatedAt)

## 🔨 Build Scripts

### Unix/Linux/macOS
```bash
# Build application
./scripts/build.sh build

# Run tests
./scripts/build.sh test

# Create release with version tagging
./scripts/build.sh release -v 1.2.3 -t -p

# Build Docker image
./scripts/build.sh docker-build -v 1.2.3

# Full help
./scripts/build.sh --help
```

### Windows
```batch
# Build application
scripts\build.bat build

# Run tests
scripts\build.bat test

# Create release with version tagging
scripts\build.bat release -v 1.2.3 -t -p

# Build Docker image
scripts\build.bat docker-build -v 1.2.3

# Full help
scripts\build.bat --help
```

## 🐳 Docker

### Development
```bash
# Run with development configuration
docker-compose up --build

# Run with additional services (logging, cache)
docker-compose --profile logging --profile cache up -d
```

### Production
```bash
# Set environment variables
export TAG=v1.0.0
export DATABASE_CONNECTION_STRING="..."
export FRONTEND_URL="https://yourapp.com"

# Deploy production stack
docker-compose -f docker-compose.prod.yml up -d

# With monitoring
docker-compose -f docker-compose.prod.yml --profile monitoring up -d
```

### Docker Image Features
- **Multi-stage builds** for optimized size
- **Security best practices** (non-root user)
- **Health checks** integration
- **Multi-platform support** (AMD64, ARM64)
- **Proper logging** configuration for containers

## 🚀 CI/CD

### GitHub Actions Workflows

#### Continuous Integration (`ci.yml`)
Triggers on: Push to main/develop, Pull Requests
- ✅ Build and test (Debug & Release)
- ✅ Code quality analysis (SonarCloud)
- ✅ Security vulnerability scanning
- ✅ Package audit for vulnerable dependencies
- ✅ Generate build summaries

#### Continuous Deployment (`cd.yml`)
Triggers on: Git tags (`v*.*.*`), Manual dispatch
- 🚀 Build and test
- 🐳 Build multi-platform Docker images
- 📦 Create GitHub releases with artifacts
- 🔄 Optional staging deployment
- 📋 Generate deployment summaries

#### Dependency Updates (`dependency-update.yml`)
Triggers on: Weekly schedule, Manual dispatch
- 📦 Check for outdated packages
- ⚠️ Identify vulnerable packages
- 🎫 Create GitHub issues for tracking
- 📊 Generate dependency reports

### Required Secrets
Configure these in your GitHub repository settings:
```
SONAR_TOKEN          # SonarCloud integration
SNYK_TOKEN           # Snyk security scanning
CODECOV_TOKEN        # Code coverage reporting
DOCKER_REGISTRY_URL  # Docker registry (optional)
```

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=Integration
```

### Test Structure
```
├── tests/
│   ├── DotNetCoreAPITemplate.UnitTests/     # Unit tests
│   ├── DotNetCoreAPITemplate.IntegrationTests/ # Integration tests
│   └── DotNetCoreAPITemplate.EndToEndTests/    # E2E tests
```

## 📊 Logging

### Log Files
- `logs/app-*.log` - General application logs
- `logs/errors-*.log` - Error-only logs
- `logs/http-*.log` - HTTP request/response logs
- `logs/performance-*.log` - Performance monitoring logs
- `logs/debug-*.log` - Debug logs (development only)

### Log Levels
- **Production**: Warning and above
- **Staging**: Information and above
- **Development**: Debug and above

### Structured Logging Examples
```csharp
// Basic logging
_logger.LogInformation("User {UserId} created entity {EntityId}", userId, entityId);

// Performance logging
using (_logger.BeginScope("Operation: {Operation}", "CreateUser"))
{
    // Your code here
}

// Error logging with exception
try
{
    // Risky operation
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to process request for user {UserId}", userId);
}
```

## 🛡️ Security

### Implemented Security Features
- **HTTPS Enforcement** in production
- **CORS Policy** configuration
- **Security Headers** (HSTS, etc.)
- **Input Validation** with Data Annotations
- **SQL Injection Protection** via Entity Framework
- **Dependency Vulnerability Monitoring**
- **Docker Security** (non-root execution)

### Security Best Practices
1. **Never commit secrets** - Use environment variables
2. **Regular dependency updates** - Automated weekly checks
3. **Input validation** - Validate all API inputs
4. **Logging security** - Don't log sensitive data
5. **HTTPS only** - Enforce in production

## 🔧 Development

### Prerequisites for Development
- Visual Studio 2022+ or Visual Studio Code
- .NET 9 SDK
- SQL Server LocalDB (Windows) or SQL Server in Docker
- Git

### Development Workflow
1. **Create feature branch** from `main`
2. **Make changes** following existing patterns
3. **Add tests** for new functionality
4. **Run local tests** and ensure they pass
5. **Update documentation** if needed
6. **Create pull request** with descriptive title
7. **CI pipeline** will validate your changes

### Code Style
- Follow **C# coding conventions**
- Use **XML documentation** for public APIs
- Add **comprehensive logging** for debugging
- Write **unit tests** for business logic
- Keep **controllers thin** - business logic in services

## 📚 API Usage Examples

### Creating a Sample Entity
```bash
curl -X POST "https://localhost:5001/api/v1/test" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Sample Item",
    "description": "This is a test item",
    "isActive": true,
    "value": 99.99,
    "type": "Premium",
    "tags": ["example", "test"],
    "metadata": {
      "category": "demo",
      "priority": 1
    }
  }'
```

### Getting Paginated Results
```bash
curl "https://localhost:5001/api/v1/test?pageNumber=1&pageSize=10&search=sample&isActive=true"
```

### Health Check
```bash
curl "https://localhost:5001/health"
```

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Commit Message Convention
We follow [Conventional Commits](https://www.conventionalcommits.org/):
- `feat:` New features
- `fix:` Bug fixes
- `docs:` Documentation updates
- `style:` Code style changes
- `refactor:` Code refactoring
- `test:` Test additions or updates
- `chore:` Maintenance tasks

## 📋 Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [ASP.NET Core Team](https://github.com/dotnet/aspnetcore) for the excellent framework
- [Serilog](https://serilog.net/) for structured logging
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) for API documentation
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for data access

## 📞 Support

- 📖 [Documentation](docs/)
- 🐛 [Issue Tracker](https://github.com/yourorg/dotnet-core-api-template/issues)
- 💬 [Discussions](https://github.com/yourorg/dotnet-core-api-template/discussions)
- 📧 Email: support@yourcompany.com

---

**Made with ❤️ by [Your Company](https://yourcompany.com)**

> This template provides a solid foundation for building production-ready .NET APIs. Feel free to customize it according to your specific requirements and organizational standards.
