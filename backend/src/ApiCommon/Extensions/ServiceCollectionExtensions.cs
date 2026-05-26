using System.Security.Cryptography.X509Certificates;
using System.Text;
using Cars.ApiCommon.Auth;
using Cars.ApiCommon.Cosmos;
using Cars.ApiCommon.Cosmos.Options;
using Cars.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

        var containerOptionsMap = CosmosContainerConstants.ContainerNames
            .ToDictionary(
                name => name,
                name => configuration
                    .GetSection($"{CosmosContainerConstants.ContainersSectionKey}:{name}")
                    .Get<CosmosContainerOptions>()
                    ?? throw new InvalidOperationException($"Missing Cosmos container config for '{name}'"));

        services.AddOptionsWithValidation<CosmosAccountOptions>(
            configuration.GetSection(CosmosAccountOptions.SectionKey))
            .Configure<ILoggerFactory>(
                (options, loggerFactory) =>
                {
                    var logger = loggerFactory.CreateLogger<CosmosAccountOptions>();
                    options.InitializeCosmosClient(logger, containerOptionsMap.Values);
                });

        foreach (var (name, containerOptions) in containerOptionsMap)
        {
            services.AddKeyedSingleton(name, (sp, _) =>
            {
                var cosmosOptions = sp.GetRequiredService<IOptions<CosmosAccountOptions>>().Value;
                var logger = sp.GetRequiredService<ILogger<CosmosFacade>>();
                return new CosmosFacade(cosmosOptions, containerOptions, logger).GetContainer();
            });
        }

        services.AddSingleton<ICarDataProvider, CarDataProvider>();
        services.AddSingleton<IUserDataProvider, UserDataProvider>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptionsWithValidation<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionKey));

        // Register the JWT token service used for generating tokens
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionKey).Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing");

        SecurityKey signingKey;
        if (!string.IsNullOrEmpty(jwtOptions.CertificateBase64))
        {
            var certBytes = Convert.FromBase64String(jwtOptions.CertificateBase64);
            var certificate = X509CertificateLoader.LoadPkcs12(certBytes, jwtOptions.CertificatePassword);
            signingKey = new RsaSecurityKey(certificate.GetRSAPrivateKey());
        }
        else if (!string.IsNullOrEmpty(jwtOptions.Secret))
        {
            signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));
        }
        else
        {
            throw new InvalidOperationException(
                "JWT signing key not configured. Provide either Jwt:CertificateBase64 or Jwt:Secret");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ClockSkew = TimeSpan.Zero,
                };
            });

        return services;
    }
}
