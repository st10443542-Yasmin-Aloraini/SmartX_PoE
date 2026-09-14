namespace SmartX.Shared.Domain;

/// <summary>
/// High-level classification of a registered sensor/actuator, as required by the
/// Sensor Payload Management functional requirement.
/// </summary>
public enum SensorCategory
{
    Environmental,
    PowerConsumption,
    Actuator
}

/// <summary>
/// The concrete telemetry data shape published by a device. Kept distinct from the
/// generic type parameter used on <see cref="TelemetryPacket{T}"/> so the API can
/// describe *what kind* of value a packet is carrying without reflection.
/// </summary>
public enum TelemetryDataKind
{
    Float,   // e.g. soil moisture, temperature
    Integer, // e.g. power wattage
    Boolean  // e.g. valve/relay state
}

/// <summary>
/// The health/engagement state a sensor tile is rendered in on the dashboard.
/// This is the core of the "Dynamic Engagement Feature": real-time visual feedback
/// with proactive, severity-tiered alert escalation (see Task 1 research report).
/// </summary>
public enum EngagementState
{
    Normal,
    Warning,
    Critical,
    Disconnected
}
