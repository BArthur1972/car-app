using Cars.ApiCommon.Cosmos.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Cars.ApiCommon.HealthChecks;

public class CosmosHealthCheck(
    IOptions<CosmosAccountOptions> cosmosAccountOptions, 
    IOptions<CosmosContainerOptions> cosmosContainerOptions) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cosmosClient = cosmosAccountOptions.Value.CosmosClient ?? 
                throw new InvalidOperationException("CosmosClient is not configured.");

            var container = cosmosClient.GetContainer(
                cosmosContainerOptions.Value.DatabaseId,
                cosmosContainerOptions.Value.ContainerId);

            await container.ReadContainerAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("CosmosDB is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("CosmosDB health check failed.", ex);
        }
    }
}
