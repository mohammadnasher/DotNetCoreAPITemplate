using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCoreAPITemplate.Configuration;

/// <summary>
/// Extension methods for configuring environment variables and configuration settings
/// </summary>
public static class EnvironmentVariableExtensions
{
    /// <summary>
    /// Adds and configures all application settings from configuration and environment variables
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplicationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure strongly-typed settings
        services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));
        services.Configure<CorsSettings>(configuration.GetSection("CorsSettings"));
        services.Configure<HealthCheckSettings>(configuration.GetSection("HealthChecks"));

        // Validate required configuration on startup
        services.AddOptions<ApiSettings>()
            .Bind(configuration.GetSection("ApiSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<CorsSettings>()
            .Bind(configuration.GetSection("CorsSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Gets a connection string with environment variable override capability
    /// Priority: Environment Variable > Configuration > Default
    /// </summary>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="name">The connection string name</param>
    /// <param name="environmentVariableName">Optional environment variable name override</param>
    /// <returns>The connection string</returns>
    public static string GetConnectionStringWithEnvironmentOverride(
        this IConfiguration configuration,
        string name,
        string? environmentVariableName = null)
    {
        // Try environment variable first
        var envVarName = environmentVariableName ?? $"ConnectionStrings__{name}";
        var connectionString = Environment.GetEnvironmentVariable(envVarName);

        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Fall back to configuration
        connectionString = configuration.GetConnectionString(name);

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException($"Connection string '{name}' not found in configuration or environment variables.");
        }

        return connectionString;
    }

    /// <summary>
    /// Gets a configuration value with environment variable override capability
    /// </summary>
    /// <param name="configuration">The configuration instance</param>
    /// <param name="key">The configuration key</param>
    /// <param name="defaultValue">Default value if not found</param>
    /// <returns>The configuration value</returns>
    public static string GetValueWithEnvironmentOverride(
        this IConfiguration configuration,
        string key,
        string defaultValue = "")
    {
        // Try environment variable first (replace : with __)
        var envVarName = key.Replace(":", "__");
        var value = Environment.GetEnvironmentVariable(envVarName);

        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Fall back to configuration
        return configuration[key] ?? defaultValue;
    }
}