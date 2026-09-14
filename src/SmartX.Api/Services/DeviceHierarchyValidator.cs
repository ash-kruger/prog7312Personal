using SmartX.Shared;

namespace SmartX.Api.Services;

/// <summary>
/// Recursively validates a nested device deployment tree, e.g.
/// Facility A -&gt; Zone 1 -&gt; Sub-Zone B -&gt; sensor. Recursion is the
/// natural fit here because the tree can be arbitrarily deep and each
/// node is validated the same way as its parent — a loop with a manual
/// stack would only reimplement what the call stack already gives us.
/// </summary>
public sealed class DeviceHierarchyValidator
{
    // Guards against pathological or accidentally-circular input trees;
    // this is what turns the recursion into a safely-terminating algorithm.
    private const int MaxDepth = 12;

    public (bool IsValid, List<string> Errors) Validate(DeviceNode root)
    {
        var errors = new List<string>();
        var seenMacs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        ValidateNode(root, depth: 0, seenMacs, errors);

        return (errors.Count == 0, errors);
    }

    private void ValidateNode(DeviceNode node, int depth, HashSet<string> seenMacs, List<string> errors)
    {
        // Base case: stop recursing once the tree is deeper than any
        // legitimate facility/zone/sub-zone/sensor path could be.
        if (depth > MaxDepth)
        {
            errors.Add($"Node '{node.Name}' exceeds the maximum nesting depth of {MaxDepth}.");
            return;
        }

        if (string.IsNullOrWhiteSpace(node.Name))
        {
            errors.Add($"A node at depth {depth} is missing a name.");
        }

        if (!string.IsNullOrWhiteSpace(node.DeviceMac) && !seenMacs.Add(node.DeviceMac))
        {
            errors.Add($"Duplicate device MAC '{node.DeviceMac}' found under '{node.Name}'.");
        }

        // Recursive case: each child sub-tree is validated exactly like the root.
        foreach (var child in node.Children)
        {
            ValidateNode(child, depth + 1, seenMacs, errors);
        }
    }
}
