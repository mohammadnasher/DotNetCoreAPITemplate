using Microsoft.EntityFrameworkCore;
using DotNetCoreAPITemplate.Data;
using DotNetCoreAPITemplate.Models;

namespace DotNetCoreAPITemplate.Services;

/// <summary>
/// Implementation of sample entity service operations
/// </summary>
public class SampleEntityService : ISampleEntityService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SampleEntityService> _logger;

    /// <summary>
    /// Initializes a new instance of the SampleEntityService
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="logger">The logger instance</param>
    public SampleEntityService(ApplicationDbContext context, ILogger<SampleEntityService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<PaginatedResult<SampleEntity>> GetAllAsync(
        int skip = 0,
        int take = 10,
        string? search = null,
        bool? isActive = null,
        SampleEntityType? type = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all sample entities with skip: {Skip}, take: {Take}, search: {Search}, isActive: {IsActive}, type: {Type}",
            skip, take, search, isActive, type);

        var query = _context.SampleEntities.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Name.Contains(search) ||
                                   (e.Description != null && e.Description.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(e => e.IsActive == isActive.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(e => e.Type == type.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply ordering and pagination
        var items = await query
            .OrderBy(e => e.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} sample entities out of {Total} total",
            items.Count, totalCount);

        return new PaginatedResult<SampleEntity>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = skip / take,
            PageSize = take
        };
    }

    /// <inheritdoc />
    public async Task<SampleEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting sample entity by ID: {Id}", id);

        var entity = await _context.SampleEntities
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Sample entity with ID {Id} not found", id);
        }
        else
        {
            _logger.LogDebug("Found sample entity: {Name}", entity.Name);
        }

        return entity;
    }

    /// <inheritdoc />
    public async Task<SampleEntity> CreateAsync(SampleEntity entity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating new sample entity: {Name}", entity.Name);

        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        // Validate name uniqueness
        var existingEntity = await _context.SampleEntities
            .FirstOrDefaultAsync(e => e.Name == entity.Name, cancellationToken);

        if (existingEntity != null)
        {
            _logger.LogWarning("Cannot create sample entity: Name '{Name}' already exists", entity.Name);
            throw new InvalidOperationException($"An entity with the name '{entity.Name}' already exists.");
        }

        // Set creation timestamp
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.SampleEntities.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created sample entity with ID {Id}: {Name}", entity.Id, entity.Name);

        return entity;
    }

    /// <inheritdoc />
    public async Task<SampleEntity?> UpdateAsync(int id, SampleEntity updatedEntity, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating sample entity with ID {Id}", id);

        if (updatedEntity == null)
        {
            throw new ArgumentNullException(nameof(updatedEntity));
        }

        var existingEntity = await _context.SampleEntities
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (existingEntity == null)
        {
            _logger.LogWarning("Cannot update: Sample entity with ID {Id} not found", id);
            return null;
        }

        // Validate name uniqueness (excluding current entity)
        if (existingEntity.Name != updatedEntity.Name)
        {
            var nameExists = await _context.SampleEntities
                .AnyAsync(e => e.Name == updatedEntity.Name && e.Id != id, cancellationToken);

            if (nameExists)
            {
                _logger.LogWarning("Cannot update sample entity: Name '{Name}' already exists", updatedEntity.Name);
                throw new InvalidOperationException($"An entity with the name '{updatedEntity.Name}' already exists.");
            }
        }

        // Update properties
        existingEntity.Name = updatedEntity.Name;
        existingEntity.Description = updatedEntity.Description;
        existingEntity.IsActive = updatedEntity.IsActive;
        existingEntity.Value = updatedEntity.Value;
        existingEntity.Type = updatedEntity.Type;
        existingEntity.Tags = updatedEntity.Tags;
        existingEntity.Metadata = updatedEntity.Metadata;
        existingEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated sample entity with ID {Id}: {Name}", id, existingEntity.Name);

        return existingEntity;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting sample entity with ID {Id}", id);

        var entity = await _context.SampleEntities
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Cannot delete: Sample entity with ID {Id} not found", id);
            return false;
        }

        _context.SampleEntities.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted sample entity with ID {Id}: {Name}", id, entity.Name);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if sample entity with ID {Id} exists", id);

        var exists = await _context.SampleEntities
            .AnyAsync(e => e.Id == id, cancellationToken);

        _logger.LogDebug("Sample entity with ID {Id} exists: {Exists}", id, exists);

        return exists;
    }
}