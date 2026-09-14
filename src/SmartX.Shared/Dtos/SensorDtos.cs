namespace SmartX.Shared.Dtos;

/// <summary>Request body for registering a new sensor with the gateway.</summary>
public record SensorRegistrationRequest(
    string DeviceMac,
    string DeploymentLocation,
    SensorCategory Category);

/// <summary>Server response describing a registered sensor's current state.</summary>
public record SensorRegistrationResponse(
    string DeviceMac,
    string DeploymentLocation,
    SensorCategory Category,
    DateTimeOffset RegisteredAt,
    List<string> AttachedFiles);
