# Architecture note: why React instead of Blazor WebAssembly

The brief's Web-First option lists three acceptable frontends for an ASP.NET Core Minimal API
backend: **Blazor WebAssembly, React, or Vue**. This solution uses **React + TypeScript**.

## Why

This project was built and verified inside a sandboxed development environment whose outbound
network access is restricted to a small allow-list of package registries (npm, PyPI, crates.io,
Go modules, etc.) for security reasons. `nuget.org` and Microsoft's package feeds were **not**
reachable from that sandbox, while `registry.npmjs.org` was. A `dotnet new blazorwasm` project
cannot restore (`Microsoft.AspNetCore.Components.WebAssembly` and related packages come from
NuGet), so it could not be built or run in that environment — meaning it could not be verified.

Given the assignment's explicit rule that *"if the code does not compile and run, across both the
frontend client and backend API layers, no marks will be awarded for any application
functionality,"* the safer engineering decision was to build the frontend with a toolchain that
could actually be restored, built, run, and screenshotted end-to-end in that environment — React
via npm/Vite — rather than submit an unverified Blazor WASM client. The backend
(`SmartX.Api`) and shared library (`SmartX.Shared`) use **zero external NuGet packages**, for the
same reason: they build offline with nothing beyond the .NET 10 shared framework.

Every functional requirement (landing page with 3 pillars, sensor registration, file upload,
generics, operator overloading, jagged/2D arrays, recursion, the dynamic engagement feature) is
implemented and was manually verified running end-to-end (see the README's Testing section and
the screenshots taken during development).

## Switching to Blazor WebAssembly

On a machine with normal internet access (i.e. any standard student/marker machine), swapping the
client for Blazor WebAssembly is straightforward, since the backend already exposes a plain JSON
HTTP API that any frontend can consume:

```bash
dotnet new blazorwasm -o src/SmartX.Client.Blazor
dotnet sln add src/SmartX.Client.Blazor
dotnet add src/SmartX.Client.Blazor reference src/SmartX.Shared
```

Then re-implement the components in `src/SmartX.Client/src/components/*` as `.razor` components,
using `HttpClient` against the same endpoints documented in the root README's API reference table,
and reuse the DTOs already defined in `SmartX.Shared.Domain` / `SmartX.Shared.Trees` directly
(no need to re-declare `SensorTileSnapshot`, `EngagementAlert`, `DeviceNode`, etc. — they are
already shared, dependency-free C# types).
