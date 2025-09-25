using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;

namespace DotNetCoreAPITemplate.Controllers;

/// <summary>
/// Health check controller providing application health and readiness endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Health check endpoints for monitoring application status")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    /// <summary>
    /// Initializes a new instance of the HealthController
    /// </summary>
    /// <param name="healthCheckService">The health check service</param>
    /// <param name="logger">The logger instance</param>
    public HealthController(
        HealthCheckService healthCheckService,
        ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the overall health status of the application
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The health status</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get application health status",
        Description = "Returns the overall health status of the application including all dependencies."
    )]
    [SwaggerResponse(200, "Application is healthy", typeof(HealthStatusResponse))]
    [SwaggerResponse(503, "Application is unhealthy", typeof(HealthStatusResponse))]
    public async Task<ActionResult<HealthStatusResponse>> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Performing health check");

        var healthReport = await _healthCheckService.CheckHealthAsync(cancellationToken);

        var response = new HealthStatusResponse
        {
            Status = healthReport.Status.ToString(),
            TotalDuration = healthReport.TotalDuration,
            Checks = healthReport.Entries.Select(e => new HealthCheckResult
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Duration = e.Value.Duration,
                Description = e.Value.Description,
                Exception = e.Value.Exception?.Message,
                Data = e.Value.Data.Count > 0 ? e.Value.Data : null
            }).ToList()
        };

        _logger.LogInformation("Health check completed with status: {Status}", healthReport.Status);

        var statusCode = healthReport.Status == HealthStatus.Healthy ? 200 : 503;
        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Gets the readiness status of the application (lightweight check)
    /// </summary>
    /// <returns>The readiness status</returns>
    [HttpGet("ready")]
    [SwaggerOperation(
        Summary = "Get application readiness status",
        Description = "Returns a lightweight readiness check to determine if the application is ready to serve requests."
    )]
    [SwaggerResponse(200, "Application is ready")]
    [SwaggerResponse(503, "Application is not ready")]
    public IActionResult GetReadiness()
    {
        _logger.LogDebug("Performing readiness check");

        try
        {
            // Perform lightweight checks here
            // For example: check if configuration is loaded, essential services are available, etc.

            var response = new
            {
                status = "Ready",
                timestamp = DateTime.UtcNow,
                uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime
            };

            _logger.LogDebug("Readiness check passed");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");
            return StatusCode(503, new { status = "Not Ready", error = ex.Message });
        }
    }

    /// <summary>
    /// Gets the liveness status of the application (basic check)
    /// </summary>
    /// <returns>The liveness status</returns>
    [HttpGet("live")]
    [SwaggerOperation(
        Summary = "Get application liveness status",
        Description = "Returns a basic liveness check to determine if the application is running."
    )]
    [SwaggerResponse(200, "Application is alive")]
    public IActionResult GetLiveness()
    {
        _logger.LogDebug("Performing liveness check");

        var response = new
        {
            status = "Alive",
            timestamp = DateTime.UtcNow,
            version = GetApplicationVersion(),
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        };

        _logger.LogDebug("Liveness check completed");
        return Ok(response);
    }

    /// <summary>
    /// Gets application version information
    /// </summary>
    /// <returns>The application version</returns>
    [HttpGet("version")]
    [SwaggerOperation(
        Summary = "Get application version information",
        Description = "Returns detailed version information about the application."
    )]
    [SwaggerResponse(200, "Version information retrieved successfully", typeof(VersionResponse))]
    public ActionResult<VersionResponse> GetVersion()
    {
        _logger.LogDebug("Getting version information");

        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var buildTime = GetBuildTime(assembly);

        var response = new VersionResponse
        {
            Version = version?.ToString() ?? "Unknown",
            InformationalVersion = informationalVersion ?? "Unknown",
            BuildTime = buildTime,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            RuntimeVersion = Environment.Version.ToString(),
            MachineName = Environment.MachineName,
            ProcessId = Environment.ProcessId,
            WorkingDirectory = Environment.CurrentDirectory
        };

        _logger.LogDebug("Version information retrieved");
        return Ok(response);
    }

    /// <summary>
    /// Gets the application version string
    /// </summary>
    /// <returns>The application version</returns>
    private static string GetApplicationVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetName().Version?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Gets the build time of the assembly
    /// </summary>
    /// <param name="assembly">The assembly to check</param>
    /// <returns>The build time</returns>
    private static DateTime? GetBuildTime(Assembly assembly)
    {
        try
        {
            var buildAttribute = assembly.GetCustomAttribute<AssemblyMetadataAttribute>();
            if (buildAttribute != null && DateTime.TryParse(buildAttribute.Value, out var buildTime))
            {
                return buildTime;
            }

            // Fallback to file creation time
            var location = assembly.Location;
            if (!string.IsNullOrEmpty(location) && File.Exists(location))
            {
                return File.GetCreationTime(location);
            }
        }
        catch
        {
            // Ignore exceptions and return null
        }

        return null;
    }
}

/// <summary>
/// Response model for health status
/// </summary>
public class HealthStatusResponse
{
    /// <summary>
    /// Overall health status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Total duration of all health checks
    /// </summary>
    public TimeSpan TotalDuration { get; set; }

    /// <summary>
    /// Individual health check results
    /// </summary>
    public List<HealthCheckResult> Checks { get; set; } = new();
}

/// <summary>
/// Individual health check result
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Name of the health check
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Status of the health check
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the health check
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Description of the health check
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Exception message if the check failed
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Additional data from the health check
    /// </summary>
    public object? Data { get; set; }
}

/// <summary>
/// Response model for version information
/// </summary>
public class VersionResponse
{
    /// <summary>
    /// Application version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Informational version (includes Git commit, etc.)
    /// </summary>
    public string InformationalVersion { get; set; } = string.Empty;

    /// <summary>
    /// Build timestamp
    /// </summary>
    public DateTime? BuildTime { get; set; }

    /// <summary>
    /// Current environment
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// .NET runtime version
    /// </summary>
    public string RuntimeVersion { get; set; } = string.Empty;

    /// <summary>
    /// Machine name
    /// </summary>
    public string MachineName { get; set; } = string.Empty;

    /// <summary>
    /// Process ID
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// Current working directory
    /// </summary>
    public string WorkingDirectory { get; set; } = string.Empty;
}