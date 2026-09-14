namespace SmartX.Api.Models;

/// <summary>
/// A single numeric power-meter reading. Operators are overloaded so
/// application code can aggregate or compare meters directly, e.g.
/// <c>Meter3 = Meter1 + Meter2</c> for a combined load, or
/// <c>Meter1 - Meter2</c> to compute the delta used by anomaly checks.
/// </summary>
public readonly struct MeterReading : IEquatable<MeterReading>
{
    public string DeviceMac { get; }
    public double Watts { get; }
    public DateTimeOffset Timestamp { get; }

    public MeterReading(string deviceMac, double watts, DateTimeOffset? timestamp = null)
    {
        DeviceMac = deviceMac;
        Watts = watts;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    /// <summary>Aggregates two meters into a combined virtual load, e.g. Meter3 = Meter1 + Meter2.</summary>
    public static MeterReading operator +(MeterReading a, MeterReading b) =>
        new($"{a.DeviceMac}+{b.DeviceMac}", a.Watts + b.Watts);

    /// <summary>Computes the delta between two meters, used for spike/drop detection.</summary>
    public static MeterReading operator -(MeterReading a, MeterReading b) =>
        new($"{a.DeviceMac}-{b.DeviceMac}", a.Watts - b.Watts);

    public static bool operator >(MeterReading a, MeterReading b) => a.Watts > b.Watts;
    public static bool operator <(MeterReading a, MeterReading b) => a.Watts < b.Watts;
    public static bool operator >=(MeterReading a, MeterReading b) => a.Watts >= b.Watts;
    public static bool operator <=(MeterReading a, MeterReading b) => a.Watts <= b.Watts;

    public static bool operator ==(MeterReading a, MeterReading b) => a.Equals(b);
    public static bool operator !=(MeterReading a, MeterReading b) => !a.Equals(b);

    public bool Equals(MeterReading other) =>
        DeviceMac == other.DeviceMac && Watts.Equals(other.Watts);

    public override bool Equals(object? obj) => obj is MeterReading other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(DeviceMac, Watts);

    public override string ToString() => $"{DeviceMac}: {Watts:F2} W @ {Timestamp:HH:mm:ss}";
}
