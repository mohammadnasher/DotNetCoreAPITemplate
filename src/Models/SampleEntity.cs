using DotNetCoreAPITemplate.Data;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreAPITemplate.Models;

/// <summary>
/// Sample entity for demonstrating CRUD operations and database structure
/// </summary>
public class SampleEntity : BaseEntity
{
    /// <summary>
    /// The name of the sample entity (required, max 100 characters)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// An optional description of the sample entity (max 500 characters)
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the entity is active or not
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// An optional numeric value for demonstration purposes
    /// </summary>
    public decimal? Value { get; set; }

    /// <summary>
    /// Category or type classification
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
/// Enumeration of sample entity types
/// </summary>
public enum SampleEntityType
{
    /// <summary>
    /// Standard type entity
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Premium type entity
    /// </summary>
    Premium = 1,

    /// <summary>
    /// Enterprise type entity
    /// </summary>
    Enterprise = 2,

    /// <summary>
    /// Custom type entity
    /// </summary>
    Custom = 3
}