import type {
  ArchitecturalPillar,
  BufferDiagnostics,
  DeviceNode,
  EngagementAlert,
  SensorRegistration,
  SensorTileSnapshot,
  TreeValidationResult,
} from "./types";

const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5220";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: init?.body instanceof FormData ? undefined : { "Content-Type": "application/json" },
    ...init,
  });

  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`${res.status} ${res.statusText}: ${text}`);
  }

  if (res.status === 202 || res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

export const api = {
  getPillars: () => request<ArchitecturalPillar[]>("/api/pillars"),

  listSensors: () => request<SensorRegistration[]>("/api/sensors"),

  registerSensor: (payload: {
    deviceMacAddress: string;
    location: string;
    category: string;
    dataKind: string;
  }) =>
    request<SensorRegistration>("/api/sensors", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  uploadAttachment: (mac: string, file: File) => {
    const form = new FormData();
    form.append("file", file);
    return request<unknown>(`/api/sensors/${encodeURIComponent(mac)}/attachments`, {
      method: "POST",
      body: form,
    });
  },

  getTiles: () => request<SensorTileSnapshot[]>("/api/telemetry/tiles"),

  getAlerts: (take = 30) => request<EngagementAlert[]>(`/api/telemetry/alerts?take=${take}`),

  getBufferDiagnostics: () => request<BufferDiagnostics>("/api/telemetry/diagnostics/buffers"),

  ingestTelemetry: (payload: {
    deviceMacAddress: string;
    location: string;
    category: string;
    kind: string;
    numericValue?: number;
    booleanValue?: boolean;
  }) =>
    request<unknown>("/api/telemetry/ingest", {
      method: "POST",
      body: JSON.stringify(payload),
    }),

  getSampleTree: () => request<DeviceNode>("/api/deployment-tree/sample"),

  validateTree: (tree: DeviceNode) =>
    request<TreeValidationResult>("/api/deployment-tree/validate", {
      method: "POST",
      body: JSON.stringify(tree),
    }),
};
