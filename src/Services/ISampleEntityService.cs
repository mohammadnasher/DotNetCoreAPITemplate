using DotNetCoreAPITemplate.Models;

namespace DotNetCoreAPITemplate.Services;

/// <summary>
/// Interface for sample entity service operations
/// </summary>
public interface ISampleEntityService
{
    /// <summary>
    /// Gets all sample entities with optional filtering and pagination
    /// </summary>
    /// <param name="skip">Number of entities to skip for pagination</param>
    /// <param name="take">Number of entities to take for pagination</param>
    /// <param name="search">Optional search term to filter by name</param>
    /// <param name="isActive">Optional filter by active status</param>
    /// <param name="type">Optional filter by entity type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated result of sample entities</returns>
    Task<PaginatedResult<SampleEntity>> GetAllAsync(
        int skip = 0,
        int take = 10,
        string? search = null,
        bool? isActive = null,
        SampleEntityType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a sample entity by its ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sample entity if found, otherwise null</returns>
    Task<SampleEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new sample entity
    /// </summary>
    /// <param name="entity">The entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created entity with generated ID</returns>
    Task<SampleEntity> CreateAsync(SampleEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing sample entity
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="entity">The updated entity data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated entity if successful, otherwise null if not found</returns>
    Task<SampleEntity?> UpdateAsync(int id, SampleEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a sample entity by its ID
    /// </summary>
    /// <param name="id">The entity ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the entity was deleted, false if not found</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a sample entity exists with the given ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the entity exists, otherwise false</returns>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic paginated result container
/// </summary>
/// <typeparam name="T">The type of items in the result</typeparam>
public class PaginatedResult<T>
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
    /// The current page number (0-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => PageNumber * PageSize < TotalCount;

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => PageNumber > 0;

    /// <summary>
    /// The total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}