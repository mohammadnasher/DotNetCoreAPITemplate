using System.ComponentModel.DataAnnotations;

namespace DotNetCoreAPITemplate.Models.DTOs;

/// <summary>
/// Data Transfer Object for SampleEntity responses
/// </summary>
public class SampleEntityDto
{
    /// <summary>
    /// The unique identifier for the entity
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the sample entity
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// An optional description of the sample entity
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the entity is active or not
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// An optional numeric value for demonstration purposes
    /// </summary>
    public decimal? Value { get; set; }

    /// <summary>
    /// Category or type classification
    /// </summary>
    public SampleEntityType Type { get; set; }

    /// <summary>
    /// Optional tags associated with this entity
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Optional metadata in JSON format
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// The date and time the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Data Transfer Object for creating a new SampleEntity
/// </summary>
public class CreateSampleEntityDto
{
    /// <summary>
    /// The name of the sample entity (required, max 100 characters)
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// An optional description of the sample entity (max 500 characters)
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the entity is active or not (default: true)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// An optional numeric value for demonstration purposes
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Value must be non-negative.")]
    public decimal? Value { get; set; }

    /// <summary>
    /// Category or type classification (default: Standard)
    /// </summary>
    public SampleEntityType Type { get; set; } = SampleEntityType.Standard;

    /// <summary>
    /// Optional tags associated with this entity
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Optional metadata in JSON format
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Data Transfer Object for updating an existing SampleEntity
/// </summary>
public class UpdateSampleEntityDto
{
    /// <summary>
    /// The name of the sample entity (required, max 100 characters)
    /// </summary>
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// An optional description of the sample entity (max 500 characters)
    /// </summary>
    [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the entity is active or not
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// An optional numeric value for demonstration purposes
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Value must be non-negative.")]
    public decimal? Value { get; set; }

    /// <summary>
    /// Category or type classification
    /// </summary>
    public SampleEntityType Type { get; set; }

    /// <summary>
    /// Optional tags associated with this entity
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Optional metadata in JSON format
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Data Transfer Object for paginated results
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public class PaginatedResultDto<T>
{
    /// <summary>
    /// The items in the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// The total count of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; set; }

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages { get; set; }
}