using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddFastEndpoints().SwaggerDocument();

var app = builder.Build();

app.UseFastEndpoints().UseSwaggerGen();

app.Run();