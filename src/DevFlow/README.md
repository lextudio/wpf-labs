# WPF DevFlow

A WPF-first DevFlow product designed for classic WPF applications.

This folder contains a new local WPF DevFlow implementation that reuses generic DevFlow infrastructure from the external MAUI DevFlow repository where it makes sense.

## What is included

- `LeXtudio.DevFlow.Agent.Core` — WPF-agnostic DevFlow HTTP server, DTOs, and shared agent plumbing
- `LeXtudio.DevFlow.Agent.WPF` — WPF-specific visual tree walker, screenshot capture, and UI interaction support

## Reuse strategy

The WPF DevFlow projects reuse existing source from `external/maui-labs/src/DevFlow/Microsoft.Maui.DevFlow.Agent.Core` for:

- HTTP server (`AgentHttpServer.cs`)
- protocol DTOs and element model (`ElementInfo.cs`)
- agent configuration (`AgentOptions.cs`)
- DevFlow action attribute metadata (`DevFlowActionAttribute.cs`)

Those files are consumed as linked source files in the local `LeXtudio.DevFlow.Agent.Core` project.

## Build

From the repo root:

```powershell
cd src\DevFlow
dotnet build WpfDevFlow.sln
```

## Use

WPF apps can register the agent during startup:

```csharp
using LeXtudio.DevFlow.Agent.WPF;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        this.AddWpfDevFlowAgent();
    }
}
```

## Uno support preview

- `LeXtudio.DevFlow.Agent.Uno` is scaffolded as the initial Uno DevFlow platform package.
- `UnoDevFlow.sln` contains the shared agent core plus the new Uno project.
- The Uno package currently has skeleton tree-walking and registration helpers so Uno-specific work can be added in place.

## Notes

- The WPF DevFlow product is intentionally focused on classic WPF, not MAUI.
- `WpfDevFlow.sln` is the local WPF solution for this product.
- `DevFlow.slnf` is an existing external MAUI DevFlow wrapper and is unrelated to the new local WPF product.
