using SmartX.Shared.Domain;

namespace SmartX.Api.Contracts;

public sealed record RegisterSensorRequest(string DeviceMacAddress, string Location, SensorCategory Category, TelemetryDataKind DataKind);

public sealed record IngestTelemetryRequest(
    string DeviceMacAddress,
    string Location,
    SensorCategory Category,
    TelemetryDataKind Kind,
    double? NumericValue,
    bool? BooleanValue);
