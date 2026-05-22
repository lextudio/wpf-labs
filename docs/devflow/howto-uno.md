# Adopt DevFlow in an Uno / WinUI 3 Project

This guide shows how to add DevFlow to an existing Uno app (including WinUI 3 targets).

If your app is a pure WinUI 3 app (without Uno), you can still follow the same package and registration approach in this guide by using `LeXtudio.DevFlow.Agent.Uno` and registering on the `Application` instance.

## 1. Prerequisites

- .NET 10.0 or later (matching current package compatibility)
- Uno Platform project builds and runs locally

## 2. Add NuGet packages

From the Uno app project folder:

```powershell
dotnet add package LeXtudio.DevFlow.Agent.Uno
dotnet add package LeXtudio.DevFlow.Driver
```

`LeXtudio.DevFlow.Agent.Uno` brings in shared agent core support.

## 3. Register DevFlow in app startup

Register the agent on the app `Application` instance:

```csharp
using Microsoft.Maui.DevFlow.Agent.Core;
using LeXtudio.DevFlow.Agent.Uno;

// Example in startup path where Application is available:
var service = Application.Current.AddUnoDevFlowAgent(new AgentOptions
{
    Port = 5500
});
```

If you do not pass `AgentOptions`, default settings are used.

## 4. Build and run

```powershell
dotnet build
dotnet run
```

## 5. Verify the agent

```powershell
Invoke-WebRequest http://localhost:5500/api/v1/agent/status | Select-Object -ExpandProperty Content
```

You should receive JSON with framework, id, running status, and port.

## 6. What to expect after adoption

A local DevFlow HTTP server is available while the app runs. It supports:

- reading agent status
- reading the live UI tree
- reading a single UI element by id
- capturing a screenshot
- tap and scroll actions

API routes:

- `GET /api/v1/agent/status`
- `GET /api/v1/ui/tree`
- `GET /api/v1/ui/element?id=<id>`
- `GET /api/v1/ui/screenshot`
- `POST /api/v1/ui/tap`
- `POST /api/v1/ui/actions/scroll`

## 7. Recommended practice

Like MAUI onboarding guidance, keep DevFlow registration in debug-only code unless you explicitly need it in non-debug runs.
