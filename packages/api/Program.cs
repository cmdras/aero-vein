using System.Reflection;
using Api.Domain;
using Api.ResourceAccess;
using Marten;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Marten")
    ?? throw new InvalidOperationException("ConnectionStrings:Marten is required");

builder.Services.AddMarten(opts =>
    {
        opts.Connection(connectionString);
    })
    .UseLightweightSessions()
    .InitializeWith(new MechelenSiteSeeds())
    .ApplyAllDatabaseChangesOnStartup();

builder.Services.AddScoped<ISiteAccess, SiteAccess>();
builder.Services.AddScoped<IMissionStore, MissionStore>();

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

app.MapGet("/api/sites", async (ISiteAccess sites, CancellationToken ct) =>
    await sites.ListAsync(ct))
    .WithName("GetSites")
    .WithTags("Sites");

app.Run();

// Exposes the implicit Program class so WebApplicationFactory<Program> can boot the
// app from the integration test project (top-level statements make it internal otherwise).
public partial class Program { }
