using SmartX.Shared.Buffers;
using SmartX.Shared.Domain;

namespace SmartX.Api.Services;

/// <summary>
/// Owns the fixed-size, rectangular <c>int[,]</c> historical grid (meters x time-slots) for
/// the simulated power meters, and periodically compacts it into an optimised
/// <see cref="List{T}"/> of <see cref="TelemetryPacket{T}"/> — the multi-dimensional-array
/// half of the "advanced arrays and lists" requirement (the jagged-array half lives in
/// <see cref="TelemetryEngine"/> for uneven-arrival environmental samples).
/// </summary>
public sealed class PowerGridDiagnostics
{
    private readonly PowerLoadHistoryGrid _grid;
    private readonly object _lock = new();
    private int _lastCompactedCount;

    public PowerGridDiagnostics(string[] meterIds, int timeSlots = 60)
    {
        _grid = new PowerLoadHistoryGrid(meterIds, timeSlots);
    }

    public void RecordSlot(int[] wattagePerMeter)
    {
        lock (_lock)
        {
            _grid.RecordSlot(wattagePerMeter);
        }
    }

    public object Diagnostics(string location)
    {
        lock (_lock)
        {
            var optimised = _grid.CompactToOptimisedList(location);
            _lastCompactedCount = optimised.Count;
            return new
            {
                meters = _grid.MeterCount,
                timeSlots = _grid.TimeSlots,
                gridCells = _grid.MeterCount * _grid.TimeSlots,
                optimisedListLength = _lastCompactedCount,
                note = "Fixed-schedule power-meter loads are recorded into a rectangular int[,] grid " +
                       "(meters x time-slots) then compacted into an optimised List<TelemetryPacket<int>>."
            };
        }
    }
}
