using System.Collections.Concurrent;
using SmartX.Shared.Domain;

namespace SmartX.Api.Services;

/// <summary>Thread-safe in-memory store of registered/simulated sensors and their attachments.</summary>
public sealed class SensorRegistry
{
    private readonly ConcurrentDictionary<string, SensorRegistration> _sensors = new(StringComparer.OrdinalIgnoreCase);

    public SensorRegistration Register(string mac, string location, SensorCategory category, TelemetryDataKind kind)
    {
        var sensor = new SensorRegistration
        {
            DeviceMacAddress = mac,
            Location = location,
            Category = category,
            DataKind = kind
        };

        _sensors[mac] = sensor;
        return sensor;
    }

    public bool TryGet(string mac, out SensorRegistration sensor) => _sensors.TryGetValue(mac, out sensor!);

    public IReadOnlyCollection<SensorRegistration> All() => _sensors.Values.ToList();

    public bool Exists(string mac) => _sensors.ContainsKey(mac);

    public void AddAttachment(string mac, SensorAttachment attachment)
    {
        if (_sensors.TryGetValue(mac, out var sensor))
        {
            lock (sensor.Attachments)
            {
                sensor.Attachments.Add(attachment);
            }
        }
    }
}
