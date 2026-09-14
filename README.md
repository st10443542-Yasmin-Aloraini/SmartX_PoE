# Smart-X IoT Mesh Ecosystem — Setup Guide

**Module:** PROG7312/AAPD7112 — Advanced Application Development
**Student:** Yasmin
**Submission:** Smart-X Data Ingestion and Validation Gateway (Part 1)

---

## What this submission contains

| Component | Location | Description |
|---|---|---|
| Research report | `Task1_Research_Report.docx` | Five engagement strategies, justification, references, diagrams |
| Shared library | `src/SmartX.Shared` | Generics, operator overloading, jagged/2D array buffers, recursive tree validator |
| Backend API | `src/SmartX.Api` | ASP.NET Core (.NET 10) Minimal API — ingestion, sensor registration, file uploads, engagement engine |
| Frontend | `src/SmartX.Client` | React + TypeScript dashboard |
| Self-check harness | `src/SmartX.SelfCheck` | Console app that proves the four core OOP concepts work, independent of the UI |
| Architecture note | `docs/architecture-note.md` | Explains the frontend technology choice (React vs Blazor WebAssembly) |

---

## Run with Docker

**Requirement:** Docker Desktop installed and running.

From the project root:

```bash
docker compose up --build
```

Once it finishes starting:
- **Dashboard:** http://localhost:5173
- **API:** http://localhost:8080

Click **"Sensor Data Ingestion and Telemetry"** on the landing page (the other two pillars are
intentionally disabled — later scope). Live sensor tiles, a proactive alert feed, a
sensor-registration/file-upload form, and a recursive deployment-tree validator are all visible
and interactive there.

Press `Ctrl+C` to stop.
