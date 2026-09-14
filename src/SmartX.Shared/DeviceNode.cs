namespace SmartX.Shared;

/// <summary>
/// One node in a multi-tier device deployment hierarchy, for example
/// "Facility A" containing "Zone 1" containing "Sub-Zone B" containing a
/// physical sensor. The same shape is posted by the client when building a
/// hierarchy and walked recursively by the API's
/// <c>DeviceHierarchyValidator</c> to confirm every node is well-formed and
/// every device MAC address appears exactly once in the tree.
/// </summary>
public class DeviceNode
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Populated only on leaf nodes that represent an actual sensor.</summary>
    public string? DeviceMac { get; set; }

    public List<DeviceNode> Children { get; set; } = new();
}
