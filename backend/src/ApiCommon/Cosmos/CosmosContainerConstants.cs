namespace Cars.ApiCommon.Cosmos;

public static class CosmosContainerConstants
{
    public const string ContainersSectionKey = "CosmosDB:Containers";

    public const string CarsContainer = "cars";
    public const string UsersContainer = "users";

    public static readonly IReadOnlySet<string> ContainerNames = new HashSet<string>
    {
        CarsContainer,
        UsersContainer,
    };
}
