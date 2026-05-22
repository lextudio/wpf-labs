# LeXtudio.DevFlow.Agent.MewUI

MewUI runtime package for LeXtudio DevFlow support in MewUI applications.

This package builds on `LeXtudio.DevFlow.Agent.Core` and adds runtime integration for `Aprillz.MewUI` based applications.

## Install

```powershell
dotnet add package LeXtudio.DevFlow.Agent.MewUI
```

## What is included

- MewUI runtime agent registration
- support for MewUI visual tree inspection
- shared DevFlow HTTP API integration

## Usage

Register the MewUI DevFlow agent when your app starts.

```csharp
Application.Current.AddMewUIDevFlowAgent();
```

## Related Packages

- [LeXtudio.DevFlow.Agent.Core](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.Core)
- [LeXtudio.DevFlow.Driver](https://www.nuget.org/packages/LeXtudio.DevFlow.Driver)
- [LeXtudio.DevFlow.Agent.WPF](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.WPF)
- [LeXtudio.DevFlow.Agent.Uno](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.Uno)

## Compatibility

- .NET 10.0+
- MewUI applications
