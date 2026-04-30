using Cars.ApiCommon.Extensions;
using Cars.ApiCommon.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add cosmos options.
builder.AddCosmosContainerOptions();
builder.AddCosmosAccountOptions();

// Add services to the DI container.
builder.RegisterServices();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();
