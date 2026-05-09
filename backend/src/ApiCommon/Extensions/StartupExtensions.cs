using Cars.ApiCommon.HealthChecks;
using Cars.DataAccess.Entities;
using Cars.Management;
using Microsoft.AspNetCore.Identity;

namespace Cars.ApiCommon.Extensions;

public static class StartupExtensions
{
    /// <summary>
    /// Registers all application services for the Cars API.
    /// </summary>
    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddCosmosDataAccess(builder.Configuration);
        builder.Services.AddCorsPolicy(builder.Configuration);
        builder.Services.AddJwtAuthentication(builder.Configuration);

        builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddSingleton<ICarManagementProvider, CarManagementProvider>();
        builder.Services.AddSingleton<IAuthManagementProvider, AuthManagementProvider>();

        builder.Services.AddHealthChecks()
            .AddCheck<CosmosHealthCheck>(
                "cosmos_health_check",
                tags: ["ready"],
                timeout: TimeSpan.FromSeconds(5));
    }
}
