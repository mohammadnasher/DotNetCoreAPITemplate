using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace DotNetCoreAPITemplate.Configuration;

/// <summary>
/// Extensions for configuring API versioning
/// </summary>
public static class VersioningExtensions
{
    /// <summary>
    /// Adds API versioning services with comprehensive configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApiVersioningServices(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Default version when no version is specified
            options.DefaultApiVersion = ApiVersion.Default;
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Configure versioning methods
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("version"),
                new QueryStringApiVersionReader("v"),
                new HeaderApiVersionReader("X-Version"),
                new HeaderApiVersionReader("X-API-Version"),
                new MediaTypeApiVersionReader("ver"),
                new UrlSegmentApiVersionReader()
            );

            // Configure version format
            options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
        });

        services.AddVersionedApiExplorer(setup =>
        {
            // Configure version format for API explorer
            setup.GroupNameFormat = "'v'VVV";
            setup.SubstituteApiVersionInUrl = true;
            setup.AssumeDefaultVersionWhenUnspecified = true;
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger to support multiple API versions
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddVersionedSwaggerDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiSettings = configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();

        services.AddSwaggerGen(options =>
        {
            // This will be called for each API version
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = description.ApiVersion.ToString(),
                    Title = $"{apiSettings.Title}",
                    Description = description.IsDeprecated
                        ? $"{apiSettings.Description} - DEPRECATED"
                        : apiSettings.Description,
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = apiSettings.Contact.Name,
                        Email = apiSettings.Contact.Email,
                        Url = !string.IsNullOrEmpty(apiSettings.Contact.Url)
                            ? new Uri(apiSettings.Contact.Url)
                            : null
                    },
                    License = new Microsoft.OpenApi.Models.OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });
            }

            // Configure XML documentation
            var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Enable annotations
            options.EnableAnnotations();

            // Filter out obsolete actions
            options.DocumentFilter<RemoveVersionFromParameter>();
            options.OperationFilter<RemoveVersionParameters>();
        });

        return services;
    }

    /// <summary>
    /// Configures versioned Swagger UI
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="provider">The API version description provider</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseVersionedSwaggerUI(
        this IApplicationBuilder app,
        IApiVersionDescriptionProvider provider,
        IConfiguration configuration)
    {
        var apiSettings = configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            // Create a swagger endpoint for each discovered API version
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/api-docs/{description.GroupName}/swagger.json",
                    $"{apiSettings.Title} {description.GroupName.ToUpperInvariant()}");
            }

            options.RoutePrefix = "api-docs";
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();
            options.EnableValidator();
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            options.DefaultModelExpandDepth(2);
            options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
        });

        return app;
    }
}

/// <summary>
/// Document filter to remove version parameter from Swagger documentation
/// </summary>
public class RemoveVersionFromParameter : Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter
{
    /// <summary>
    /// Applies the filter to remove version parameter
    /// </summary>
    /// <param name="swaggerDoc">The OpenAPI document</param>
    /// <param name="context">The document filter context</param>
    public void Apply(Microsoft.OpenApi.Models.OpenApiDocument swaggerDoc, Swashbuckle.AspNetCore.SwaggerGen.DocumentFilterContext context)
    {
        // Remove version parameter from paths
        foreach (var path in swaggerDoc.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var versionParameter = operation.Value.Parameters?.FirstOrDefault(p =>
                    p.Name.Equals("version", StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Equals("v", StringComparison.OrdinalIgnoreCase));

                if (versionParameter != null)
                {
                    operation.Value.Parameters.Remove(versionParameter);
                }
            }
        }
    }
}

/// <summary>
/// Operation filter to remove version parameters from operations
/// </summary>
public class RemoveVersionParameters : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    /// <summary>
    /// Applies the filter to remove version parameters from operations
    /// </summary>
    /// <param name="operation">The OpenAPI operation</param>
    /// <param name="context">The operation filter context</param>
    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        var versionParameter = operation.Parameters?.FirstOrDefault(p =>
            p.Name.Equals("version", StringComparison.OrdinalIgnoreCase) ||
            p.Name.Equals("v", StringComparison.OrdinalIgnoreCase));

        if (versionParameter != null)
        {
            operation.Parameters.Remove(versionParameter);
        }
    }
}