using Cars.ApiCommon.Cosmos.Options;
using Microsoft.Azure.Cosmos;

namespace Cars.ApiCommon.Cosmos;

public class CosmosFacade(
    CosmosAccountOptions cosmosAccountOptions,
    CosmosContainerOptions cosmosContainerOptions,
     ILogger logger)
{
    private readonly CosmosAccountOptions cosmosAccountOptions = cosmosAccountOptions;
    private readonly CosmosContainerOptions cosmosContainerOptions = cosmosContainerOptions;
    private readonly ILogger logger = logger;

    public Container GetContainer()
    {
        if (cosmosAccountOptions.CosmosClient == null)
        {
            throw new InvalidOperationException("Cosmos DB Client is not initialized.");
        }

        Container container = cosmosAccountOptions.CosmosClient.GetContainer(
            cosmosContainerOptions.DatabaseId,
            cosmosContainerOptions.ContainerId);

        logger.LogInformation(
            "Cosmos DB container initialized for database: {DatabaseId} and container: {ContainerId}",
            cosmosContainerOptions.DatabaseId,
            cosmosContainerOptions.ContainerId);

        return container;
    }
}
