using SmartX.Api.Contracts;
using SmartX.Api.Services;
using SmartX.Shared.Domain;

namespace SmartX.Api.Endpoints;

public static class TelemetryEndpoints
{
    public static void MapTelemetryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/telemetry").WithTags("Telemetry");

        group.MapPost("/ingest", (IngestTelemetryRequest request, TelemetryEngine engine, SensorRegistry registry) =>
        {
            if (!registry.Exists(request.DeviceMacAddress))
                registry.Register(request.DeviceMacAddress, request.Location, request.Category, request.Kind);

            if (request.Kind == TelemetryDataKind.Boolean)
            {
                engine.IngestBoolean(request.DeviceMacAddress, request.Location, request.Category, request.BooleanValue ?? false);
            }
            else
            {
                engine.IngestNumeric(request.DeviceMacAddress, request.Location, request.Category, request.Kind, request.NumericValue ?? 0);
            }

            return Results.Accepted();
        })
        .WithName("IngestTelemetry");

        group.MapGet("/tiles", (TelemetryEngine engine) => Results.Ok(engine.GetTiles()))
            .WithName("GetTelemetryTiles");

        group.MapGet("/alerts", (int? take, AlertFeed alerts) => Results.Ok(alerts.Recent(take ?? 30)))
            .WithName("GetAlerts");

        group.MapGet("/diagnostics/buffers", (TelemetryEngine engine, MockTelemetrySeeder seeder) => Results.Ok(new
        {
            jaggedArrayBuffer = engine.BufferDiagnostics(),
            multiDimensionalGrid = seeder.PowerGrid.Diagnostics("Facility B")
        }))
        .WithName("GetBufferDiagnostics");
    }
}
