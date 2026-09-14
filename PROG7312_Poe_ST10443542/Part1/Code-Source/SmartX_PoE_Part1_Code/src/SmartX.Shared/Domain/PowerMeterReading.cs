namespace SmartX.Shared.Domain;

/// <summary>
/// A power-consumption aggregate for a smart meter. Operator overloads allow direct,
/// readable aggregation and delta comparison of sensor values in code, e.g.
/// <c>var combinedLoad = Meter1 + Meter2;</c> or <c>var drift = MeterNow - MeterBaseline;</c>,
/// exactly as required for the smart-grid "aggregate load of two smart meters" scenario.
/// </summary>
public readonly struct PowerMeterReading : IEquatable<PowerMeterReading>, IComparable<PowerMeterReading>
{
    public string MeterId { get; }
    public double WattageLoad { get; }
    public DateTimeOffset Timestamp { get; }

    public PowerMeterReading(string meterId, double wattageLoad, DateTimeOffset? timestamp = null)
    {
        MeterId = meterId;
        WattageLoad = wattageLoad;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    /// <summary>Aggregates two meters into a single combined-load reading (Meter3 = Meter1 + Meter2).</summary>
    public static PowerMeterReading operator +(PowerMeterReading a, PowerMeterReading b) =>
        new($"{a.MeterId}+{b.MeterId}", a.WattageLoad + b.WattageLoad, DateTimeOffset.UtcNow);

    /// <summary>Delta comparison between two readings — used by the engagement/alert engine
    /// to detect a sudden spike between consecutive samples from the same device.</summary>
    public static double operator -(PowerMeterReading a, PowerMeterReading b) => a.WattageLoad - b.WattageLoad;

    public static bool operator >(PowerMeterReading a, PowerMeterReading b) => a.WattageLoad > b.WattageLoad;
    public static bool operator <(PowerMeterReading a, PowerMeterReading b) => a.WattageLoad < b.WattageLoad;
    public static bool operator >=(PowerMeterReading a, PowerMeterReading b) => a.WattageLoad >= b.WattageLoad;
    public static bool operator <=(PowerMeterReading a, PowerMeterReading b) => a.WattageLoad <= b.WattageLoad;

    public static bool operator ==(PowerMeterReading a, PowerMeterReading b) => a.Equals(b);
    public static bool operator !=(PowerMeterReading a, PowerMeterReading b) => !a.Equals(b);

    public bool Equals(PowerMeterReading other) =>
        MeterId == other.MeterId && WattageLoad.Equals(other.WattageLoad);

    public override bool Equals(object? obj) => obj is PowerMeterReading other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(MeterId, WattageLoad);

    public int CompareTo(PowerMeterReading other) => WattageLoad.CompareTo(other.WattageLoad);

    public override string ToString() => $"{MeterId}: {WattageLoad:F2} W @ {Timestamp:O}";
}
