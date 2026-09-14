using SmartX.Api.Contracts;
using SmartX.Api.Services;
using SmartX.Shared.Domain;

namespace SmartX.Api.Endpoints;

public static class SensorEndpoints
{
    public static void MapSensorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sensors").WithTags("Sensors");

        group.MapGet("/", (SensorRegistry registry) => Results.Ok(registry.All()))
            .WithName("ListSensors");

        group.MapPost("/", (RegisterSensorRequest request, SensorRegistry registry, TelemetryEngine engine) =>
        {
            if (string.IsNullOrWhiteSpace(request.DeviceMacAddress))
                return Results.BadRequest("A device MAC address / unique identifier is required.");

            var sensor = registry.Register(request.DeviceMacAddress, request.Location, request.Category, request.DataKind);

            // Seed one immediate reading so the sensor shows up as a live tile straight away.
            // Without this, a freshly registered sensor sits in the registry but never appears
            // on the dashboard, since GetTiles() only reflects sensors that have received
            // telemetry via IngestNumeric/IngestBoolean, and the background seeder (below)
            // now also picks it up on every subsequent cycle so it keeps updating like any
            // other tile.
            if (request.DataKind == TelemetryDataKind.Boolean)
            {
                engine.IngestBoolean(sensor.DeviceMacAddress, sensor.Location, sensor.Category, false);
            }
            else
            {
                var baseline = request.Category == SensorCategory.Environmental ? 45.0 : 22.0;
                engine.IngestNumeric(sensor.DeviceMacAddress, sensor.Location, sensor.Category, request.DataKind, baseline);
            }

            return Results.Created($"/api/sensors/{sensor.DeviceMacAddress}", sensor);
        })
        .WithName("RegisterSensor");

        group.MapGet("/{mac}", (string mac, SensorRegistry registry) =>
            registry.TryGet(mac, out var sensor) ? Results.Ok(sensor) : Results.NotFound())
            .WithName("GetSensor");

        // Media/Log Attachment requirement: an API-based multipart file uploader for device
        // configuration files, deployment photos, or hardware logs.
        group.MapPost("/{mac}/attachments", async (string mac, IFormFile file, SensorRegistry registry, FileStorageService storage) =>
        {
            if (!registry.Exists(mac))
                return Results.NotFound($"No sensor registered with MAC '{mac}'.");

            if (file.Length == 0)
                return Results.BadRequest("Uploaded file is empty.");

            var attachment = await storage.SaveAsync(mac, file);
            registry.AddAttachment(mac, attachment);
            return Results.Ok(attachment);
        })
        .DisableAntiforgery()
        .WithName("UploadSensorAttachment");

        group.MapGet("/{mac}/attachments", (string mac, SensorRegistry registry) =>
            registry.TryGet(mac, out var sensor) ? Results.Ok(sensor.Attachments) : Results.NotFound())
            .WithName("ListSensorAttachments");
    }
}
