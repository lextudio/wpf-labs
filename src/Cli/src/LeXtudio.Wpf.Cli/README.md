# LeXtudio WPF CLI

A command-line tool for WPF project workflows, diagnostics, packaging, and DevFlow integration.

## Package

[![LeXtudio.Wpf.Cli](https://img.shields.io/nuget/v/LeXtudio.Wpf.Cli.svg?label=WPF%20CLI)](https://www.nuget.org/packages/LeXtudio.Wpf.Cli)
[![LeXtudio.Wpf.Cli Downloads](https://img.shields.io/nuget/dt/LeXtudio.Wpf.Cli.svg?label=Downloads)](https://www.nuget.org/packages/LeXtudio.Wpf.Cli)

## Install

```powershell
dotnet tool install -g LeXtudio.Wpf.Cli
```

## Quick Start

```powershell
dotnet wpflex doctor
```

Create a new WPF project:

```powershell
dotnet wpflex new app --name MyWpfApp --framework net8.0-windows
```

Build and run:

```powershell
dotnet wpflex build
dotnet wpflex run
```

## Core Commands

| Command | Description |
|---------|-------------|
| `dotnet wpflex doctor` | Validate the WPF development environment and surface missing toolchain components. |
| `dotnet wpflex version` | Display CLI and environment version information. |
| `dotnet wpflex new` | Scaffold a new WPF app or library. |
| `dotnet wpflex build` | Build a WPF project with WPF-aware defaults. |
| `dotnet wpflex run` | Run a WPF application. |
| `dotnet wpflex publish` | Publish a WPF application for deployment. |
| `dotnet wpflex package` | Package WPF outputs for distribution. |
| `dotnet wpflex diagnostics` | Run WPF-specific diagnostics and validation checks. |
| `dotnet wpflex env` | Inspect installed SDKs, tooling, and environment status. |

## DevFlow Commands

Query a running WPF DevFlow agent:

```powershell
dotnet wpflex devflow status --host localhost --port 5500
```

Use `--json` for scripting:

```powershell
dotnet wpflex --json devflow status
```

## Output and Automation

| Option | Description |
|--------|-------------|
| `--json` | Emit structured JSON output for scripting and CI. |
| `-v`, `--verbose` | Enable verbose diagnostic output. |
| `--dry-run` | Show planned actions without changing the system. |
| `--ci` | Run in CI-friendly non-interactive mode. |

## Notes

This CLI is a WPF-focused tool that mirrors MAUI CLI design patterns and is intended for environment validation, project workflows, and DevFlow connectivity.
