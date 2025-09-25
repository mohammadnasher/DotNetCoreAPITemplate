using DotNetCoreAPITemplate.Configuration;
using DotNetCoreAPITemplate.Services;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Serilog;

// Configure Serilog early in the application startup
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog logging
builder.ConfigureSerilogLogging();

try
{
    Log.Information("Starting up DotNet Core API Template");

    // Add configuration services
    builder.Services.AddApplicationConfiguration(builder.Configuration);

    // Add database services
    builder.Services.AddDatabaseServices(builder.Configuration);

    // Add API versioning
    builder.Services.AddApiVersioningServices();

    // Add controllers and API explorer
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Add Swagger documentation with versioning support
    builder.Services.AddVersionedSwaggerDocumentation(builder.Configuration);

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            var corsSettings = builder.Configuration.GetSection("CorsSettings").Get<CorsSettings>() ?? new();

            if (corsSettings.AllowedOrigins.Length > 0)
            {
                policy.WithOrigins(corsSettings.AllowedOrigins);
            }
            else
            {
                policy.AllowAnyOrigin();
            }

            policy.AllowAnyMethod()
                  .AllowAnyHeader();

            if (corsSettings.AllowCredentials && corsSettings.AllowedOrigins.Length > 0)
            {
                policy.AllowCredentials();
            }
        });
    });

    // Add health checks
    builder.Services.AddHealthChecks();

    // Register application services
    builder.Services.AddScoped<ISampleEntityService, SampleEntityService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline

    // Use request logging (must be early in pipeline)
    app.UseRequestLogging();

    // Use performance logging
    app.UsePerformanceLogging();

    // Development-specific middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    // CORS
    app.UseCors();

    // HTTPS redirection
    app.UseHttpsRedirection();

    // Authentication and Authorization (if needed in the future)
    // app.UseAuthentication();
    app.UseAuthorization();

    // Swagger documentation
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseVersionedSwaggerUI(provider, builder.Configuration);

    // Health checks
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });

    // Map controllers
    app.MapControllers();

    // Ensure database is created and migrated
    app.EnsureDatabaseCreated(Log.Logger);

    // Seed database if empty
    if (app.Environment.IsDevelopment())
    {
        app.SeedDatabase(Log.Logger);
    }

    Log.Information("DotNet Core API Template started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
