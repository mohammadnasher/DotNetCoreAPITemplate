namespace DotNetCoreAPITemplate.Configuration;

/// <summary>
/// Configuration settings for health checks
/// </summary>
public class HealthCheckSettings
{
    /// <summary>
    /// Whether health checks are enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether database health check is enabled
    /// </summary>
    public bool DatabaseCheckEnabled { get; set; } = true;

    /// <summary>
    /// Timeout for health checks in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Health check endpoint path
    /// </summary>
    public string Endpoint { get; set; } = "/health";

    /// <summary>
    /// Ready check endpoint path
    /// </summary>
    public string ReadyEndpoint { get; set; } = "/ready";

    /// <summary>
    /// Live check endpoint path
    /// </summary>
    public string LiveEndpoint { get; set; } = "/live";
}