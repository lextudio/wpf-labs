# Desktop DevFlow

A Windows desktop DevFlow product designed for classic WPF and WinForms applications, with additional Uno and MewUI coverage.

This folder contains the shared DevFlow runtime packages for WPF, WinForms, WinUI 3, Uno Platform, and MewUI applications.

Each package has its own package-specific README for the most relevant installation and usage guidance:

- `LeXtudio.DevFlow.Agent.Core/README.md`
- `LeXtudio.DevFlow.Agent.WPF/README.md`
- `LeXtudio.DevFlow.Agent.WinForms/README.md`
- `LeXtudio.DevFlow.Agent.Uno/README.md`
- `LeXtudio.DevFlow.Agent.MewUI/README.md`
- `LeXtudio.DevFlow.Driver/README.md`

## NuGet packages

[![LeXtudio.DevFlow.Agent.Core](https://img.shields.io/nuget/v/LeXtudio.DevFlow.Agent.Core.svg)](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.Core)
[![LeXtudio.DevFlow.Agent.WPF](https://img.shields.io/nuget/v/LeXtudio.DevFlow.Agent.WPF.svg)](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.WPF)
[![LeXtudio.DevFlow.Agent.WinForms](https://img.shields.io/nuget/v/LeXtudio.DevFlow.Agent.WinForms.svg)](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.WinForms)
[![LeXtudio.DevFlow.Agent.Uno](https://img.shields.io/nuget/v/LeXtudio.DevFlow.Agent.Uno.svg)](https://www.nuget.org/packages/LeXtudio.DevFlow.Agent.Uno)
[![LeXtudio.DevFlow.Driver](https://img.shields.io/nuget/v/LeXtudio.DevFlow.Driver.svg)](https://www.nuget.org/packages/LeXtudio.DevFlow.Driver)

Install the runtime package for your UI stack:

```powershell
dotnet add package LeXtudio.DevFlow.Agent.WPF
dotnet add package LeXtudio.DevFlow.Driver
```

```powershell
dotnet add package LeXtudio.DevFlow.Agent.WinForms
dotnet add package LeXtudio.DevFlow.Driver
```

```powershell
dotnet add package LeXtudio.DevFlow.Agent.Uno
dotnet add package LeXtudio.DevFlow.Driver
```

## What is included

- `LeXtudio.DevFlow.Agent.Core` — UI-stack-agnostic DevFlow HTTP server, DTOs, and shared agent plumbing
- `LeXtudio.DevFlow.Agent.WPF` — WPF-specific visual tree walker, screenshot capture, and UI interaction support
- `LeXtudio.DevFlow.Agent.WinForms` — WinForms-specific control tree walker, screenshot capture, and UI interaction support
- `LeXtudio.DevFlow.Agent.Uno` — Uno Platform and WinUI 3 registration and visual tree support
- `LeXtudio.DevFlow.Agent.MewUI` — MewUI runtime support via NuGet-deployed Aprillz.MewUI packages
- `LeXtudio.DevFlow.Driver` — HTTP client for querying a running DevFlow agent

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

Uno Platform and WinUI 3 apps can register the Uno agent:

```csharp
using LeXtudio.DevFlow.Agent.Uno;

builder.UseUnoDevFlowAgent();
```

WinForms apps can register the agent through an `ApplicationContext`:

```csharp
using LeXtudio.DevFlow.Agent.WinForms;
using Microsoft.Maui.DevFlow.Agent.Core;

var form = new MainForm();
var context = new ApplicationContext(form);
context.AddWinFormsDevFlowAgent(new AgentOptions { Port = 9223 });
Application.Run(context);
```

## Web API

By default, the sample apps start the agent on port `9223`.
You can override the port at build time with `dotnet build -p:MauiDevFlowPort=9500` or by adding a `.mauidevflow` file to your project directory.

| Request | Description |
|---------|-------------|
| `GET /api/v1/agent/status` | Read agent status. |
| `GET /api/v1/ui/tree` | Read the live UI tree. |
| `GET /api/v1/ui/element?id=<id>` | Read one UI element by id. |
| `GET /api/v1/ui/screenshot` | Capture a screenshot. |
| `GET /api/v1/ui/screenshot?id=<id>` | Capture a screenshot for one element/control. |
| `GET /api/v1/ui/screenshot?selector=%23<id>` | Capture a screenshot using a selector. |
| `GET /api/v1/webview/contexts` | List discoverable WebView contexts. |
| `GET /api/v1/webview/screenshot?context=<id>` | Capture a WebView screenshot. |
| `POST /api/v1/webview/cdp` | Execute a WebView CDP command. |
| `POST /api/v1/ui/tap` | Tap an element with body `{ "id": "<element-id>" }`. |
| `POST /api/v1/ui/actions/fill` | Fill text with body `{ "elementId": "<element-id>", "text": "value" }`. |
| `POST /api/v1/ui/actions/clear` | Clear text with body `{ "elementId": "<element-id>" }`. |
| `POST /api/v1/ui/actions/focus` | Focus an element with body `{ "elementId": "<element-id>" }`. |
| `POST /api/v1/ui/actions/key` | Send key/text input with body `{ "elementId": "<element-id>", "text": "A" }`. |
| `POST /api/v1/ui/actions/scroll` | Scroll an element with body `{ "id": "<element-id>", "deltaX": 0, "deltaY": 600 }`. |

## WinForms support

- `LeXtudio.DevFlow.Agent.WinForms` is the WinForms DevFlow runtime package.
- `WinFormsDevFlowTestApp` is a reference sample project demonstrating a process-local DevFlow HTTP agent in a classic WinForms app.
- `LeXtudio.DevFlow.Agent.WinForms.Tests` covers status, tree inspection, query, screenshots, tap, fill, clear, focus, key, scroll, and structured error behavior.
- WinForms WebView/CDP and app theme APIs are currently not advertised as supported capabilities.

## Uno support preview

- `LeXtudio.DevFlow.Agent.Uno` is the Uno DevFlow platform package.
- `UnoDevFlow.sln` contains the shared agent core plus the Uno project.
- The Uno package supports registration, tree walking, screenshots, tap, and scroll through the shared Web API.
- Uno WebView/CDP support is currently best-effort and target-dependent.
- WebView integration tests currently run on `net10.0-desktop` only; `net10.0-windows10.0.19041.0` (WinUI) WebView tests are temporarily excluded due to unstable build/runtime behavior in CI/dev environments.

## MewUI support preview

- `LeXtudio.DevFlow.Agent.MewUI` is a new MewUI runtime package that uses `Aprillz.MewUI.Core` and `Aprillz.MewUI.Platform.Win32`.
- `MewUIDevFlowTestApp` is a reference sample project demonstrating how to start a MewUI app and host the DevFlow HTTP agent.
- Register DevFlow during your app startup with `Application.Current.AddMewUIDevFlowAgent()`.

## Reuse strategy

The DevFlow projects reuse source from `external/maui-labs/src/DevFlow/Microsoft.Maui.DevFlow.Agent.Core` where it makes sense. Those files are consumed as linked source files in `LeXtudio.DevFlow.Agent.Core`.

## Notes

- The Desktop DevFlow product is focused on WPF, WinForms, WinUI 3, Uno Platform, and MewUI, not MAUI.
- `WpfDevFlow.sln` is the local desktop solution for this product.
- `DevFlow.slnf` is an external MAUI DevFlow wrapper that now also references the local WinForms projects used by this product.
