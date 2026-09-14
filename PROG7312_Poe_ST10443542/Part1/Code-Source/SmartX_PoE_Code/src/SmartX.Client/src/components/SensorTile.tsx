import type { SensorTileSnapshot } from "../types";
import { Sparkline } from "./Sparkline";

const STATE_STROKE: Record<string, string> = {
  Normal: "#3fa34d",
  Warning: "#e0a52c",
  Critical: "#c0392b",
  Disconnected: "#7a7f8c",
};

function timeAgo(iso: string): string {
  const seconds = Math.max(0, Math.floor((Date.now() - new Date(iso).getTime()) / 1000));
  if (seconds < 60) return `${seconds}s ago`;
  return `${Math.floor(seconds / 60)}m ago`;
}

export function SensorTile({ tile }: { tile: SensorTileSnapshot }) {
  const displayValue =
    tile.dataKind === "Boolean"
      ? tile.latestBooleanValue
        ? "OPEN"
        : "CLOSED"
      : tile.latestNumericValue.toFixed(tile.dataKind === "Integer" ? 0 : 1);

  const unit = tile.category === "PowerConsumption" ? " W" : tile.category === "Environmental" ? "%" : "";

  return (
    <div className={`sensor-tile state-${tile.state}`}>
      <div className="tile-top">
        <span className="tile-mac">{tile.deviceMacAddress}</span>
        <span className={`state-dot state-${tile.state}`} title={tile.state} />
      </div>
      <div className="tile-location">{tile.location}</div>
      <div className="tile-value">
        {displayValue}
        {tile.dataKind !== "Boolean" && unit}
      </div>
      {tile.dataKind !== "Boolean" && <Sparkline values={tile.sparkline} stroke={STATE_STROKE[tile.state]} />}
      <div className="tile-meta">
        <span className="streak-chip">🔥 {tile.streak}</span>
        <span>{timeAgo(tile.lastSeen)}</span>
      </div>
    </div>
  );
}
