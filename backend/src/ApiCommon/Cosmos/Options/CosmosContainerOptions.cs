namespace Cars.ApiCommon.Cosmos.Options;

public class CosmosContainerOptions
{
    public required string DatabaseId { get; set; }
    public required string ContainerId { get; set; }
    public required string PartitionKey { get; set; }
}
