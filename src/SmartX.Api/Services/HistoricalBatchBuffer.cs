namespace SmartX.Api.Services;

/// <summary>
/// Buffers raw incoming telemetry as a jagged array — one <c>double[]</c>
/// row per ingestion cycle, where rows can (and typically do) have
/// different lengths, mirroring how a fast-polling moisture sensor and a
/// slow-polling power meter would flush different numbers of readings per
/// cycle. Each completed cycle is then transferred into a single optimised
/// <see cref="List{T}"/> so downstream code (history queries, charting)
/// never has to walk the raw jagged structure directly.
/// </summary>
public sealed class HistoricalBatchBuffer
{
    // Jagged array: double[][] where each row is independently sized.
    private double[][] _rawCycles = Array.Empty<double[]>();
    private readonly List<double> _optimisedHistory = new();

    public IReadOnlyList<double> OptimisedHistory => _optimisedHistory;

    public IReadOnlyList<double[]> RawCycles => _rawCycles;

    /// <summary>Appends one ingestion cycle's raw readings as a new jagged-array row.</summary>
    public void IngestCycle(double[] readingsThisCycle)
    {
        var grown = new double[_rawCycles.Length + 1][];
        Array.Copy(_rawCycles, grown, _rawCycles.Length);
        grown[^1] = readingsThisCycle;
        _rawCycles = grown;

        TransferToOptimisedList(readingsThisCycle);
    }

    private void TransferToOptimisedList(double[] cycle)
    {
        foreach (var reading in cycle)
        {
            _optimisedHistory.Add(reading);
        }
    }
}
