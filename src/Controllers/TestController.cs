using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using DotNetCoreAPITemplate.Models;
using DotNetCoreAPITemplate.Models.DTOs;
using DotNetCoreAPITemplate.Services;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreAPITemplate.Controllers;

/// <summary>
/// Test controller demonstrating comprehensive CRUD operations with standard practices
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/test")]
[Produces("application/json")]
[SwaggerTag("Test endpoints for demonstrating CRUD operations and API patterns")]
public class TestController : ControllerBase
{
    private readonly ISampleEntityService _sampleEntityService;
    private readonly ILogger<TestController> _logger;

    /// <summary>
    /// Initializes a new instance of the TestController
    /// </summary>
    /// <param name="sampleEntityService">The sample entity service</param>
    /// <param name="logger">The logger instance</param>
    public TestController(
        ISampleEntityService sampleEntityService,
        ILogger<TestController> logger)
    {
        _sampleEntityService = sampleEntityService ?? throw new ArgumentNullException(nameof(sampleEntityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all sample entities with optional filtering and pagination
    /// </summary>
    /// <param name="pageNumber">The page number (1-based, default: 1)</param>
    /// <param name="pageSize">The page size (default: 10, max: 100)</param>
    /// <param name="search">Optional search term to filter by name or description</param>
    /// <param name="isActive">Optional filter by active status</param>
    /// <param name="type">Optional filter by entity type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A paginated list of sample entities</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all sample entities",
        Description = "Retrieves a paginated list of sample entities with optional filtering capabilities."
    )]
    [SwaggerResponse(200, "Successfully retrieved sample entities", typeof(PaginatedResultDto<SampleEntityDto>))]
    [SwaggerResponse(400, "Invalid request parameters")]
    public async Task<ActionResult<PaginatedResultDto<SampleEntityDto>>> GetAllAsync(
        [FromQuery][Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery][Range(1, 100)] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] SampleEntityType? type = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all sample entities - Page: {PageNumber}, Size: {PageSize}, Search: {Search}, IsActive: {IsActive}, Type: {Type}",
            pageNumber, pageSize, search, isActive, type);

        var skip = (pageNumber - 1) * pageSize;
        var result = await _sampleEntityService.GetAllAsync(skip, pageSize, search, isActive, type, cancellationToken);

        var dto = new PaginatedResultDto<SampleEntityDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            HasNextPage = result.HasNextPage,
            HasPreviousPage = result.HasPreviousPage,
            TotalPages = result.TotalPages
        };

        _logger.LogInformation("Retrieved {Count} sample entities from page {Page}", dto.Items.Count, pageNumber);

        return Ok(dto);
    }

    /// <summary>
    /// Gets a specific sample entity by its ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sample entity if found</returns>
    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Get sample entity by ID",
        Description = "Retrieves a specific sample entity by its unique identifier."
    )]
    [SwaggerResponse(200, "Successfully retrieved the sample entity", typeof(SampleEntityDto))]
    [SwaggerResponse(404, "Sample entity not found")]
    [SwaggerResponse(400, "Invalid ID parameter")]
    public async Task<ActionResult<SampleEntityDto>> GetByIdAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting sample entity by ID: {Id}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID provided: {Id}", id);
            return BadRequest("ID must be a positive integer.");
        }

        var entity = await _sampleEntityService.GetByIdAsync(id, cancellationToken);

        if (entity == null)
        {
            _logger.LogWarning("Sample entity with ID {Id} not found", id);
            return NotFound($"Sample entity with ID {id} was not found.");
        }

        _logger.LogDebug("Successfully retrieved sample entity: {Name}", entity.Name);

        return Ok(MapToDto(entity));
    }

    /// <summary>
    /// Creates a new sample entity
    /// </summary>
    /// <param name="createDto">The entity data to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sample entity</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new sample entity",
        Description = "Creates a new sample entity with the provided data."
    )]
    [SwaggerResponse(201, "Successfully created the sample entity", typeof(SampleEntityDto))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(409, "Entity with the same name already exists")]
    public async Task<ActionResult<SampleEntityDto>> CreateAsync(
        [FromBody] CreateSampleEntityDto createDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating new sample entity: {Name}", createDto.Name);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for create request");
            return BadRequest(ModelState);
        }

        try
        {
            var entity = MapFromCreateDto(createDto);
            var createdEntity = await _sampleEntityService.CreateAsync(entity, cancellationToken);

            _logger.LogInformation("Successfully created sample entity with ID {Id}: {Name}",
                createdEntity.Id, createdEntity.Name);

            var dto = MapToDto(createdEntity);

            return CreatedAtAction(
                nameof(GetByIdAsync),
                new { id = dto.Id, version = "1.0" },
                dto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict occurred while creating sample entity: {Name}", createDto.Name);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating sample entity: {Name}", createDto.Name);
            return StatusCode(500, new { message = "An error occurred while creating the entity." });
        }
    }

    /// <summary>
    /// Updates an existing sample entity
    /// </summary>
    /// <param name="id">The entity ID to update</param>
    /// <param name="updateDto">The updated entity data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated sample entity</returns>
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Update an existing sample entity",
        Description = "Updates an existing sample entity with the provided data."
    )]
    [SwaggerResponse(200, "Successfully updated the sample entity", typeof(SampleEntityDto))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(404, "Sample entity not found")]
    [SwaggerResponse(409, "Entity with the same name already exists")]
    public async Task<ActionResult<SampleEntityDto>> UpdateAsync(
        [FromRoute] int id,
        [FromBody] UpdateSampleEntityDto updateDto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating sample entity with ID {Id}: {Name}", id, updateDto.Name);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID provided: {Id}", id);
            return BadRequest("ID must be a positive integer.");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for update request");
            return BadRequest(ModelState);
        }

        try
        {
            var entity = MapFromUpdateDto(updateDto);
            var updatedEntity = await _sampleEntityService.UpdateAsync(id, entity, cancellationToken);

            if (updatedEntity == null)
            {
                _logger.LogWarning("Sample entity with ID {Id} not found for update", id);
                return NotFound($"Sample entity with ID {id} was not found.");
            }

            _logger.LogInformation("Successfully updated sample entity with ID {Id}: {Name}",
                id, updatedEntity.Name);

            return Ok(MapToDto(updatedEntity));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict occurred while updating sample entity with ID {Id}", id);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating sample entity with ID {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating the entity." });
        }
    }

    /// <summary>
    /// Deletes a sample entity by its ID
    /// </summary>
    /// <param name="id">The entity ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Delete a sample entity",
        Description = "Deletes a sample entity by its unique identifier."
    )]
    [SwaggerResponse(204, "Successfully deleted the sample entity")]
    [SwaggerResponse(400, "Invalid ID parameter")]
    [SwaggerResponse(404, "Sample entity not found")]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting sample entity with ID: {Id}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID provided: {Id}", id);
            return BadRequest("ID must be a positive integer.");
        }

        try
        {
            var deleted = await _sampleEntityService.DeleteAsync(id, cancellationToken);

            if (!deleted)
            {
                _logger.LogWarning("Sample entity with ID {Id} not found for deletion", id);
                return NotFound($"Sample entity with ID {id} was not found.");
            }

            _logger.LogInformation("Successfully deleted sample entity with ID {Id}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting sample entity with ID {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the entity." });
        }
    }

    /// <summary>
    /// Checks if a sample entity exists by its ID
    /// </summary>
    /// <param name="id">The entity ID to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>OK if exists, NotFound if not</returns>
    [HttpHead("{id:int}")]
    [SwaggerOperation(
        Summary = "Check if sample entity exists",
        Description = "Checks if a sample entity exists by its unique identifier using HEAD request."
    )]
    [SwaggerResponse(200, "Sample entity exists")]
    [SwaggerResponse(404, "Sample entity does not exist")]
    [SwaggerResponse(400, "Invalid ID parameter")]
    public async Task<IActionResult> ExistsAsync(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking existence of sample entity with ID: {Id}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid ID provided: {Id}", id);
            return BadRequest();
        }

        var exists = await _sampleEntityService.ExistsAsync(id, cancellationToken);

        _logger.LogDebug("Sample entity with ID {Id} exists: {Exists}", id, exists);

        return exists ? Ok() : NotFound();
    }

    /// <summary>
    /// Maps a SampleEntity to a SampleEntityDto
    /// </summary>
    /// <param name="entity">The entity to map</param>
    /// <returns>The mapped DTO</returns>
    private static SampleEntityDto MapToDto(SampleEntity entity)
    {
        return new SampleEntityDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            Value = entity.Value,
            Type = entity.Type,
            Tags = entity.Tags,
            Metadata = entity.Metadata,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a CreateSampleEntityDto to a SampleEntity
    /// </summary>
    /// <param name="dto">The DTO to map</param>
    /// <returns>The mapped entity</returns>
    private static SampleEntity MapFromCreateDto(CreateSampleEntityDto dto)
    {
        return new SampleEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            Value = dto.Value,
            Type = dto.Type,
            Tags = dto.Tags,
            Metadata = dto.Metadata
        };
    }

    /// <summary>
    /// Maps an UpdateSampleEntityDto to a SampleEntity
    /// </summary>
    /// <param name="dto">The DTO to map</param>
    /// <returns>The mapped entity</returns>
    private static SampleEntity MapFromUpdateDto(UpdateSampleEntityDto dto)
    {
        return new SampleEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            Value = dto.Value,
            Type = dto.Type,
            Tags = dto.Tags,
            Metadata = dto.Metadata
        };
    }
}