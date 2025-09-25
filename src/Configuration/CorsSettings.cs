namespace DotNetCoreAPITemplate.Configuration;

/// <summary>
/// Configuration settings for CORS (Cross-Origin Resource Sharing)
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// List of allowed origins for CORS requests
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether to allow credentials in CORS requests
    /// </summary>
    public bool AllowCredentials { get; set; } = false;

    /// <summary>
    /// List of allowed headers for CORS requests
    /// </summary>
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of allowed HTTP methods for CORS requests
    /// </summary>
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
}