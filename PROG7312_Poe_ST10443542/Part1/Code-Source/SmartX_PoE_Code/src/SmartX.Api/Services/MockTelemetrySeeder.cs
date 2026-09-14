using SmartX.Shared.Domain;

namespace SmartX.Api.Services;

/// <summary>
/// Heavily seeds the gateway with simulated ESP32 telemetry so the data structures
/// (generic packets, jagged/2D array buffers, the engagement/alert engine) can be shown
/// working under continuous, high-throughput load without requiring physical hardware.
/// Deliberately injects occasional spikes and one simulated disconnect so the dynamic
/// engagement feature has something real to react to.
/// </summary>
public sealed class MockTelemetrySeeder : BackgroundService
{
    private readonly TelemetryEngine _engine;
    private readonly SensorRegistry _registry;
    private readonly PowerGridDiagnostics _powerGrid;
    private readonly Random _random = new();

    private readonly (string Mac, string Location, SensorCategory Category, TelemetryDataKind Kind)[] _devices;
    private readonly string[] _powerMeterIds;

    public MockTelemetrySeeder(TelemetryEngine engine, SensorRegistry registry)
    {
        _engine = engine;
        _registry = registry;

        _devices = BuildDeviceRoster();
        _powerMeterIds = _devices.Where(d => d.Category == SensorCategory.PowerConsumption).Select(d => d.Mac).ToArray();
        _powerGrid = new PowerGridDiagnostics(_powerMeterIds.Length == 0 ? new[] { "PWR-NONE" } : _powerMeterIds);
    }

    public PowerGridDiagnostics PowerGrid => _powerGrid;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        foreach (var d in _devices)
        {
            if (!_registry.Exists(d.Mac))
                _registry.Register(d.Mac, d.Location, d.Category, d.Kind);
        }

        var cycle = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            cycle++;
            var powerWattages = new int[_powerMeterIds.Length];
            var powerIdx = 0;

            foreach (var d in _devices)
            {
                // Simulate device "SOIL-07" going silent for a while to exercise disconnect detection.
                if (d.Mac == "SOIL-07" && cycle % 40 is > 8 and < 24)
                    continue;

                switch (d.Kind)
                {
                    case TelemetryDataKind.Float:
                        var baseline = d.Category == SensorCategory.Environmental ? 45.0 : 22.0;
                        var noise = (_random.NextDouble() - 0.5) * 4.0;
                        var spike = _random.NextDouble() < 0.04 ? (_random.NextDouble() < 0.5 ? -1 : 1) * _random.Next(25, 60) : 0;
                        var value = Math.Round(baseline + noise + spike, 2);
                        _engine.IngestNumeric(d.Mac, d.Location, d.Category, d.Kind, value);
                        break;

                    case TelemetryDataKind.Integer:
                        var wattage = 300 + _random.Next(-20, 20);
                        if (_random.NextDouble() < 0.04) wattage += _random.Next(150, 400);
                        _engine.IngestNumeric(d.Mac, d.Location, d.Category, d.Kind, wattage);
                        if (powerIdx < powerWattages.Length) powerWattages[powerIdx++] = wattage;
                        break;

                    case TelemetryDataKind.Boolean:
                        var isOpen = _random.NextDouble() < 0.5;
                        _engine.IngestBoolean(d.Mac, d.Location, d.Category, isOpen);
                        break;
                }
            }

            // Also keep simulating any sensor added later through the registration form/API.
            // Without this, a user-registered sensor only ever gets the single seed reading
            // taken at registration time and then goes silent, so it gets marked Disconnected
            // a few seconds later instead of behaving like a normal live tile.
            foreach (var sensor in _registry.All())
            {
                var isFixedDevice = _devices.Any(d =>
                    string.Equals(d.Mac, sensor.DeviceMacAddress, StringComparison.OrdinalIgnoreCase));
                if (isFixedDevice)
                    continue;

                switch (sensor.DataKind)
                {
                    case TelemetryDataKind.Float:
                        var baseline = sensor.Category == SensorCategory.Environmental ? 45.0 : 22.0;
                        var noise = (_random.NextDouble() - 0.5) * 4.0;
                        var value = Math.Round(baseline + noise, 2);
                        _engine.IngestNumeric(sensor.DeviceMacAddress, sensor.Location, sensor.Category, sensor.DataKind, value);
                        break;

                    case TelemetryDataKind.Integer:
                        var wattage = 300 + _random.Next(-20, 20);
                        _engine.IngestNumeric(sensor.DeviceMacAddress, sensor.Location, sensor.Category, sensor.DataKind, wattage);
                        break;

                    case TelemetryDataKind.Boolean:
                        _engine.IngestBoolean(sensor.DeviceMacAddress, sensor.Location, sensor.Category, _random.NextDouble() < 0.5);
                        break;
                }
            }

            if (powerWattages.Length > 0)
                _powerGrid.RecordSlot(powerWattages);

            if (cycle % 5 == 0)
                _engine.SweepDisconnects();

            await Task.Delay(TimeSpan.FromSeconds(1.5), stoppingToken).ContinueWith(_ => { }, TaskScheduler.Default);
        }
    }

    private static (string, string, SensorCategory, TelemetryDataKind)[] BuildDeviceRoster() =>
    [
        ("SOIL-01", "Facility A -> Zone 1 -> Sub-Zone A -> Bed 1", SensorCategory.Environmental, TelemetryDataKind.Float),
        ("SOIL-02", "Facility A -> Zone 1 -> Sub-Zone A -> Bed 2", SensorCategory.Environmental, TelemetryDataKind.Float),
        ("SOIL-03", "Facility A -> Zone 1 -> Sub-Zone B -> Bed 1", SensorCategory.Environmental, TelemetryDataKind.Float),
        ("SOIL-04", "Facility A -> Zone 2 -> Sub-Zone A -> Bed 1", SensorCategory.Environmental, TelemetryDataKind.Float),
        ("SOIL-05", "Facility A -> Zone 2 -> Sub-Zone A -> Bed 2", SensorCategory.Environmental, TelemetryDataKind.Float),
        ("SOIL-06", "Facility A -> Zone 2 -> Sub-Zone B -> Bed 1", SensorCategory.Environmental, TelemetryDataKind.Float),
        ("SOIL-07", "Facility A -> Zone 1 -> Sub-Zone A -> Bed 3", SensorCategory.Environmental, TelemetryDataKind.Float),
        ("TEMP-01", "Facility A -> Zone 1 -> Greenhouse", SensorCategory.Environmental, TelemetryDataKind.Float),
        ("TEMP-02", "Facility A -> Zone 2 -> Greenhouse", SensorCategory.Environmental, TelemetryDataKind.Float),
        ("PWR-01", "Facility B -> Grid Zone 1 -> Node 1", SensorCategory.PowerConsumption, TelemetryDataKind.Integer),
        ("PWR-02", "Facility B -> Grid Zone 1 -> Node 2", SensorCategory.PowerConsumption, TelemetryDataKind.Integer),
        ("PWR-03", "Facility B -> Grid Zone 2 -> Node 1", SensorCategory.PowerConsumption, TelemetryDataKind.Integer),
        ("PWR-04", "Facility B -> Grid Zone 2 -> Node 2", SensorCategory.PowerConsumption, TelemetryDataKind.Integer),
        ("PWR-05", "Facility B -> Grid Zone 3 -> Node 1", SensorCategory.PowerConsumption, TelemetryDataKind.Integer),
        ("VALVE-01", "Facility A -> Zone 1 -> Sub-Zone A -> Bed 1", SensorCategory.Actuator, TelemetryDataKind.Boolean),
        ("VALVE-02", "Facility A -> Zone 1 -> Sub-Zone B -> Bed 1", SensorCategory.Actuator, TelemetryDataKind.Boolean),
        ("VALVE-03", "Facility A -> Zone 2 -> Sub-Zone A -> Bed 1", SensorCategory.Actuator, TelemetryDataKind.Boolean),
        ("RELAY-01", "Facility B -> Grid Zone 1 -> Node 1", SensorCategory.Actuator, TelemetryDataKind.Boolean),
    ];
}
