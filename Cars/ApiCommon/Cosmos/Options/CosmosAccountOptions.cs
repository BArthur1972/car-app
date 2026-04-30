using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace Cars.ApiCommon.Cosmos.Options
{
    public class CosmosAccountOptions
    {
        public const string SectionKey = "CosmosDB:CosmosAccountOptions";

        /// <summary>
        /// Gets or sets the Cosmos DB account endpoint.
        /// </summary>
        public string AccountEndpoint { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether MSI credentials will be used or not.
        /// </summary>
        public bool UseManagedIdentity { get; set; }

        /// <summary>
        /// The environment variable name for the Cosmos DB MSI client ID.
        /// </summary>
        public string CosmosMSIEnvName = "COSMOS_MSI_CLIENT_ID";

        /// <summary>
        /// Gets or sets the Cosmos client to use for connecting to the Cosmos DB account.
        /// </summary>
        public CosmosClient? CosmosClient { get; private set; }

        /// <summary>
        /// Initializes the Cosmos client with the specified options.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="cosmosContainerOptions">The Cosmos container options.</param>
        public void InitializeCosmosClient(ILogger logger, CosmosContainerOptions? cosmosContainerOptions = null)
        {
            ArgumentNullException.ThrowIfNull(cosmosContainerOptions, nameof(cosmosContainerOptions));

            TokenCredential credential; 
            if (UseManagedIdentity)
            {
                // In production, use ManagedIdentityCredential specifically
                logger.LogInformation("Using Managed Identity for Cosmos DB authentication");
                var clientId = Environment.GetEnvironmentVariable(CosmosMSIEnvName);
                credential = new ManagedIdentityCredential(clientId);
            }
            else
            {
                // For local development, use DefaultAzureCredential
                // This will try environment variables, Visual Studio, Azure CLI, etc.
                logger.LogInformation("Using DefaultAzureCredential for Cosmos DB authentication");
                credential = new DefaultAzureCredential();
            }
            
            try
            {
                // Initialize container for CosmosClient
                IReadOnlyList<(string, string)> containers = [
                    (cosmosContainerOptions.DatabaseId, cosmosContainerOptions.ContainerId)
                ];

                // Options to override default serialization and use System.Text.Json
                CosmosClientOptions clientOptions = new()
                {
                    UseSystemTextJsonSerializerWithOptions = new System.Text.Json.JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    }
                };

                CosmosClient ??= CosmosClient.CreateAndInitializeAsync(
                    AccountEndpoint, 
                    credential, 
                    containers,
                    clientOptions).GetAwaiter().GetResult(); // Calling GetAwaiter().GetResult() to block until the task is completed.
                
                logger.LogInformation("Cosmos DB client initialized for endpoint: {Endpoint}", CosmosClient.Endpoint.AbsoluteUri);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to initialize Cosmos DB client with token credentials");
                throw;
            }
        }
    }
}
