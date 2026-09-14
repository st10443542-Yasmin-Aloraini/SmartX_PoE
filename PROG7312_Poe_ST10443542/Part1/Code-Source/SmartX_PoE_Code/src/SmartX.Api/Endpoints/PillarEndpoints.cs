using SmartX.Shared.Domain;

namespace SmartX.Api.Endpoints;

public static class PillarEndpoints
{
    public static void MapPillarEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/pillars", () => Results.Ok(new[]
        {
            new ArchitecturalPillar
            {
                Key = "ingestion",
                Title = "Sensor Data Ingestion and Telemetry",
                Description = "Register devices, stream mock/live telemetry, and watch the real-time engagement dashboard.",
                Enabled = true
            },
            new ArchitecturalPillar
            {
                Key = "command-stream",
                Title = "Real-Time Command Stream and History",
                Description = "Bidirectional command streaming and historical playback — arriving in Part 2 of the PoE.",
                Enabled = false
            },
            new ArchitecturalPillar
            {
                Key = "mesh-routing",
                Title = "Network Topology and Mesh Routing",
                Description = "Mesh topology visualisation and routing diagnostics — arriving in the final PoE.",
                Enabled = false
            }
        }))
        .WithName("GetPillars")
        .WithTags("Pillars");
    }
}
