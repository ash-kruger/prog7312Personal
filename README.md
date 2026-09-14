# Smart-X IoT Ingestion Gateway — Part 1

PROG7312/w (Programming 3B) &middot; AAPD7112/w (Advanced Application Development) &middot; POE Part 1: Smart-X Data Ingestion and Validation Gateway

This repository contains the Task 2 implementation for Part 1 of the Smart-X POE: a .NET ingestion API paired with a Blazor WebAssembly client dashboard. It will be extended in Part 2 (Real-Time Command Stream) and the final PoE (Network Topology and Mesh Routing) — **do not delete or rewrite this project**, later parts build directly on it.

## ⚠️ Before you submit

This solution was written and reviewed carefully, but it was **not compiled in the environment that produced it** (no .NET SDK was available there). Run the build steps below yourself and fix anything the compiler flags **before you submit** — the brief is explicit that no marks are awarded for code that doesn't compile and run. In practice this usually means, at most, a missing `using`, a package version bump, or a typo — the logic and structure have been checked by hand line by line.

## Solution layout

```
SmartX.sln
src/
  SmartX.Shared/   Class library: DTOs and the SensorCategory / DeviceNode types shared by API and client
  SmartX.Api/       ASP.NET Core Minimal API — the ingestion gateway
  SmartX.Client/    Blazor WebAssembly — the configuration dashboard
docker/
  Dockerfile.api    Optional container build for the API
docker-compose.yml  Optional — runs the API in a container
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`dotnet --version` should report a `10.x` SDK)
- Any editor (Visual Studio 2022 17.9+, VS Code with the C# Dev Kit, or Rider)
- No database is required — everything is stored in memory for Part 1, as instructed ("heavily seed your application with mock telemetry data")

## Restore, build, and run

From the repository root:

```bash
# 1. Restore all three projects
dotnet restore

# 2. Build everything
dotnet build

# 3. Run the API (from one terminal — keep it running)
dotnet run --project src/SmartX.Api

# 4. Run the client (from a second terminal)
dotnet run --project src/SmartX.Client
```

By default:
- The API listens on `http://localhost:5233` (see `src/SmartX.Api/Properties/launchSettings.json`).
- The client listens on whatever port `dotnet run` reports (typically `http://localhost:5xxx`) and is configured to call the API at `http://localhost:5233/` via `src/SmartX.Client/wwwroot/appsettings.json`. If your API ends up on a different port, update `ApiBaseUrl` in that file.

Open the client's URL in a browser. You should see the Smart-X landing page with three pillars — only **Sensor Data Ingestion & Telemetry** is enabled for Part 1.

## Using the app (also doubles as a test script)

1. **Register a sensor** — fill in a MAC address (e.g. `AA:BB:CC:DD:EE:01`), a deployment location, and a category, then click *Register Sensor*. It should appear in the table below.
2. **Attach a file** — select the sensor's row (*Select*), choose any file, and click *Upload*. The file count in the table should increment.
3. **Submit telemetry** — pick the sensor, choose a value type (`float`, `int`, or `bool`), enter a value (and optionally a critical threshold), and click *Ingest Telemetry*. Submit several values in a row (try increasing and then spiking one) to see the **Live Anomaly Radar** sparkline update and change colour (green → amber → red) — this is the dynamic dashboard engagement feature described in the Task 1 research report.
4. **Meter aggregation** — enter two MAC/wattage pairs and click *Aggregate* to see the overloaded `+`, `-`, and `>` operators on `MeterReading` in action.
5. **Hierarchy validator** — edit the sample JSON tree (or paste your own nested `Facility → Zone → Sub-Zone` structure) and click *Validate Hierarchy* to exercise the recursive `DeviceHierarchyValidator`.

You can also exercise the API directly, e.g. with `curl`:

```bash
curl -X POST http://localhost:5233/api/sensors/register \
  -H "Content-Type: application/json" \
  -d '{"deviceMac":"AA:BB:CC:DD:EE:01","deploymentLocation":"Zone 1 / Sub-Zone B","category":"Environmental"}'

curl -X POST http://localhost:5233/api/telemetry/ingest \
  -H "Content-Type: application/json" \
  -d '{"deviceMac":"AA:BB:CC:DD:EE:01","valueType":"float","floatValue":42.5}'

curl http://localhost:5233/api/telemetry/AA:BB:CC:DD:EE:01/history
```

## Where each technical requirement lives

| Requirement | File |
|---|---|
| Generics (`TelemetryPacket<T>`) | `src/SmartX.Api/Models/TelemetryPacket.cs` |
| Operator overloading | `src/SmartX.Api/Models/MeterReading.cs` |
| Jagged arrays → `List<T>` | `src/SmartX.Api/Services/HistoricalBatchBuffer.cs` |
| Recursion (nested hierarchy validation) | `src/SmartX.Api/Services/DeviceHierarchyValidator.cs` |
| Dictionary-backed O(1) sensor lookup | `src/SmartX.Api/Services/SensorRegistry.cs` |
| Multipart file upload | `src/SmartX.Api/Services/FileStorageService.cs` + `POST /api/sensors/{mac}/upload` |
| Dynamic dashboard engagement feature | `src/SmartX.Client/Pages/SensorIngestion.razor` (Live Anomaly Radar section) |
| Startup menu with 3 pillars (2 disabled) | `src/SmartX.Client/Pages/Home.razor` |

## API endpoints

| Method | Route | Purpose |
|---|---|---|
| GET | `/` | Health check |
| POST | `/api/sensors/register` | Register a sensor |
| GET | `/api/sensors` | List registered sensors |
| GET | `/api/sensors/{mac}` | Get one sensor |
| POST | `/api/sensors/{mac}/upload` | Attach a config file / photo / log (multipart) |
| POST | `/api/telemetry/ingest` | Ingest one telemetry sample (`float`/`int`/`bool`) |
| GET | `/api/telemetry/{mac}/history` | Get a device's history |
| POST | `/api/telemetry/aggregate` | Aggregate/compare two meters (operator overloading demo) |
| POST | `/api/devices/validate-hierarchy` | Recursively validate a device deployment tree |

## Optional: Docker

The API can be built and run in a container (this is optional, per the brief):

```bash
docker compose up --build
```

This only containerises the API; the Blazor client is a static app best run with `dotnet run` or served from any static file host during development.

## Research report

The Task 1 research report (`Smart-X Dashboard Engagement Research.docx`) is included in the submission alongside this source code. It explains and justifies the Live Anomaly Radar engagement strategy implemented in `SensorIngestion.razor`.

## Notes on data persistence

All data (sensors, telemetry history, uploaded file metadata) is held in memory (`ConcurrentDictionary`/`List<T>`) for Part 1, matching the "heavily seed with mock telemetry data" simulation note in the brief. Restarting the API clears all data — this is expected for this stage of the project.
