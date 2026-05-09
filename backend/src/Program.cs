using Cars.ApiCommon.Extensions;
using Cars.ApiCommon.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize Cosmos DB infrastructure (database/container)
// In production, infrastructure should be pre-provisioned via IaC (Bicep/Terraform)
// This only auto-creates in Development or when using the Cosmos emulator
await app.InitializeCosmosDbAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks();

app.Run();
