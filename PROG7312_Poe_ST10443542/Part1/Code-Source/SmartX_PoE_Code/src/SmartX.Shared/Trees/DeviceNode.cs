namespace SmartX.Shared.Trees;

/// <summary>
/// A node in a multi-tier device deployment / configuration profile tree, e.g.
/// Facility A -&gt; Zone 1 -&gt; Sub-Zone B -&gt; Node. Non-leaf nodes group child nodes;
/// leaf nodes represent an actual physical device and must carry a MAC address.
/// </summary>
public sealed class DeviceNode
{
    public required string Name { get; set; }
    public string? DeviceMacAddress { get; set; }
    public bool IsLeafDevice { get; set; }
    public List<DeviceNode> Children { get; set; } = new();
}

public sealed record ValidationIssue(string Path, string Message);

public sealed record TreeValidationResult(bool IsValid, IReadOnlyList<ValidationIssue> Issues, int NodesVisited, int MaxDepthReached);

/// <summary>
/// Recursively walks a nested device deployment tree to confirm every node is safely
/// configured, e.g. that a node nested within Sub-Zone B -&gt; Zone 1 -&gt; Facility A has a
/// unique MAC address, sits within the maximum allowed nesting depth, and that every
/// branch terminates correctly (leaves carry a device, zones are never empty).
/// </summary>
public static class DeploymentTreeValidator
{
    private const int MaxDepth = 6;

    public static TreeValidationResult Validate(DeviceNode root)
    {
        var issues = new List<ValidationIssue>();
        var seenMacs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nodesVisited = 0;
        var maxDepthReached = 0;

        ValidateRecursive(root, depth: 0, path: root.Name, issues, seenMacs, ref nodesVisited, ref maxDepthReached);

        return new TreeValidationResult(issues.Count == 0, issues, nodesVisited, maxDepthReached);
    }

    private static void ValidateRecursive(
        DeviceNode node,
        int depth,
        string path,
        List<ValidationIssue> issues,
        HashSet<string> seenMacs,
        ref int nodesVisited,
        ref int maxDepthReached)
    {
        nodesVisited++;
        maxDepthReached = Math.Max(maxDepthReached, depth);

        // Base case 1: a runaway/misconfigured tree that nests too deeply.
        if (depth > MaxDepth)
        {
            issues.Add(new ValidationIssue(path, $"Maximum nesting depth of {MaxDepth} exceeded."));
            return;
        }

        if (string.IsNullOrWhiteSpace(node.Name))
            issues.Add(new ValidationIssue(path, "Node has no name."));

        // Base case 2: a leaf device. Validate it and stop recursing on this branch.
        if (node.IsLeafDevice)
        {
            if (string.IsNullOrWhiteSpace(node.DeviceMacAddress))
            {
                issues.Add(new ValidationIssue(path, "Leaf device node is missing a MAC address."));
            }
            else if (!seenMacs.Add(node.DeviceMacAddress))
            {
                issues.Add(new ValidationIssue(path, $"Duplicate MAC address '{node.DeviceMacAddress}' found elsewhere in the tree."));
            }

            if (node.Children.Count > 0)
                issues.Add(new ValidationIssue(path, "Leaf device node must not have child nodes."));

            return;
        }

        // Base case 3: a group/zone node with nothing inside it.
        if (node.Children.Count == 0)
        {
            issues.Add(new ValidationIssue(path, "Non-leaf node (zone/facility) has no children."));
            return;
        }

        // Recursive case: descend into every child (e.g. Facility A -> Zone 1 -> Sub-Zone B -> Node).
        foreach (var child in node.Children)
        {
            ValidateRecursive(child, depth + 1, $"{path} -> {child.Name}", issues, seenMacs, ref nodesVisited, ref maxDepthReached);
        }
    }
}
