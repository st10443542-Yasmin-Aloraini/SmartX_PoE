namespace SmartX.Shared.Domain;

/// <summary>A user-registered/simulated sensor record (Sensor Payload Management requirement).</summary>
public sealed class SensorRegistration
{
    public required string DeviceMacAddress { get; init; }
    public required string Location { get; set; }
    public required SensorCategory Category { get; set; }
    public TelemetryDataKind DataKind { get; set; } = TelemetryDataKind.Float;
    public DateTimeOffset RegisteredAt { get; init; } = DateTimeOffset.UtcNow;
    public List<SensorAttachment> Attachments { get; init; } = new();
}

/// <summary>A physical device configuration file, deployment photo, or hardware log
/// attached to a specific sensor profile (Media/Log Attachment requirement).</summary>
public sealed class SensorAttachment
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long SizeBytes { get; init; }
    public required string StoragePath { get; init; }
    public DateTimeOffset UploadedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>Snapshot of a sensor's current dashboard tile state, sent to the client.</summary>
public sealed class SensorTileSnapshot
{
    public required string DeviceMacAddress { get; init; }
    public required string Location { get; init; }
    public required SensorCategory Category { get; init; }
    public required TelemetryDataKind DataKind { get; init; }
    public required EngagementState State { get; init; }
    public required double LatestNumericValue { get; init; }
    public bool? LatestBooleanValue { get; init; }
    public required DateTimeOffset LastSeen { get; init; }
    public required double[] Sparkline { get; init; }
    public int Streak { get; init; }
}

/// <summary>A single escalated, de-duplicated alert for the live alert feed.</summary>
public sealed class EngagementAlert
{
    public required string Id { get; init; }
    public required string DeviceMacAddress { get; init; }
    public required EngagementState Severity { get; init; }
    public required string Message { get; init; }
    public required DateTimeOffset RaisedAt { get; init; }
}

/// <summary>One of the three architectural pillars shown on the Smart-X landing page.</summary>
public sealed class ArchitecturalPillar
{
    public required string Key { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required bool Enabled { get; init; }
}
