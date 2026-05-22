# Adopt DevFlow in a WPF Project

This guide shows how to add DevFlow to an existing WPF app and verify it is working.

## 1. Prerequisites

- .NET 8.0 or later
- A WPF app project on Windows
- App can run locally

## 2. Add NuGet packages

From your WPF app folder:

```powershell
dotnet add package LeXtudio.DevFlow.Agent.WPF
dotnet add package LeXtudio.DevFlow.Driver
```

`LeXtudio.DevFlow.Agent.WPF` brings in the shared core package automatically.

## 3. Register DevFlow at app startup

Update `App.xaml.cs`:

```csharp
using System;
using System.Windows;
using Microsoft.Maui.DevFlow.Agent.Core;
using LeXtudio.DevFlow.Agent.WPF;

namespace YourAppNamespace;

public partial class App : Application
{
    private WpfAgentService? _devFlowService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var port = 5500;
        var portValue = Environment.GetEnvironmentVariable("DEVFLOW_AGENT_PORT");
        if (!string.IsNullOrWhiteSpace(portValue) && int.TryParse(portValue, out var parsedPort))
        {
            port = parsedPort;
        }

        _devFlowService = this.AddWpfDevFlowAgent(new AgentOptions { Port = port });
    }
}
```

Recommended: keep this inside debug-only logic if you do not want the local HTTP agent in release runs.

## 4. Build and run

```powershell
dotnet build
dotnet run
```

## 5. Verify the agent

Check status:

```powershell
Invoke-WebRequest http://localhost:5500/api/v1/agent/status | Select-Object -ExpandProperty Content
```

If you used a custom port, replace `5500`.

## 6. What to expect after adoption

After startup, your app hosts a local DevFlow HTTP agent with these endpoints:

- `GET /api/v1/agent/status`
- `GET /api/v1/ui/tree`
- `GET /api/v1/ui/element?id=<id>`
- `GET /api/v1/ui/screenshot`
- `POST /api/v1/ui/tap`
- `POST /api/v1/ui/actions/scroll`

Practically, this means tooling can inspect the live UI tree, capture screenshots, and drive supported UI actions through HTTP while your app is running.

## 7. Optional environment variables

- `DEVFLOW_AGENT_PORT`: override default port (`5500`)
- `DEVFLOW_HIDE_WINDOW`: for automation scenarios, hides the main window when set to `true`
