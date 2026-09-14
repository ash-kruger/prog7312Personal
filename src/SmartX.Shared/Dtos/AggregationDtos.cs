namespace SmartX.Shared.Dtos;

/// <summary>
/// Requests the aggregate load and delta between two power meters, e.g.
/// Meter3 = Meter1 + Meter2, demonstrating operator-overloaded sensor math.
/// </summary>
public record MeterAggregationRequest(
    string DeviceMacA,
    double WattsA,
    string DeviceMacB,
    double WattsB);

public record MeterAggregationResponse(
    double AggregateWatts,
    double DeltaWatts,
    bool DeviceAHigherLoad);
