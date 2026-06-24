using System.Reflection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi("/openapi/{documentName}.json");
app.MapScalarApiReference("/openapi", options =>
{
    options.WithOpenApiRoutePattern("/openapi/{documentName}.json");
});

app.MapGet("/", () => new
{
    name = "AeroVein API",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
});

// 401 until Microsoft OAuth (ARCHITECTURE §6.2) is wired.
app.MapGet("/api/auth/me", () => Results.Unauthorized());

app.Run();

// Exposes the implicit Program class so WebApplicationFactory<Program> can boot the
// app from the integration test project (top-level statements make it internal otherwise).
public partial class Program { }
