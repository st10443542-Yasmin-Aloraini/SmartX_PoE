using System.Collections.Concurrent;
using SmartX.Shared.Domain;

namespace SmartX.Api.Services;

/// <summary>A capped, thread-safe feed of the most recent proactive alerts.</summary>
public sealed class AlertFeed
{
    private readonly ConcurrentQueue<EngagementAlert> _alerts = new();
    private const int Capacity = 200;
    private long _counter;

    public void Raise(string mac, EngagementState severity, string message)
    {
        var alert = new EngagementAlert
        {
            Id = Interlocked.Increment(ref _counter).ToString(),
            DeviceMacAddress = mac,
            Severity = severity,
            Message = message,
            RaisedAt = DateTimeOffset.UtcNow
        };

        _alerts.Enqueue(alert);
        while (_alerts.Count > Capacity && _alerts.TryDequeue(out _)) { }
    }

    public IReadOnlyList<EngagementAlert> Recent(int take = 30) =>
        _alerts.Reverse().Take(take).ToList();
}
