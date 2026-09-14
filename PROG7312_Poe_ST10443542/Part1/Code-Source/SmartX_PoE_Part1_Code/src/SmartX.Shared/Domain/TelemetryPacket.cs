namespace SmartX.Shared.Domain;

/// <summary>
/// Non-generic view of a telemetry packet, used only at API/serialisation boundaries
/// (e.g. building a JSON-friendly DTO). Ingestion, validation and storage all operate
/// on the fully generic <see cref="TelemetryPacket{T}"/> directly, so the hot path
/// never boxes the payload value.
/// </summary>
public interface ITelemetryPacket
{
    string DeviceMacAddress { get; }
    string Location { get; }
    SensorCategory Category { get; }
    TelemetryDataKind Kind { get; }
    DateTimeOffset Timestamp { get; }
}

/// <summary>
/// A reusable generic wrapper for disparate incoming sensor payloads (float soil-moisture
/// readings, int power-wattage readings, bool valve/switch states, ...). Declaring it as a
/// generic <c>readonly struct</c> constrained to <c>struct</c> lets the gateway handle every
/// payload shape uniformly through one type definition while the CLR generates a specialised,
/// value-type implementation per closed generic (TelemetryPacket&lt;float&gt;,
/// TelemetryPacket&lt;int&gt;, TelemetryPacket&lt;bool&gt;, ...): there is no shared
/// "object Value" field anywhere on the ingestion/storage path, so no boxing/unboxing occurs
/// when packets are created, validated, aggregated or stored in a typed List&lt;T&gt;.
/// </summary>
/// <typeparam name="T">The unmanaged value type carried by this packet (float, int, bool, ...).</typeparam>
public readonly struct TelemetryPacket<T> : ITelemetryPacket, IEquatable<TelemetryPacket<T>>
    where T : struct
{
    public string DeviceMacAddress { get; }
    public string Location { get; }
    public SensorCategory Category { get; }
    public TelemetryDataKind Kind { get; }
    public DateTimeOffset Timestamp { get; }
    public T Value { get; }

    public TelemetryPacket(
        string deviceMacAddress,
        string location,
        SensorCategory category,
        TelemetryDataKind kind,
        T value,
        DateTimeOffset? timestamp = null)
    {
        if (string.IsNullOrWhiteSpace(deviceMacAddress))
            throw new ArgumentException("Device MAC address is required.", nameof(deviceMacAddress));

        DeviceMacAddress = deviceMacAddress;
        Location = location;
        Category = category;
        Kind = kind;
        Value = value;
        Timestamp = timestamp ?? DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Boxing happens only here, deliberately, at the point a packet must cross into a
    /// weakly-typed shape (e.g. a JSON response DTO) — never during ingestion or storage.
    /// </summary>
    public object BoxedValue() => Value;

    public bool Equals(TelemetryPacket<T> other) =>
        DeviceMacAddress == other.DeviceMacAddress &&
        Timestamp == other.Timestamp &&
        EqualityComparer<T>.Default.Equals(Value, other.Value);

    public override bool Equals(object? obj) => obj is TelemetryPacket<T> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(DeviceMacAddress, Timestamp, Value);

    public override string ToString() =>
        $"[{Timestamp:HH:mm:ss.fff}] {DeviceMacAddress} ({Location}, {Category}) {Kind}={Value}";
}
