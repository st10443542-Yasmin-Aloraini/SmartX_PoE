import { useEffect, useState } from "react";
import { api } from "../api";
import type { BufferDiagnostics, EngagementAlert, SensorTileSnapshot } from "../types";
import { SensorTile } from "./SensorTile";
import { AlertFeed } from "./AlertFeed";
import { SensorRegistrationForm } from "./SensorRegistrationForm";
import { DeploymentTreeExplorer } from "./DeploymentTreeExplorer";
import { BufferDiagnosticsPanel } from "./BufferDiagnosticsPanel";

const POLL_MS = 2000;

export function Dashboard({ onBack }: { onBack: () => void }) {
  const [tiles, setTiles] = useState<SensorTileSnapshot[]>([]);
  const [alerts, setAlerts] = useState<EngagementAlert[]>([]);
  const [diagnostics, setDiagnostics] = useState<BufferDiagnostics | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function poll() {
      try {
        const [t, a, d] = await Promise.all([api.getTiles(), api.getAlerts(20), api.getBufferDiagnostics()]);
        if (!cancelled) {
          setTiles(t);
          setAlerts(a);
          setDiagnostics(d);
          setError(null);
        }
      } catch (err) {
        if (!cancelled) setError((err as Error).message);
      }
    }

    poll();
    const timer = setInterval(poll, POLL_MS);
    return () => {
      cancelled = true;
      clearInterval(timer);
    };
  }, []);

  const summary = tiles.reduce(
    (acc, t) => {
      acc[t.state] = (acc[t.state] ?? 0) + 1;
      return acc;
    },
    {} as Record<string, number>
  );

  return (
    <div>
      <span className="back-link" onClick={onBack}>&larr; Back to pillars</span>
      <div className="app-header">
        <div>
          <h1>Sensor Data Ingestion &amp; Telemetry</h1>
          <div className="subtitle">
            {tiles.length} devices · {summary.Normal ?? 0} normal · {summary.Warning ?? 0} warning ·{" "}
            {summary.Critical ?? 0} critical · {summary.Disconnected ?? 0} disconnected
          </div>
        </div>
      </div>

      {error && <div className="panel" style={{ borderColor: "var(--critical)" }}>⚠ {error} — is the API running?</div>}

      <div className="grid-2">
        <div>
          <div className="panel">
            <h2>Live Sensor Tiles</h2>
            <div className="tile-grid">
              {tiles.map((t) => (
                <SensorTile key={t.deviceMacAddress} tile={t} />
              ))}
              {tiles.length === 0 && <p className="small-note">Waiting for telemetry…</p>}
            </div>
          </div>

          <div className="panel">
            <h2>Buffer &amp; Data-Structure Diagnostics</h2>
            <BufferDiagnosticsPanel diagnostics={diagnostics} />
          </div>

          <div className="panel">
            <h2>Multi-Tier Deployment Tree (Recursive Validation)</h2>
            <DeploymentTreeExplorer />
          </div>
        </div>

        <div>
          <div className="panel">
            <h2>Proactive Alert Feed</h2>
            <AlertFeed alerts={alerts} />
          </div>

          <div className="panel">
            <h2>Register / Simulate a Sensor</h2>
            <SensorRegistrationForm />
          </div>
        </div>
      </div>
    </div>
  );
}
