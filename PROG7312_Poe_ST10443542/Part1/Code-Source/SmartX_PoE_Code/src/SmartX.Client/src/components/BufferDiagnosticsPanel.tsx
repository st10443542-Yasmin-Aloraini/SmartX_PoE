import type { BufferDiagnostics } from "../types";

export function BufferDiagnosticsPanel({ diagnostics }: { diagnostics: BufferDiagnostics | null }) {
  if (!diagnostics) return <p className="small-note">Loading buffer diagnostics…</p>;

  return (
    <div>
      <p className="small-note">
        Proof that the underlying data structures keep up under continuous, high-throughput
        ingestion — a jagged array for uneven-arrival environmental samples, and a fixed
        multi-dimensional grid for the scheduled power-meter readings.
      </p>
      <div className="diagnostics-row">
        <div className="stat">
          <b>{diagnostics.jaggedArrayBuffer.jaggedCycles}</b>
          jagged float[][] cycles buffered
        </div>
        <div className="stat">
          <b>{diagnostics.jaggedArrayBuffer.flattenedOptimisedListLength}</b>
          flattened into List&lt;float&gt;
        </div>
        <div className="stat">
          <b>{diagnostics.multiDimensionalGrid.gridCells}</b>
          int[,] grid cells ({diagnostics.multiDimensionalGrid.meters}×{diagnostics.multiDimensionalGrid.timeSlots})
        </div>
        <div className="stat">
          <b>{diagnostics.multiDimensionalGrid.optimisedListLength}</b>
          compacted into List&lt;TelemetryPacket&lt;int&gt;&gt;
        </div>
      </div>
    </div>
  );
}
