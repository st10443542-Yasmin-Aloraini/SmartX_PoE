using SmartX.Shared.Trees;

namespace SmartX.Api.Endpoints;

public static class DeploymentTreeEndpoints
{
    public static void MapDeploymentTreeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/deployment-tree").WithTags("DeploymentTree");

        group.MapGet("/sample", () => Results.Ok(BuildSampleTree()))
            .WithName("GetSampleDeploymentTree");

        group.MapPost("/validate", (DeviceNode tree) =>
        {
            var result = DeploymentTreeValidator.Validate(tree);
            return Results.Ok(result);
        })
        .WithName("ValidateDeploymentTree");
    }

    /// <summary>Facility A -> Zone 1 -> Sub-Zone B -> Node, exactly as described in the brief,
    /// plus a deliberately broken branch so /validate has something to flag.</summary>
    private static DeviceNode BuildSampleTree() => new()
    {
        Name = "Facility A",
        Children =
        [
            new DeviceNode
            {
                Name = "Zone 1",
                Children =
                [
                    new DeviceNode
                    {
                        Name = "Sub-Zone A",
                        Children =
                        [
                            new DeviceNode { Name = "SOIL-01", IsLeafDevice = true, DeviceMacAddress = "SOIL-01" },
                            new DeviceNode { Name = "SOIL-02", IsLeafDevice = true, DeviceMacAddress = "SOIL-02" }
                        ]
                    },
                    new DeviceNode
                    {
                        Name = "Sub-Zone B",
                        Children =
                        [
                            new DeviceNode { Name = "SOIL-03", IsLeafDevice = true, DeviceMacAddress = "SOIL-03" },
                            // Deliberately invalid: leaf device missing its MAC address.
                            new DeviceNode { Name = "Unconfigured Node", IsLeafDevice = true }
                        ]
                    }
                ]
            },
            new DeviceNode
            {
                Name = "Zone 2",
                Children =
                [
                    new DeviceNode
                    {
                        Name = "Sub-Zone A",
                        Children = [new DeviceNode { Name = "SOIL-04", IsLeafDevice = true, DeviceMacAddress = "SOIL-04" }]
                    },
                    // Deliberately invalid: an empty zone with no children.
                    new DeviceNode { Name = "Sub-Zone B (empty)", Children = [] }
                ]
            }
        ]
    };
}
