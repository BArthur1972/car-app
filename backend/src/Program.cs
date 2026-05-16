using Cars.ApiCommon.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterServices();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Initialize Cosmos DB infrastructure (database/container)
// This only auto-creates in Development or when using the Cosmos emulator
await app.InitializeCosmosDbAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks();

app.Run();
