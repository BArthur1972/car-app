using System.Net.Sockets;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace Cars.ApiCommon.Cosmos.Options;

public class CosmosAccountOptions
{
    public const string SectionKey = "CosmosDB:CosmosAccountOptions";

    /// <summary>
    /// Gets or sets the Cosmos DB account endpoint.
    /// </summary>
    public required string AccountEndpoint { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether MSI credentials will be used or not.
    /// </summary>
    public bool UseManagedIdentity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the local Cosmos DB emulator is being used.
    /// </summary>
    public bool UseEmulator { get; set; }

    /// <summary>
    /// Gets or sets the Cosmos DB account key. Only used when connecting to the local emulator.
    /// </summary>
    public string? AccountKey { get; set; }

    /// <summary>
    /// The environment variable name for the Cosmos DB MSI client ID.
    /// </summary>
    public string CosmosMsiEnvName = "COSMOS_MSI_CLIENT_ID";

    /// <summary>
    /// Gets or sets the Cosmos client to use for connecting to the Cosmos DB account.
    /// </summary>
    public CosmosClient? CosmosClient { get; private set; }

    /// <summary>
    /// Initializes the Cosmos client with the specified options.
    /// </summary>
    /// <param name="logger">The logger to use for logging.</param>
    /// <param name="cosmosContainerOptions">The Cosmos container options.</param>
    public void InitializeCosmosClient(ILogger logger, CosmosContainerOptions cosmosContainerOptions)
    {

        try
        {
            IReadOnlyList<(string, string)> containers = [
                (cosmosContainerOptions.DatabaseId, cosmosContainerOptions.ContainerId)
            ];

            CosmosClientOptions clientOptions = new()
            {
                UseSystemTextJsonSerializerWithOptions = new System.Text.Json.JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                }
            };

            if (UseEmulator)
            {
                clientOptions.ConnectionMode = ConnectionMode.Gateway;

                // The Cosmos emulator returns 127.0.0.1 in its service discovery responses.
                // From within a Docker container, 127.0.0.1 resolves to the container's own
                // loopback, not the emulator. This handler intercepts those connections and
                // redirects them to the actual emulator hostname.
                clientOptions.HttpClientFactory = () => new HttpClient(new SocketsHttpHandler
                {
                    ConnectCallback = async (context, ct) =>
                    {
                        var emulatorHost = new Uri(AccountEndpoint).Host;
                        var host = context.DnsEndPoint.Host is "127.0.0.1" or "localhost"
                            ? emulatorHost
                            : context.DnsEndPoint.Host;
                        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
                        {
                            NoDelay = true
                        };
                        await socket.ConnectAsync(host, context.DnsEndPoint.Port, ct);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                });

                logger.LogInformation("Using local Cosmos DB emulator at {Endpoint}", AccountEndpoint);
                CosmosClient ??= CosmosClient.CreateAndInitializeAsync(
                    AccountEndpoint,
                    AccountKey,
                    containers,
                    clientOptions).GetAwaiter().GetResult();
            }
            else
            {
                TokenCredential credential;
                if (UseManagedIdentity)
                {
                    logger.LogInformation("Using Managed Identity for Cosmos DB authentication");
                    var clientId = Environment.GetEnvironmentVariable(CosmosMsiEnvName);
                    credential = new ManagedIdentityCredential(clientId);
                }
                else
                {
                    logger.LogInformation("Using DefaultAzureCredential for Cosmos DB authentication");
                    credential = new DefaultAzureCredential();
                }

                CosmosClient ??= CosmosClient.CreateAndInitializeAsync(
                    AccountEndpoint,
                    credential,
                    containers,
                    clientOptions).GetAwaiter().GetResult();
            }

            logger.LogInformation("Cosmos DB client initialized for endpoint: {Endpoint}", CosmosClient.Endpoint.AbsoluteUri);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize Cosmos DB client");
            throw;
        }
    }
}
