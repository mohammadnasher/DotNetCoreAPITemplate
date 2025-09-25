using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace DotNetCoreAPITemplate.Configuration;

/// <summary>
/// Extensions for configuring Swagger/OpenAPI documentation
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger/OpenAPI services with comprehensive configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiSettings = configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();

        services.AddSwaggerGen(options =>
        {
            // Configure API information
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = apiSettings.Version,
                Title = apiSettings.Title,
                Description = apiSettings.Description,
                Contact = new OpenApiContact
                {
                    Name = apiSettings.Contact.Name,
                    Email = apiSettings.Contact.Email,
                    Url = !string.IsNullOrEmpty(apiSettings.Contact.Url) ? new Uri(apiSettings.Contact.Url) : null
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add XML documentation
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            // Add security definitions for Bearer token
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            });

            // Add security requirement
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Configure additional options
            options.EnableAnnotations();
            options.DescribeAllParametersInCamelCase();
            options.SupportNonNullableReferenceTypes();

            // Custom schema filters
            options.SchemaFilter<RequiredNotNullableSchemaFilter>();
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger middleware in the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="configuration">The configuration instance</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseSwaggerDocumentation(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var apiSettings = configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api-docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/api-docs/v1/swagger.json", $"{apiSettings.Title} {apiSettings.Version}");
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
/// Schema filter to mark required properties as non-nullable in Swagger documentation
/// </summary>
public class RequiredNotNullableSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Applies the filter to mark required properties as non-nullable
    /// </summary>
    /// <param name="schema">The OpenAPI schema</param>
    /// <param name="context">The schema filter context</param>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null) return;

        var requiredProperties = schema.Required?.ToHashSet() ?? new HashSet<string>();

        foreach (var property in schema.Properties)
        {
            if (requiredProperties.Contains(property.Key) && property.Value.Nullable == true)
            {
                property.Value.Nullable = false;
            }
        }
    }
}