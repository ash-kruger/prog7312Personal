using SmartX.Shared;

namespace SmartX.Api.Models;

/// <summary>
/// Generic wrapper that carries a single strongly-typed telemetry sample —
/// a <c>float</c> for environmental readings (e.g. soil moisture), an
/// <c>int</c> for power/wattage counters, or a <c>bool</c> for actuator and
/// valve states — without ever boxing the payload into <c>object</c>.
/// Because <typeparamref name="T"/> stays a real generic type parameter,
/// the CLR generates a specialised value-type implementation per closed
/// generic (TelemetryPacket&lt;float&gt;, TelemetryPacket&lt;int&gt;,
/// TelemetryPacket&lt;bool&gt;) instead of allocating a boxed object for
/// every sample on a resource-constrained gateway device.
/// </summary>
/// <typeparam name="T">
/// A value type — enforced by the <c>struct</c> constraint so the wrapper
/// can never accidentally be used with a reference type that would defeat
/// the whole point of avoiding boxing.
/// </typeparam>
public sealed class TelemetryPacket<T> where T : struct
{
    public string DeviceMac { get; }
    public SensorCategory Category { get; }
    public T Value { get; }
    public DateTimeOffset Timestamp { get; }

    public TelemetryPacket(string deviceMac, SensorCategory category, T value, DateTimeOffset? timestamp = null)
    {
        DeviceMac = deviceMac;
        Category = category;
        Value = value;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    public override string ToString() => $"[{Timestamp:O}] {DeviceMac} ({Category}) = {Value}";
}
