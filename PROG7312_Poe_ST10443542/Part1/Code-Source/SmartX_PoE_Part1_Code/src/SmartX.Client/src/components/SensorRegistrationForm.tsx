import { useState } from "react";
import { api } from "../api";

const CATEGORIES = ["Environmental", "PowerConsumption", "Actuator"] as const;
const KINDS = ["Float", "Integer", "Boolean"] as const;

export function SensorRegistrationForm() {
  const [mac, setMac] = useState("");
  const [location, setLocation] = useState("Facility A -> Zone 1 -> Sub-Zone A -> Bed 1");
  const [category, setCategory] = useState<(typeof CATEGORIES)[number]>("Environmental");
  const [dataKind, setDataKind] = useState<(typeof KINDS)[number]>("Float");
  const [file, setFile] = useState<File | null>(null);
  const [status, setStatus] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!mac.trim()) {
      setStatus("Device MAC address / unique identifier is required.");
      return;
    }

    setBusy(true);
    setStatus(null);
    try {
      await api.registerSensor({ deviceMacAddress: mac.trim(), location, category, dataKind });
      if (file) {
        await api.uploadAttachment(mac.trim(), file);
      }
      setStatus(`Registered ${mac.trim()} successfully${file ? " with attachment." : "."}`);
      setMac("");
      setFile(null);
    } catch (err) {
      setStatus(`Failed: ${(err as Error).message}`);
    } finally {
      setBusy(false);
    }
  }

  return (
    <form className="registration-form" onSubmit={handleSubmit}>
      <div>
        <label htmlFor="mac">Device MAC Address / Unique Identifier</label>
        <input id="mac" value={mac} onChange={(e) => setMac(e.target.value)} placeholder="e.g. AA:BB:CC:00:11:22" />
      </div>
      <div>
        <label htmlFor="location">Sensor Deployment Location (Room / Zone / Node ID)</label>
        <input id="location" value={location} onChange={(e) => setLocation(e.target.value)} />
      </div>
      <div>
        <label htmlFor="category">Sensor Category</label>
        <select id="category" value={category} onChange={(e) => setCategory(e.target.value as typeof category)}>
          {CATEGORIES.map((c) => (
            <option key={c} value={c}>{c}</option>
          ))}
        </select>
      </div>
      <div>
        <label htmlFor="kind">Telemetry Data Type</label>
        <select id="kind" value={dataKind} onChange={(e) => setDataKind(e.target.value as typeof dataKind)}>
          {KINDS.map((k) => (
            <option key={k} value={k}>{k}</option>
          ))}
        </select>
      </div>
      <div>
        <label htmlFor="file">Device Config File / Deployment Photo / Hardware Log</label>
        <input id="file" type="file" onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
      </div>
      <button type="submit" disabled={busy}>{busy ? "Registering…" : "Register Sensor"}</button>
      {status && <p className="small-note">{status}</p>}
    </form>
  );
}
