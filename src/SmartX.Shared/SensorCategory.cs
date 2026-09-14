namespace SmartX.Shared;

/// <summary>
/// The category a registered sensor belongs to. Shared by the API (for
/// storage and validation) and the Blazor client (for the registration
/// form and dashboard filtering).
/// </summary>
public enum SensorCategory
{
    Environmental,
    PowerConsumption,
    Actuator
}
