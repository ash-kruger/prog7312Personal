using SmartX.Shared;

namespace SmartX.Api.Models;

/// <summary>Server-side record of a registered sensor and any files attached to it.</summary>
public class SensorRegistration
{
    public required string DeviceMac { get; init; }
    public required string DeploymentLocation { get; init; }
    public required SensorCategory Category { get; init; }
    public DateTimeOffset RegisteredAt { get; init; } = DateTimeOffset.UtcNow;
    public List<string> AttachedFiles { get; } = new();
}
