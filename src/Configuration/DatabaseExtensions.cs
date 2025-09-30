using Microsoft.EntityFrameworkCore;
using DotNetCoreAPITemplate.Data;

namespace DotNetCoreAPITemplate.Configuration;

/// <summary>
/// Extensions for configuring database services and Entity Framework
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Adds Entity Framework and database services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get connection string from appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Add Entity Framework with SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);

                sqlOptions.CommandTimeout(60);
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            });

            // Enable sensitive data logging in development
            if (configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }

            // Configure query behavior
            options.ConfigureWarnings(warnings =>
            {
                warnings.Default(WarningBehavior.Log);
            });
        });

        // Add health checks for database
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                name: "database",
                tags: new[] { "db", "sql", "sqlserver" });

        return services;
    }

    /// <summary>
    /// Ensures database is created and migrations are applied
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="logger">Optional logger for migration operations</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder EnsureDatabaseCreated(
        this IApplicationBuilder app,
        ILogger? logger = null)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        try
        {
            logger?.LogInformation("Checking database connectivity...");

            // Test database connectivity
            var canConnect = context.Database.CanConnect();
            if (!canConnect)
            {
                logger?.LogWarning("Database connection test failed. Attempting to create database...");
                context.Database.EnsureCreated();
            }

            // Apply pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                logger?.LogInformation("Applying pending database migrations...");
                context.Database.Migrate();
                logger?.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                logger?.LogInformation("Database is up to date. No migrations needed.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while ensuring database is created and migrated.");

            // In production, you might want to throw the exception
            // In development, you might want to continue with warnings
            if (environment.IsProduction())
            {
                throw;
            }
            else
            {
                logger?.LogWarning("Continuing with potential database issues in development environment.");
            }
        }

        return app;
    }

    /// <summary>
    /// Seeds initial data into the database if it's empty
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="logger">Optional logger for seeding operations</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder SeedDatabase(
        this IApplicationBuilder app,
        ILogger? logger = null)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Check if database already has data
            if (context.SampleEntities.Any())
            {
                logger?.LogInformation("Database already contains sample data. Skipping seeding.");
                return app;
            }

            logger?.LogInformation("Seeding initial data into the database...");

            // Seed sample data
            var sampleEntities = new[]
            {
                new Models.SampleEntity
                {
                    Name = "Sample Item 1",
                    Description = "This is a sample item for testing purposes",
                    IsActive = true,
                    Value = 100.50m,
                    Type = Models.SampleEntityType.Standard,
                    Tags = new List<string> { "sample", "test", "demo" },
                    Metadata = new Dictionary<string, object>
                    {
                        { "category", "test-data" },
                        { "priority", 1 },
                        { "color", "blue" }
                    }
                },
                new Models.SampleEntity
                {
                    Name = "Sample Item 2",
                    Description = "Another sample item with different properties",
                    IsActive = true,
                    Value = 250.75m,
                    Type = Models.SampleEntityType.Premium,
                    Tags = new List<string> { "premium", "featured" },
                    Metadata = new Dictionary<string, object>
                    {
                        { "category", "premium-data" },
                        { "priority", 2 },
                        { "color", "gold" }
                    }
                },
                new Models.SampleEntity
                {
                    Name = "Sample Item 3",
                    Description = "Enterprise level sample item",
                    IsActive = false,
                    Value = 1000.00m,
                    Type = Models.SampleEntityType.Enterprise,
                    Tags = new List<string> { "enterprise", "advanced" },
                    Metadata = new Dictionary<string, object>
                    {
                        { "category", "enterprise-data" },
                        { "priority", 3 },
                        { "color", "platinum" }
                    }
                }
            };

            context.SampleEntities.AddRange(sampleEntities);
            context.SaveChanges();

            logger?.LogInformation("Successfully seeded {Count} sample entities into the database.", sampleEntities.Length);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error occurred while seeding the database.");
            throw;
        }

        return app;
    }
}