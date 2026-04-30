using Cars.ApiCommon.Extensions;
using Cars.ApiCommon.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add cosmos options.
builder.AddCosmosContainerOptions();
builder.AddCosmosAccountOptions();

// Add services to the DI container.
builder.RegisterServices();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
