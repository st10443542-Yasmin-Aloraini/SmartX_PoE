# Smart-X IoT Mesh Ecosystem — Setup Guide

**Module:** PROG7312 — Advanced Application Development
**Student:** Yasmin
**Submission Location:** `PROG7312_Poe_ST10443542/Part1` 
 
---
 
## What this submission contains
 
| Component | Location |
|---|---|
| Video | YouTube link (submitted separately) |
| Research document | `Task1_Research_Report.docx` |
| Code | `src/` |
 
---
 
## Run Part 1 with Docker
 
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
 
