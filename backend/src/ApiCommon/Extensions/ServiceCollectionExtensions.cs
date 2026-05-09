using Cars.ApiCommon.Cosmos;
using Cars.ApiCommon.Cosmos.Options;
using Cars.DataAccess;
using Microsoft.Extensions.Options;

namespace Cars.ApiCommon.Extensions;

public static class ServiceCollectionExtensions
{
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

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    public static IServiceCollection AddCosmosDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptionsWithValidation<CosmosContainerOptions>(
            configuration.GetSection(CosmosContainerOptions.SectionKey));

        // Initialize CosmosClient during DI configuration
        services.AddOptionsWithValidation<CosmosAccountOptions>(
            configuration.GetSection(CosmosAccountOptions.SectionKey))
            .Configure<ILoggerFactory, IOptions<CosmosContainerOptions>>(
                (options, loggerFactory, cosmosContainerOptions) =>
                {
                    ArgumentNullException.ThrowIfNull(loggerFactory);
                    ArgumentNullException.ThrowIfNull(cosmosContainerOptions);

                    var logger = loggerFactory.CreateLogger<CosmosAccountOptions>();
                    options.InitializeCosmosClient(logger, cosmosContainerOptions.Value);
                });

        // Add a single instance of the Cosmos container to be shared across the application.
        services.AddSingleton(sp =>
        {
            var cosmosOptions = sp.GetRequiredService<IOptions<CosmosAccountOptions>>().Value;
            var containerOptions = sp.GetRequiredService<IOptions<CosmosContainerOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<CosmosFacade>>();
            return new CosmosFacade(cosmosOptions, containerOptions, logger).GetContainer();
        });

        services.AddSingleton<ICarDataProvider, CarDataProvider>();

        return services;
    }
}
