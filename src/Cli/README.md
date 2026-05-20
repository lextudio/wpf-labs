# LeXtudio WPF CLI

A command-line tool for WPF development environment validation, project workflows, packaging, and diagnostics.

> ⚠️ **Experimental** — APIs and command names may evolve. This project is intended as a foundation for a WPF CLI experience modeled after the MAUI CLI design.

## Package

[![LeXtudio.Wpf.Cli](https://img.shields.io/nuget/v/LeXtudio.Wpf.Cli.svg)](https://www.nuget.org/packages/LeXtudio.Wpf.Cli)
[![NuGet Downloads](https://img.shields.io/nuget/dt/LeXtudio.Wpf.Cli.svg)](https://www.nuget.org/packages/LeXtudio.Wpf.Cli)

| Package | Description |
|---------|-------------|
| **LeXtudio.Wpf.Cli** | Global CLI tool (`dotnet wpflex`) for WPF project workflows, diagnostics, and packaging. |

## Quick Start

### 1. Install the CLI tool

```powershell
dotnet tool install -g LeXtudio.Wpf.Cli
```

### 2. Check your environment

```powershell
dotnet wpflex doctor
```

### 3. Create a new WPF project

```powershell
dotnet wpflex new app --name MyWpfApp --framework net8.0-windows
```

### 4. Build and run

```powershell
dotnet wpflex build
dotnet wpflex run
```

## Core Commands

| Command | Description |
|---------|-------------|
| `dotnet wpflex doctor` | Validate the WPF development environment and surface missing toolchain components. |
| `dotnet wpflex version` | Display CLI and environment version information. |
| `dotnet wpflex new` | Scaffold a new WPF app or library from templates. |
| `dotnet wpflex build` | Build a WPF project with WPF-aware defaults. |
| `dotnet wpflex run` | Run a WPF application. |
| `dotnet wpflex publish` | Publish a WPF application for deployment. |
| `dotnet wpflex package` | Package WPF outputs, including MSIX or self-contained bundles. |
| `dotnet wpflex diagnostics` | Run WPF-specific diagnostics and validation checks. |
| `dotnet wpflex env` | Inspect installed SDKs, tooling, and Windows environment status. |

Run `dotnet wpflex <command> --help` for detailed options on any command.

## DevFlow Commands

The CLI can query a running WPF DevFlow agent.

```powershell
dotnet wpflex devflow status --host localhost --port 5500
```

Use `--json` when scripting:

```powershell
dotnet wpflex --json devflow status
```

Runtime UI actions such as tap and scroll are currently exposed by the DevFlow Web API rather than dedicated CLI subcommands.

| Request | Description |
|---------|-------------|
| `GET /api/v1/agent/status` | Read agent status. |
| `GET /api/v1/ui/tree` | Read the live UI tree. |
| `GET /api/v1/ui/element?id=<id>` | Read one UI element by id. |
| `GET /api/v1/ui/screenshot` | Capture a screenshot. |
| `POST /api/v1/ui/tap` | Tap an element with body `{ "id": "<element-id>" }`. |
| `POST /api/v1/ui/actions/scroll` | Scroll an element with body `{ "id": "<element-id>", "deltaX": 0, "deltaY": 600 }`. |

## Output and Automation

The WPF CLI should support a human-friendly interactive console experience by default and a machine-readable JSON mode using `--json` for automation.

| Option | Description |
|--------|-------------|
| `--json` | Emit structured JSON output for scripting and CI. |
| `-v`, `--verbose` | Enable verbose diagnostic output. |
| `--dry-run` | Show planned actions without changing the system. |
| `--ci` | Run in CI-friendly non-interactive mode. |

## Foundation

This repository is the foundation for a WPF CLI that mirrors MAUI CLI design principles:

- nested command groups and clear surface area
- shared output layers for interactive and JSON modes
- platform-aware environment checks and diagnostics
- extensible provider architecture for Windows tooling
- stable error categories and remediation guidance

See `DESIGN.md` for the proposed architecture, command hierarchy, and implementation plan.
