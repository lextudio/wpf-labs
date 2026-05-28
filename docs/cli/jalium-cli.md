# Jalium CLI Guide (`dotnet jalex`)

This guide explains how to use the Jalium CLI for app workflows and DevFlow agent operations.

## 1. Install

```powershell
dotnet tool install -g LeXtudio.Jalium.Cli
```

If already installed:

```powershell
dotnet tool update -g LeXtudio.Jalium.Cli
```

## 2. Check available commands

```powershell
dotnet jalex --help
```

Core commands:

- `doctor`
- `version`
- `new`
- `build`
- `run`
- `publish`
- `package`
- `diagnostics`
- `env`
- `devflow` (`status`, `screenshot`, `tap`)

## 3. Scaffold a new Jalium app

```powershell
dotnet jalex new MyJaliumApp
```

What happens:

- Creates folder `MyJaliumApp`
- Generates `MyJaliumApp.csproj`
- Generates `Program.cs`
- If run inside this repo layout, it also wires a local project reference to `LeXtudio.DevFlow.Agent.Jalium`

## 4. Build and run projects

From your project folder:

```powershell
dotnet jalex build
dotnet jalex run
```

Target a specific project:

```powershell
dotnet jalex build .\src\MyJaliumApp\MyJaliumApp.csproj
dotnet jalex run .\src\MyJaliumApp\MyJaliumApp.csproj
```

Common options:

- `-c|--configuration Debug|Release`
- `-r|--runtime <RID>`
- `--framework <TFM>`
- `--output <folder>` (for `build`/`publish`)

## 5. Publish and package

Publish:

```powershell
dotnet jalex publish .\src\MyJaliumApp\MyJaliumApp.csproj -c Release
```

Package (publishes first, then zips output as `publish.zip` in current directory):

```powershell
dotnet jalex package .\src\MyJaliumApp\MyJaliumApp.csproj -c Release
```

## 6. Environment and diagnostics

```powershell
dotnet jalex doctor
dotnet jalex env
dotnet jalex diagnostics
```

## 7. Use Jalium CLI with DevFlow

Assumes your app is running with DevFlow enabled on `localhost:9223`.

Check agent status:

```powershell
dotnet jalex devflow status
dotnet jalex devflow status --host localhost --port 9223
```

Capture screenshot:

```powershell
dotnet jalex devflow screenshot --output jalium-shot.png
```

Tap an element:

```powershell
dotnet jalex devflow tap --id <element-id>
```

## 8. Output modes for automation

Global options:

- `--json`
- `-v|--verbose`
- `--dry-run`
- `--ci`

Examples:

```powershell
dotnet jalex --json devflow status
dotnet jalex --dry-run package .\src\MyJaliumApp\MyJaliumApp.csproj
```

## 9. Expected behavior and exit codes

- Success returns exit code `0`.
- Unknown command/invalid options return non-zero.
- `devflow` commands return clear connectivity/action errors if the agent is unavailable.
- `package` generates `publish.zip` only after a successful publish.
