using System.Text.Json.Serialization;
using SmartX.Api.Endpoints;
using SmartX.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("SmartXClient", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173", // Vite dev server default
                "http://localhost:4173",
                "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<SensorRegistry>();
builder.Services.AddSingleton<AlertFeed>();
builder.Services.AddSingleton<TelemetryEngine>();
builder.Services.AddSingleton<FileStorageService>();
builder.Services.AddSingleton<MockTelemetrySeeder>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MockTelemetrySeeder>());

var app = builder.Build();

app.UseCors("SmartXClient");
app.UseStaticFiles(); // serves /uploads/** from wwwroot

app.MapPillarEndpoints();
app.MapSensorEndpoints();
app.MapTelemetryEndpoints();
app.MapDeploymentTreeEndpoints();

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", utc = DateTimeOffset.UtcNow }))
    .WithTags("Health");

app.Run();
