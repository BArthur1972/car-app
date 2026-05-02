namespace Cars.ApiCommon.Cosmos.Options
{
    public class CosmosContainerOptions
    {
        public const string SectionKey = "CosmosDB:CosmosContainerOptions";

        public required string DatabaseId { get; set; }
        public required string ContainerId { get; set; }
    }
}