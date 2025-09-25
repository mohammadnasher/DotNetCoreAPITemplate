namespace DotNetCoreAPITemplate.Configuration;

/// <summary>
/// Configuration settings for API documentation and metadata
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// The title of the API
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The version of the API
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The description of the API
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Contact information for the API
    /// </summary>
    public ContactInfo Contact { get; set; } = new();
}

/// <summary>
/// Contact information for the API
/// </summary>
public class ContactInfo
{
    /// <summary>
    /// The name of the contact person/organization
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The email address of the contact
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The URL of the contact's website
    /// </summary>
    public string Url { get; set; } = string.Empty;
}