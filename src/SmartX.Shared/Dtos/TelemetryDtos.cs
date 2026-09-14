namespace SmartX.Shared.Dtos;

/// <summary>
/// A single telemetry submission. Exactly one of <see cref="FloatValue"/>,
/// <see cref="IntValue"/> or <see cref="BoolValue"/> is populated, matching
/// <see cref="ValueType"/> ("float" | "int" | "bool"). This lets the wire
/// format stay simple JSON while the API rehydrates each value into a
/// strongly-typed <c>TelemetryPacket&lt;T&gt;</c> internally.
/// </summary>
public record TelemetryIngestRequest(
    string DeviceMac,
    string ValueType,
    float? FloatValue,
    int? IntValue,
    bool? BoolValue,
    double? CriticalThreshold);

/// <summary>One historical sample, flattened for charting on the client.</summary>
public record TelemetrySample(double Value, DateTimeOffset Timestamp);

/// <summary>Full ingestion history for a single device, returned to the dashboard.</summary>
public record TelemetryHistoryResponse(
    List<TelemetrySample> Floats,
    List<TelemetrySample> Ints,
    List<TelemetrySample> Bools,
    List<double> OptimisedHistory);
