import type { EngagementAlert } from "../types";

function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString();
}

export function AlertFeed({ alerts }: { alerts: EngagementAlert[] }) {
  if (alerts.length === 0) {
    return <p className="small-note">No proactive alerts yet — all sensors are within their learned baseline.</p>;
  }

  return (
    <ul className="alert-list">
      {alerts.map((a) => (
        <li key={a.id} className={`alert-item sev-${a.severity}`}>
          <strong>{a.severity}</strong> — {a.message}
          <span className="alert-time">{a.deviceMacAddress} · {formatTime(a.raisedAt)}</span>
        </li>
      ))}
    </ul>
  );
}
