using SmartX.Shared.Domain;

namespace SmartX.Shared.Buffers;

/// <summary>
/// Buffers raw, per-cycle telemetry samples in a <b>jagged array</b> (<c>float[][]</c>)
/// before they are transferred into an optimised <see cref="List{T}"/>. A jagged array is
/// used rather than a rectangular 2D array because not every device reports on every
/// ingestion cycle, so each "row" (cycle) genuinely has a different number of raw samples —
/// exactly the high-throughput, uneven-arrival pattern thousands of ESP32 devices produce.
/// </summary>
public sealed class RawTelemetryBatchBuffer
{
    private float[][] _rawCycles;
    private int _cycleCount;

    public RawTelemetryBatchBuffer(int initialCapacity = 16)
    {
        if (initialCapacity < 1) initialCapacity = 1;
        _rawCycles = new float[initialCapacity][];
    }

    public int CycleCount => _cycleCount;

    /// <summary>Appends one ingestion cycle's raw samples (a single jagged-array row).</summary>
    public void AppendCycle(float[] cycleSamples)
    {
        if (_cycleCount == _rawCycles.Length)
        {
            Array.Resize(ref _rawCycles, _rawCycles.Length * 2);
        }

        _rawCycles[_cycleCount++] = cycleSamples;
    }

    /// <summary>Read-only snapshot of the raw jagged buffer, trimmed to the cycles actually used.</summary>
    public float[][] RawSnapshot() => _rawCycles[.._cycleCount];

    /// <summary>
    /// Flattens every buffered jagged-array cycle into a single, pre-sized
    /// <see cref="List{T}"/>. This is the "transfer into optimised Collections" step:
    /// once the bursty per-tick ingestion has settled, downstream aggregation/LINQ/sparkline
    /// code works against a flat, cache-friendly list instead of a ragged array-of-arrays.
    /// </summary>
    public List<float> FlattenToOptimisedList()
    {
        var total = 0;
        for (var i = 0; i < _cycleCount; i++) total += _rawCycles[i].Length;

        var optimised = new List<float>(total);
        for (var i = 0; i < _cycleCount; i++)
        {
            optimised.AddRange(_rawCycles[i]);
        }

        return optimised;
    }

    public void Clear() => _cycleCount = 0;
}

/// <summary>
/// Buffers historical power-meter loads in a fixed <b>multi-dimensional array</b>
/// (<c>int[,]</c>, meters x time-slots) — appropriate here because every registered
/// power meter is sampled on the same fixed schedule, giving a genuinely rectangular grid —
/// before compacting the grid into an optimised <see cref="List{T}"/> of
/// <see cref="TelemetryPacket{T}"/> values for storage and transmission.
/// </summary>
public sealed class PowerLoadHistoryGrid
{
    private readonly int[,] _grid;
    private readonly string[] _meterIds;
    private int _nextSlot;

    public PowerLoadHistoryGrid(string[] meterIds, int timeSlots)
    {
        _meterIds = meterIds;
        _grid = new int[meterIds.Length, timeSlots];
    }

    public int TimeSlots => _grid.GetLength(1);
    public int MeterCount => _grid.GetLength(0);

    public void RecordSlot(int[] wattageForEachMeter)
    {
        if (wattageForEachMeter.Length != MeterCount)
            throw new ArgumentException("Sample count must match the number of registered meters.");

        var slot = _nextSlot % TimeSlots;
        for (var m = 0; m < MeterCount; m++)
        {
            _grid[m, slot] = wattageForEachMeter[m];
        }

        _nextSlot++;
    }

    /// <summary>Compacts the fixed 2D grid into an optimised list of typed telemetry packets.</summary>
    public List<TelemetryPacket<int>> CompactToOptimisedList(string location)
    {
        var filledSlots = Math.Min(_nextSlot, TimeSlots);
        var list = new List<TelemetryPacket<int>>(MeterCount * filledSlots);

        for (var m = 0; m < MeterCount; m++)
        {
            for (var s = 0; s < filledSlots; s++)
            {
                list.Add(new TelemetryPacket<int>(
                    _meterIds[m], location, SensorCategory.PowerConsumption,
                    TelemetryDataKind.Integer, _grid[m, s]));
            }
        }

        return list;
    }
}
