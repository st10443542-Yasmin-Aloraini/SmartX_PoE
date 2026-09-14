import { useState } from "react";
import { api } from "../api";

const CATEGORIES = ["Environmental", "PowerConsumption", "Actuator"] as const;
const KINDS = ["Float", "Integer", "Boolean"] as const;

// Mirrors the facility/zone/sub-zone/node hierarchy already used by the seeded devices and
// the deployment tree explorer, so a new sensor's location is always consistent with the
// rest of the mesh instead of being freely typed (and easy to typo).
const KNOWN_LOCATIONS = [
  "Facility A -> Zone 1 -> Sub-Zone A -> Bed 1",
  "Facility A -> Zone 1 -> Sub-Zone A -> Bed 2",
  "Facility A -> Zone 1 -> Sub-Zone A -> Bed 3",
  "Facility A -> Zone 1 -> Sub-Zone A -> Bed 4",
  "Facility A -> Zone 1 -> Sub-Zone B -> Bed 1",
  "Facility A -> Zone 1 -> Sub-Zone B -> Bed 2",
  "Facility A -> Zone 1 -> Greenhouse",
  "Facility A -> Zone 2 -> Sub-Zone A -> Bed 1",
  "Facility A -> Zone 2 -> Sub-Zone A -> Bed 2",
  "Facility A -> Zone 2 -> Sub-Zone A -> Bed 3",
  "Facility A -> Zone 2 -> Sub-Zone B -> Bed 1",
  "Facility A -> Zone 2 -> Sub-Zone B -> Bed 2",
  "Facility A -> Zone 2 -> Greenhouse",
  "Facility B -> Grid Zone 1 -> Node 1",
  "Facility B -> Grid Zone 1 -> Node 2",
  "Facility B -> Grid Zone 1 -> Node 3",
  "Facility B -> Grid Zone 2 -> Node 1",
  "Facility B -> Grid Zone 2 -> Node 2",
  "Facility B -> Grid Zone 3 -> Node 1",
  "Facility B -> Grid Zone 3 -> Node 2",
] as const;

const OTHER_LOCATION = "__other__";

export function SensorRegistrationForm() {
  const [mac, setMac] = useState("");
  const [locationChoice, setLocationChoice] = useState<string>(KNOWN_LOCATIONS[0]);
  const [customLocation, setCustomLocation] = useState("");
  const [category, setCategory] = useState<(typeof CATEGORIES)[number]>("Environmental");
  const [dataKind, setDataKind] = useState<(typeof KINDS)[number]>("Float");
  const [file, setFile] = useState<File | null>(null);
  const [status, setStatus] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const location = locationChoice === OTHER_LOCATION ? customLocation.trim() : locationChoice;

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!mac.trim()) {
      setStatus("Device MAC address / unique identifier is required.");
      return;
    }
    if (!location) {
      setStatus("Please choose or enter a deployment location.");
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
        <label htmlFor="location">Sensor Deployment Location (Facility / Zone / Sub-Zone / Node)</label>
        <select id="location" value={locationChoice} onChange={(e) => setLocationChoice(e.target.value)}>
          {KNOWN_LOCATIONS.map((loc) => (
            <option key={loc} value={loc}>{loc}</option>
          ))}
          <option value={OTHER_LOCATION}>Other (type a custom location)</option>
        </select>
        {locationChoice === OTHER_LOCATION && (
          <input
            id="custom-location"
            value={customLocation}
            onChange={(e) => setCustomLocation(e.target.value)}
            placeholder="e.g. Facility C -> Zone 1 -> Sub-Zone A"
            style={{ marginTop: "0.5rem" }}
          />
        )}
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
