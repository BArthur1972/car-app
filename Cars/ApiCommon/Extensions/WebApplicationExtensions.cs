using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Cars.ApiCommon.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Maps the health check endpoint.
    /// </summary>
    /// <param name="app">The WebApplication instance.</param>
    public static void MapHealthChecks(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var result = new 
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.ToDictionary(
                        entry => entry.Key,
                        entry => new 
                        {
                            name = entry.Key,
                            status = entry.Value.Status.ToString(),
                            exception = entry.Value.Exception?.Message,
                            duration = entry.Value.Duration.ToString()
                        })
                };

                await context.Response.WriteAsJsonAsync(result);
            }
        });
    }
}
