# LeXtudio Uno CLI

A command-line tool for Uno development environment workflows, project scaffolding, packaging, and DevFlow integration.

## Package

[![LeXtudio.Uno.Cli](https://img.shields.io/nuget/v/LeXtudio.Uno.Cli.svg?label=Uno%20CLI)](https://www.nuget.org/packages/LeXtudio.Uno.Cli)
[![LeXtudio.Uno.Cli Downloads](https://img.shields.io/nuget/dt/LeXtudio.Uno.Cli.svg?label=Downloads)](https://www.nuget.org/packages/LeXtudio.Uno.Cli)

## Install

```powershell
dotnet tool install -g LeXtudio.Uno.Cli
```

## Documentation

- [Uno CLI Guide](https://github.com/lextudio/wpf-labs/blob/master/docs/cli/uno-cli.md)

## Quick Start

```powershell
dotnet unolex doctor
```

Create a new Uno app using the official Uno Skia desktop template:

```powershell
dotnet unolex new MyUnoApp
```

Build and run:

```powershell
dotnet unolex build
dotnet unolex run
```

## Core Commands

| Command | Description |
|---------|-------------|
| `dotnet unolex doctor` | Validate the Uno development environment. |
| `dotnet unolex version` | Display CLI and environment version information. |
| `dotnet unolex new` | Scaffold a new Uno application. |
| `dotnet unolex build` | Build an Uno project. |
| `dotnet unolex run` | Run an Uno application. |
| `dotnet unolex publish` | Publish an Uno application. |
| `dotnet unolex package` | Package Uno output artifacts. |
| `dotnet unolex diagnostics` | Run Uno diagnostics and validation. |
| `dotnet unolex env` | Inspect installed SDKs and tooling. |
| `dotnet unolex devflow` | Query a running Uno DevFlow agent. |

## DevFlow Commands

```powershell
dotnet unolex devflow status --host localhost --port 9223
```

Capture a screenshot:

```powershell
dotnet unolex devflow screenshot --host localhost --port 9223 --output screenshot.png
```

## Output and Automation

| Option | Description |
|--------|-------------|
| `--json` | Emit structured JSON output for scripting and CI. |
| `-v`, `--verbose` | Enable verbose diagnostic output. |
| `--dry-run` | Show planned actions without changing the system. |
| `--ci` | Run in CI-friendly non-interactive mode. |

## Notes

This CLI is the Uno toolchain companion for DevFlow-enabled apps.
