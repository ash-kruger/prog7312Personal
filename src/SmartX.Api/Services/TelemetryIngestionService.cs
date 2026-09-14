using System.Collections.Concurrent;
using SmartX.Api.Models;
using SmartX.Shared;

namespace SmartX.Api.Services;

/// <summary>
/// Central ingestion point for all telemetry. Keeps a per-device,
/// per-type history of <see cref="TelemetryPacket{T}"/> samples (using the
/// generic wrapper so a float moisture reading, an int wattage counter and
/// a bool valve state are each stored without boxing) and feeds every
/// numeric sample into that device's <see cref="HistoricalBatchBuffer"/>
/// for jagged-array-to-List optimisation.
/// </summary>
public sealed class TelemetryIngestionService
{
    private readonly ConcurrentDictionary<string, List<TelemetryPacket<float>>> _floatHistory = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<TelemetryPacket<int>>> _intHistory = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, List<TelemetryPacket<bool>>> _boolHistory = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, HistoricalBatchBuffer> _batchBuffers = new(StringComparer.OrdinalIgnoreCase);

    public void Ingest(TelemetryPacket<float> packet)
    {
        var list = _floatHistory.GetOrAdd(packet.DeviceMac, static _ => new List<TelemetryPacket<float>>());
        lock (list) { list.Add(packet); }
        BufferForBatch(packet.DeviceMac, packet.Value);
    }

    public void Ingest(TelemetryPacket<int> packet)
    {
        var list = _intHistory.GetOrAdd(packet.DeviceMac, static _ => new List<TelemetryPacket<int>>());
        lock (list) { list.Add(packet); }
        BufferForBatch(packet.DeviceMac, packet.Value);
    }

    public void Ingest(TelemetryPacket<bool> packet)
    {
        var list = _boolHistory.GetOrAdd(packet.DeviceMac, static _ => new List<TelemetryPacket<bool>>());
        lock (list) { list.Add(packet); }
    }

    private void BufferForBatch(string deviceMac, double value)
    {
        var buffer = _batchBuffers.GetOrAdd(deviceMac, static _ => new HistoricalBatchBuffer());
        buffer.IngestCycle(new[] { value });
    }

    public IReadOnlyList<TelemetryPacket<float>> GetFloatHistory(string deviceMac) =>
        _floatHistory.TryGetValue(deviceMac, out var list) ? list : Array.Empty<TelemetryPacket<float>>();

    public IReadOnlyList<TelemetryPacket<int>> GetIntHistory(string deviceMac) =>
        _intHistory.TryGetValue(deviceMac, out var list) ? list : Array.Empty<TelemetryPacket<int>>();

    public IReadOnlyList<TelemetryPacket<bool>> GetBoolHistory(string deviceMac) =>
        _boolHistory.TryGetValue(deviceMac, out var list) ? list : Array.Empty<TelemetryPacket<bool>>();

    public HistoricalBatchBuffer? GetBatchBuffer(string deviceMac) =>
        _batchBuffers.TryGetValue(deviceMac, out var buffer) ? buffer : null;
}
