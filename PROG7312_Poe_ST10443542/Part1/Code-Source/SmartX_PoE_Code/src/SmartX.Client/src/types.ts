export type SensorCategory = "Environmental" | "PowerConsumption" | "Actuator";
export type TelemetryDataKind = "Float" | "Integer" | "Boolean";
export type EngagementState = "Normal" | "Warning" | "Critical" | "Disconnected";

export interface ArchitecturalPillar {
  key: string;
  title: string;
  description: string;
  enabled: boolean;
}

export interface SensorAttachment {
  fileName: string;
  contentType: string;
  sizeBytes: number;
  storagePath: string;
  uploadedAt: string;
}

export interface SensorRegistration {
  deviceMacAddress: string;
  location: string;
  category: SensorCategory;
  dataKind: TelemetryDataKind;
  registeredAt: string;
  attachments: SensorAttachment[];
}

export interface SensorTileSnapshot {
  deviceMacAddress: string;
  location: string;
  category: SensorCategory;
  dataKind: TelemetryDataKind;
  state: EngagementState;
  latestNumericValue: number;
  latestBooleanValue: boolean | null;
  lastSeen: string;
  sparkline: number[];
  streak: number;
}

export interface EngagementAlert {
  id: string;
  deviceMacAddress: string;
  severity: EngagementState;
  message: string;
  raisedAt: string;
}

export interface BufferDiagnostics {
  jaggedArrayBuffer: {
    jaggedCycles: number;
    flattenedOptimisedListLength: number;
    note: string;
  };
  multiDimensionalGrid: {
    meters: number;
    timeSlots: number;
    gridCells: number;
    optimisedListLength: number;
    note: string;
  };
}

export interface DeviceNode {
  name: string;
  deviceMacAddress?: string | null;
  isLeafDevice: boolean;
  children: DeviceNode[];
}

export interface ValidationIssue {
  path: string;
  message: string;
}

export interface TreeValidationResult {
  isValid: boolean;
  issues: ValidationIssue[];
  nodesVisited: number;
  maxDepthReached: number;
}
