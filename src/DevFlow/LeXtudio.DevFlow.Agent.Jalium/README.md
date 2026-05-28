# LeXtudio.DevFlow.Agent.Jalium

DevFlow agent for Jalium apps. Exposes a local HTTP API that AI tools and the `dotnet jalex devflow` CLI can use to inspect and drive a running Jalium application.

## Install

```powershell
dotnet add package LeXtudio.DevFlow.Agent.Jalium
dotnet add package LeXtudio.DevFlow.Driver
```

## Register

After the Jalium application context is initialized:

```csharp
using LeXtudio.DevFlow.Agent.Jalium;

var agent = app.AddJaliumDevFlowAgent();
```

See `docs/devflow/howto-jalium.md` for full setup instructions.
