// SmartX.SelfCheck — a zero-dependency console harness that proves the four required
// advanced OOP concepts work correctly, without needing the API or client running.
// Run with: dotnet run --project src/SmartX.SelfCheck

using SmartX.Shared.Buffers;
using SmartX.Shared.Domain;
using SmartX.Shared.Trees;

var failures = 0;

Check("Generics: TelemetryPacket<T> handles float/int/bool without boxing on the hot path", () =>
{
    var floatPacket = new TelemetryPacket<float>("SOIL-01", "Zone 1", SensorCategory.Environmental, TelemetryDataKind.Float, 41.7f);
    var intPacket = new TelemetryPacket<int>("PWR-01", "Grid Zone 1", SensorCategory.PowerConsumption, TelemetryDataKind.Integer, 312);
    var boolPacket = new TelemetryPacket<bool>("VALVE-01", "Zone 1", SensorCategory.Actuator, TelemetryDataKind.Boolean, true);

    Assert(floatPacket.Value == 41.7f, "float packet retains its value");
    Assert(intPacket.Value == 312, "int packet retains its value");
    Assert(boolPacket.Value, "bool packet retains its value");
    Assert(floatPacket.GetType() != intPacket.GetType(), "each closed generic is a distinct specialised value type");
});

Check("Operator overloading: PowerMeterReading aggregation and comparison", () =>
{
    var meter1 = new PowerMeterReading("PWR-01", 120.5);
    var meter2 = new PowerMeterReading("PWR-02", 80.25);

    var meter3 = meter1 + meter2; // aggregate load
    Assert(Math.Abs(meter3.WattageLoad - 200.75) < 0.001, "operator+ aggregates two meters' load");

    var delta = meter1 - meter2; // delta comparison
    Assert(Math.Abs(delta - 40.25) < 0.001, "operator- yields the delta between two readings");

    Assert(meter1 > meter2, "operator> compares wattage load");
    Assert(!(meter2 > meter1), "operator> is directionally correct");
    Assert(meter1 == new PowerMeterReading("PWR-01", 120.5), "operator== matches on id and load");
});

Check("Advanced arrays/lists: jagged array buffer flattens into an optimised List<T>", () =>
{
    var buffer = new RawTelemetryBatchBuffer(initialCapacity: 2);
    buffer.AppendCycle([41.1f, 42.3f]);          // cycle 1: two devices reported
    buffer.AppendCycle([40.9f]);                 // cycle 2: only one device reported (jagged!)
    buffer.AppendCycle([41.5f, 42.0f, 43.2f]);   // cycle 3: three devices reported

    Assert(buffer.CycleCount == 3, "buffer tracks 3 buffered cycles");
    var raw = buffer.RawSnapshot();
    Assert(raw[0].Length == 2 && raw[1].Length == 1 && raw[2].Length == 3, "rows are genuinely jagged (uneven lengths)");

    var flattened = buffer.FlattenToOptimisedList();
    Assert(flattened.Count == 6, "flattened List<float> contains all 6 raw samples");
});

Check("Advanced arrays/lists: 2D grid compacts into an optimised List<TelemetryPacket<T>>", () =>
{
    var grid = new PowerLoadHistoryGrid(["PWR-01", "PWR-02"], timeSlots: 3);
    grid.RecordSlot([300, 310]);
    grid.RecordSlot([305, 308]);

    var compacted = grid.CompactToOptimisedList("Facility B");
    Assert(compacted.Count == 4, "2 meters x 2 recorded slots = 4 optimised packets");
    Assert(compacted.All(p => p.Category == SensorCategory.PowerConsumption), "compacted packets keep their category");
});

Check("Recursion: DeploymentTreeValidator walks nested Facility -> Zone -> Sub-Zone -> Node", () =>
{
    var validTree = new DeviceNode
    {
        Name = "Facility A",
        Children =
        [
            new DeviceNode
            {
                Name = "Zone 1",
                Children =
                [
                    new DeviceNode
                    {
                        Name = "Sub-Zone B",
                        Children = [new DeviceNode { Name = "Node", IsLeafDevice = true, DeviceMacAddress = "AA:BB:CC:00:11:22" }]
                    }
                ]
            }
        ]
    };

    var goodResult = DeploymentTreeValidator.Validate(validTree);
    Assert(goodResult.IsValid, "a correctly nested tree passes recursive validation");
    Assert(goodResult.NodesVisited == 4, "recursion visits all 4 nodes (Facility, Zone, Sub-Zone, Node)");
    Assert(goodResult.MaxDepthReached == 3, "recursion correctly measures nesting depth");

    var brokenTree = new DeviceNode
    {
        Name = "Facility A",
        Children =
        [
            new DeviceNode { Name = "Zone 1", Children = [new DeviceNode { Name = "Orphan Node", IsLeafDevice = true }] }, // missing MAC
            new DeviceNode { Name = "Zone 2 (empty)", Children = [] } // empty zone
        ]
    };

    var badResult = DeploymentTreeValidator.Validate(brokenTree);
    Assert(!badResult.IsValid, "a misconfigured tree is correctly rejected");
    Assert(badResult.Issues.Count == 2, "both deliberate faults are detected by the recursive walk");
});

Console.WriteLine();
if (failures == 0)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("ALL CHECKS PASSED ✔");
}
else
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"{failures} CHECK GROUP(S) FAILED ✘");
}
Console.ResetColor();
Environment.Exit(failures == 0 ? 0 : 1);

void Check(string name, Action body)
{
    Console.Write($"• {name} ... ");
    try
    {
        body();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("PASS");
    }
    catch (Exception ex)
    {
        failures++;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"FAIL ({ex.Message})");
    }
    finally
    {
        Console.ResetColor();
    }
}

static void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
