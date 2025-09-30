using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace DotNetCoreAPITemplate.Configuration;

/// <summary>
/// Extensions for configuring comprehensive logging with Serilog
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog logging with comprehensive settings for different environments
    /// </summary>
    /// <param name="builder">The web application builder</param>
    /// <returns>The web application builder for chaining</returns>
    public static WebApplicationBuilder ConfigureSerilogLogging(this WebApplicationBuilder builder)
    {
        var environment = builder.Environment.EnvironmentName;
        var configuration = builder.Configuration;

        // Clear default logging providers
        builder.Logging.ClearProviders();

        // Configure Serilog
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithProperty("Application", "DotNetCoreAPITemplate")
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId();

        // Configure minimum level based on environment
        var minimumLevel = environment.ToLower() switch
        {
            "development" => LogEventLevel.Debug,
            "staging" => LogEventLevel.Information,
            "production" => LogEventLevel.Warning,
            _ => LogEventLevel.Information
        };

        loggerConfiguration.MinimumLevel.Is(minimumLevel);

        // Override specific namespaces
        loggerConfiguration
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning);

        // Console logging for all environments
        loggerConfiguration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // File logging configuration
        var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(logsPath);

        // General application logs
        loggerConfiguration.WriteTo.File(
            path: Path.Combine(logsPath, "app-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            restrictedToMinimumLevel: LogEventLevel.Information);

        // Error-only logs
        loggerConfiguration.WriteTo.File(
            path: Path.Combine(logsPath, "errors-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 90,
            restrictedToMinimumLevel: LogEventLevel.Error,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        // HTTP request logs
        loggerConfiguration.WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.WithProperty("RequestMethod"))
            .WriteTo.File(
                path: Path.Combine(logsPath, "http-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {RequestMethod} {RequestPath} {StatusCode} {Elapsed:000}ms {Properties:j}{NewLine}{Exception}"));

        // Performance logs
        loggerConfiguration.WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(Matching.WithProperty("Performance"))
            .WriteTo.File(
                path: Path.Combine(logsPath, "performance-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

        // Development-specific logging
        if (builder.Environment.IsDevelopment())
        {
            loggerConfiguration.MinimumLevel.Is(LogEventLevel.Debug);

            // Detailed EF Core logging in development
            loggerConfiguration
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Debug);

            // Debug logs in development
            loggerConfiguration.WriteTo.File(
                path: Path.Combine(logsPath, "debug-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 3,
                restrictedToMinimumLevel: LogEventLevel.Debug,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        }

        // Production-specific logging
        if (builder.Environment.IsProduction())
        {
            // Seq integration (if configured)
            var seqUrl = configuration["Serilog:WriteTo:0:Args:serverUrl"];
            var seqApiKey = configuration["Serilog:WriteTo:0:Args:apiKey"];

            if (!string.IsNullOrEmpty(seqUrl))
            {
                if (!string.IsNullOrEmpty(seqApiKey))
                {
                    loggerConfiguration.WriteTo.Seq(seqUrl, apiKey: seqApiKey);
                }
                else
                {
                    loggerConfiguration.WriteTo.Seq(seqUrl);
                }
            }

            // Compact file format for production
            loggerConfiguration.WriteTo.File(
                path: Path.Combine(logsPath, "app-compact-.json"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
                restrictedToMinimumLevel: LogEventLevel.Warning);
        }

        // Create and assign the logger
        var logger = loggerConfiguration.CreateLogger();
        Log.Logger = logger;

        // Use Serilog for ASP.NET Core
        builder.Host.UseSerilog(logger);

        return builder;
    }

    /// <summary>
    /// Adds request logging middleware with custom configuration
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            // Customize the message template
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            // Emit debug-level events instead of the defaults
            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : httpContext.Response.StatusCode > 399
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;

            // Attach additional properties to the request completion event
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
                diagnosticContext.Set("RequestPath", httpContext.Request.Path);
                diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
                }

                // Add correlation ID if available
                if (httpContext.Request.Headers.ContainsKey("X-Correlation-ID"))
                {
                    diagnosticContext.Set("CorrelationId", httpContext.Request.Headers["X-Correlation-ID"].ToString());
                }

                // Add custom timing information
                if (httpContext.Items.ContainsKey("RequestStartTime"))
                {
                    var startTime = (DateTime)httpContext.Items["RequestStartTime"]!;
                    var elapsed = DateTime.UtcNow - startTime;
                    diagnosticContext.Set("ElapsedMilliseconds", elapsed.TotalMilliseconds);
                }
            };
        });

        return app;
    }
}

/// <summary>
/// Custom middleware for tracking request performance
/// </summary>
public class PerformanceLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceLoggingMiddleware> _logger;
    private readonly long _slowRequestThreshold;

    /// <summary>
    /// Initializes a new instance of the PerformanceLoggingMiddleware
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">The logger instance</param>
    /// <param name="configuration">The configuration instance</param>
    public PerformanceLoggingMiddleware(
        RequestDelegate next,
        ILogger<PerformanceLoggingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _slowRequestThreshold = configuration.GetValue<long>("Logging:SlowRequestThresholdMs", 1000);
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">The HTTP context</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        context.Items["RequestStartTime"] = startTime;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;

            if (elapsed > _slowRequestThreshold)
            {
                _logger.LogWarning("Slow request detected: {RequestMethod} {RequestPath} took {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    elapsed);

                _logger.LogInformation("Slow Request: {RequestMethod} {RequestPath} {StatusCode} {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsed);
            }
        }
    }
}

/// <summary>
/// Extension method to add performance logging middleware
/// </summary>
public static class PerformanceLoggingMiddlewareExtensions
{
    /// <summary>
    /// Adds performance logging middleware to the pipeline
    /// </summary>
    /// <param name="builder">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UsePerformanceLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PerformanceLoggingMiddleware>();
    }
}