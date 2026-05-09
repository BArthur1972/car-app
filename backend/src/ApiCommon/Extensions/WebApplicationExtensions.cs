using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Cars.ApiCommon.Cosmos.Options;

namespace Cars.ApiCommon.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Ensures Cosmos DB database and container exist.
    /// CosmosClient must already be initialized (happens during DI configuration).
    /// Only creates database/container in Development or when using Emulator.
    /// Should be called after app.Build() and before app.Run().
    /// </summary>
    public static async Task InitializeCosmosDbAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var cosmosAccountOptions = app.Services.GetRequiredService<IOptions<CosmosAccountOptions>>().Value;

        if (cosmosAccountOptions.CosmosClient is null)
        {
            logger.LogWarning("CosmosClient is not initialized. Skipping database initialization.");
            return;
        }

        if (!app.Environment.IsDevelopment() && !cosmosAccountOptions.UseEmulator)
        {
            logger.LogInformation(
                "Production mode: Skipping auto-creation of database/container. " +
                "Infrastructure should be pre-provisioned.");
            return;
        }

        try
        {
            foreach (var containerOpts in cosmosAccountOptions.ContainerOptions)
            {
                var databaseResponse = await cosmosAccountOptions.CosmosClient
                    .CreateDatabaseIfNotExistsAsync(containerOpts.DatabaseId);

                logger.LogInformation(
                    "Database '{DatabaseId}' ready (Status: {Status})",
                    containerOpts.DatabaseId,
                    databaseResponse.StatusCode);

                var containerResponse = await databaseResponse.Database
                    .CreateContainerIfNotExistsAsync(containerOpts.ContainerId, containerOpts.PartitionKey);

                logger.LogInformation(
                    "Container '{ContainerId}' ready (Status: {Status})",
                    containerOpts.ContainerId,
                    containerResponse.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize Cosmos DB database and containers");
            throw;
        }
    }

    public static void MapHealthChecks(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteResponse
        });
    }

    private static async Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    exception = entry.Value.Exception?.Message,
                    duration = entry.Value.Duration.ToString()
                })
        });
    }
}
