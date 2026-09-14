# Smart-X IoT Mesh Ecosystem — Part 1: Data Ingestion & Validation Gateway

PROG7312/AAPD7112 — Advanced Application Development, PoE Part 1 (100 marks).

A Web-First implementation: an **ASP.NET Core (.NET 10) Minimal API** backend paired with a
**React + TypeScript (Vite)** client. See [`docs/architecture-note.md`](docs/architecture-note.md)
for why React was used instead of Blazor WebAssembly, and how to switch to Blazor WASM instead.

## What's here

| Project | Description |
|---|---|
| `src/SmartX.Shared` | Class library: `TelemetryPacket<T>` generic, `PowerMeterReading` operator overloads, jagged/2D array buffers, recursive deployment-tree validator. No external dependencies. |
| `src/SmartX.Api` | ASP.NET Core Minimal API: ingestion, sensor registration, file uploads, engagement/alert engine, mock telemetry seeder. No external NuGet packages required. |
| `src/SmartX.Client` | React + TypeScript dashboard: landing page (3 pillars), live sensor tiles, alert feed, sensor registration + file upload, recursive tree explorer, buffer diagnostics. |
| `src/SmartX.SelfCheck` | Zero-dependency console harness that proves the four required OOP concepts work correctly (see below). |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org) and npm
- Docker + Docker Compose (optional, for containerised run)

## Quick start (no Docker)

**1. Restore & build everything**

```bash
dotnet restore
dotnet build
```

**2. Run the self-check (proves generics / operator overloading / arrays / recursion work)**

```bash
dotnet run --project src/SmartX.SelfCheck
```

Expect `ALL CHECKS PASSED ✔`.

**3. Run the API** (from the repo root, in its own terminal)

```bash
dotnet run --project src/SmartX.Api
```

The API listens on `http://localhost:5220` (see `src/SmartX.Api/Properties/launchSettings.json`)
and immediately starts seeding ~18 simulated ESP32 devices with mock telemetry every 1.5s.
Try it: `curl http://localhost:5220/api/telemetry/tiles`.

**4. Run the client** (in a second terminal)

```bash
cd src/SmartX.Client
npm install
npm run dev
```

Open the printed URL (default `http://localhost:5173`). Click **Sensor Data Ingestion and
Telemetry** on the landing page to open the live dashboard. The other two pillars are
intentionally disabled — they are Part 2 / final-PoE scope.

If your API runs on a different port, copy `.env.example` to `.env` in `src/SmartX.Client`
and adjust `VITE_API_BASE_URL`.

## Quick start (Docker)

```bash
docker compose up --build
```

- API: http://localhost:8080
- Client: http://localhost:5173

## Architectural pillars (landing page)

| Pillar | Status |
|---|---|
| Sensor Data Ingestion and Telemetry | **Enabled** — implemented in this PoE part |
| Real-Time Command Stream and History | Disabled — Part 2 |
| Network Topology and Mesh Routing | Disabled — final PoE |

## How the required technical concepts are implemented

- **Generics** — `SmartX.Shared/Domain/TelemetryPacket.cs`: `TelemetryPacket<T> where T : struct`
  wraps float/int/bool payloads uniformly. Each closed generic (`TelemetryPacket<float>`,
  `TelemetryPacket<int>`, `TelemetryPacket<bool>`) is a distinct value type — no boxing occurs on
  the ingestion/validation/storage path; boxing happens only, deliberately, at the JSON/API
  boundary via `BoxedValue()`.
- **Operator overloading** — `SmartX.Shared/Domain/PowerMeterReading.cs`: `+` aggregates two
  meters' load (`Meter3 = Meter1 + Meter2`), `-` computes a delta, `>`/`<`/`==`/`!=` compare
  readings. This is not just declared — `TelemetryEngine.IngestNumeric` uses the overloaded `-`
  and `>` operators live, in the anomaly-detection path for power-consumption devices.
- **Advanced arrays and lists** — `SmartX.Shared/Buffers/RawTelemetryBatchBuffer.cs`: a
  **jagged** `float[][]` buffers uneven-arrival environmental samples per ingestion cycle, then
  `FlattenToOptimisedList()` compacts them into `List<float>`. `PowerLoadHistoryGrid` uses a
  **rectangular** `int[,]` grid (meters × time-slots) for the fixed-schedule power meters, then
  `CompactToOptimisedList()` produces `List<TelemetryPacket<int>>`. Both are exercised
  continuously by the mock seeder and visible live on the dashboard's "Buffer & Data-Structure
  Diagnostics" panel, and at `GET /api/telemetry/diagnostics/buffers`.
- **Recursion** — `SmartX.Shared/Trees/DeviceNode.cs`: `DeploymentTreeValidator.Validate`
  recursively descends a nested `Facility -> Zone -> Sub-Zone -> Node` tree, checking unique MAC
  addresses, maximum nesting depth, and that every branch terminates correctly (leaves carry a
  device, zones are never empty). Try it at `GET /api/deployment-tree/sample` and
  `POST /api/deployment-tree/validate`, or interactively on the dashboard.
- **Dynamic engagement feature** — `SmartX.Api/Services/TelemetryEngine.cs` implements the
  strategy justified in the Task 1 research report: **real-time visual feedback with proactive,
  severity-tiered alert escalation**. Every sensor is classified into `Normal` / `Warning` /
  `Critical` / `Disconnected` using an online (Welford) z-score baseline per device, rendered as a
  colour-and-motion-coded tile with a live sparkline and an uptime "streak" counter, with
  de-duplicated alerts raised to a live feed. This is not a progress bar.

## API reference

| Method | Path | Purpose |
|---|---|---|
| GET | `/api/pillars` | The 3 landing-page pillars and their enabled state |
| GET / POST | `/api/sensors` | List / register sensors (MAC, location, category) |
| GET | `/api/sensors/{mac}` | Get one sensor |
| POST | `/api/sensors/{mac}/attachments` | Multipart file upload (config/photo/log) |
| GET | `/api/sensors/{mac}/attachments` | List a sensor's attachments |
| POST | `/api/telemetry/ingest` | Ingest one telemetry reading |
| GET | `/api/telemetry/tiles` | Current dashboard tile snapshot per sensor |
| GET | `/api/telemetry/alerts?take=30` | Recent proactive alerts |
| GET | `/api/telemetry/diagnostics/buffers` | Jagged-array / 2D-grid buffer stats |
| GET | `/api/deployment-tree/sample` | Sample nested deployment tree |
| POST | `/api/deployment-tree/validate` | Recursively validate a submitted tree |
| GET | `/api/health` | Liveness check |

## Mock data seeding

`SmartX.Api/Services/MockTelemetrySeeder.cs` is a `BackgroundService` that registers 18
simulated devices (soil-moisture/temperature floats, power-meter integers, valve/relay booleans)
spread across a realistic facility/zone hierarchy, and publishes a reading for each every 1.5
seconds — occasionally injecting a spike (~4% of readings) and periodically taking one sensor
offline — so the engagement engine and both buffer structures have real, continuous load to react
to without needing physical ESP32 hardware. Real hardware can be plugged in by POSTing to
`/api/telemetry/ingest` instead.

## Testing

`src/SmartX.SelfCheck` is a dependency-free console app (only references `SmartX.Shared`) that
asserts each of the four required OOP concepts behaves correctly in isolation — useful for a
marker who wants a fast, no-server-required proof that the data structures work. Run it with
`dotnet run --project src/SmartX.SelfCheck`.

The API and client were also manually verified end-to-end (registration, file upload, live
telemetry classification, alert de-duplication, and recursive tree validation all confirmed
working together).

## Project structure

```
smartx/
├── docker-compose.yml
├── SmartX.sln
├── docs/
│   └── architecture-note.md
└── src/
    ├── SmartX.Shared/       # generics, operator overloads, array buffers, recursion
    ├── SmartX.Api/          # Minimal API, engagement engine, mock seeder, Dockerfile
    ├── SmartX.Client/       # React + TS dashboard, Dockerfile
    └── SmartX.SelfCheck/    # console self-test harness
```
