import { useEffect, useState } from "react";
import { api } from "../api";
import type { ArchitecturalPillar } from "../types";

export function LandingPage({ onSelectPillar }: { onSelectPillar: (key: string) => void }) {
  const [pillars, setPillars] = useState<ArchitecturalPillar[]>([]);

  useEffect(() => {
    api.getPillars().then(setPillars).catch(() => {});
  }, []);

  return (
    <div>
      <div className="app-header">
        <div>
          <h1>Smart-X IoT Mesh Ecosystem</h1>
          <div className="subtitle">Gateway console — pick an architectural pillar to continue</div>
        </div>
      </div>
      <div className="pillar-grid">
        {pillars.map((p) => (
          <div
            key={p.key}
            className={`pillar-card ${p.enabled ? "" : "disabled"}`}
            onClick={() => p.enabled && onSelectPillar(p.key)}
            title={p.enabled ? "Open" : "Coming in a later PoE"}
          >
            <span className={`badge ${p.enabled ? "enabled" : "disabled-badge"}`}>
              {p.enabled ? "ENABLED" : "DISABLED"}
            </span>
            <h3>{p.title}</h3>
            <p>{p.description}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
