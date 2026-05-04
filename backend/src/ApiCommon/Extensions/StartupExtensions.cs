using Cars.ApiCommon.Cosmos;
using Cars.ApiCommon.Cosmos.Options;
using Cars.ApiCommon.HealthChecks;
using Cars.DataAccess;
using Cars.Management;
using Microsoft.Extensions.Options;

namespace Cars.ApiCommon.Extensions;

public static class StartupExtensions
{
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddDataAccessServices();

        builder.Services.AddSingleton<ICarManagementProvider, CarManagementProvider>();

        builder.Services.AddHealthChecks()
            .AddCheck<CosmosHealthCheck>(
                "cosmos_health_check",
                timeout: TimeSpan.FromSeconds(5));
    }

    public static OptionsBuilder<TOptions> AddOptionsWithValidation<TOptions>(
        this IServiceCollection services,
        IConfigurationSection configurationSection)
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);

        return services
            .AddOptions<TOptions>()
            .Bind(configurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static void AddDataAccessServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptionsWithValidation<CosmosContainerOptions>(
            builder.Configuration.GetSection(CosmosContainerOptions.SectionKey));

        builder.Services.AddOptionsWithValidation<CosmosAccountOptions>(
            builder.Configuration.GetSection(CosmosAccountOptions.SectionKey))
            .Configure<ILoggerFactory, IOptions<CosmosContainerOptions>>(
                (options, loggerFactory, cosmosContainerOptions) =>
                {
                    ArgumentNullException.ThrowIfNull(loggerFactory);
                    ArgumentNullException.ThrowIfNull(cosmosContainerOptions);

                    var logger = loggerFactory.CreateLogger<CosmosAccountOptions>();
                    options.InitializeCosmosClient(logger, cosmosContainerOptions.Value);
                });

        // Add a single instance of the Cosmos container to be shared across the application.
        builder.Services.AddSingleton(sp =>
        {
            var cosmosOptions = sp.GetRequiredService<IOptions<CosmosAccountOptions>>().Value;
            var containerOptions = sp.GetRequiredService<IOptions<CosmosContainerOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<CosmosFacade>>();
            return new CosmosFacade(cosmosOptions, containerOptions, logger).GetContainer();
        });

        builder.Services.AddSingleton<ICarDataProvider, CarDataProvider>();
    }
}
