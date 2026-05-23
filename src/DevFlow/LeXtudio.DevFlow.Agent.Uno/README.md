# LeXtudio.DevFlow.Agent.Uno

Uno Platform DevFlow agent runtime for Uno and WinUI 3 applications.

This package extends `LeXtudio.DevFlow.Agent.Core` with Uno-compatible registration, visual tree traversal, and Web API support.

## Install

```powershell
dotnet add package LeXtudio.DevFlow.Agent.Uno
```

## What is included

- Uno/WinUI visual tree walker
- platform registration helpers for Uno app startup
- shared DevFlow HTTP API support

## Usage

Register the Uno DevFlow agent during app initialization:

```csharp
using LeXtudio.DevFlow.Agent.Uno;

builder.UseUnoDevFlowAgent();
```

## Getting Started

For detailed setup and configuration instructions, see the [Uno/WinUI 3 DevFlow Guide](https://github.com/lextudio/wpf-labs/blob/master/docs/devflow/howto-uno.md).

For common questions about port configuration and troubleshooting, see the [DevFlow FAQ](https://github.com/lextudio/wpf-labs/blob/master/docs/devflow/faq.md).

## Related Packages

- [LeXtudio.DevFlow.Agent.Core](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.Core)
- [LeXtudio.DevFlow.Driver](https://www.nuget.org/packages/LeXtudio.DevFlow.Driver)
- [LeXtudio.DevFlow.Agent.WPF](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.WPF)
- [LeXtudio.DevFlow.Agent.MewUI](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.MewUI)

## Compatibility

- .NET 10.0+
- Uno Platform and WinUI 3 apps
