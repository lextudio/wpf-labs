# MewUI CLI Guide (`dotnet mewlex`)

This guide explains how to use the MewUI CLI for app workflows and DevFlow agent operations.

## 1. Install

```powershell
dotnet tool install -g LeXtudio.MewUI.Cli
```

If already installed:

```powershell
dotnet tool update -g LeXtudio.MewUI.Cli
```

## 2. Check available commands

```powershell
dotnet mewlex --help
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

## 3. Scaffold a new MewUI app

```powershell
dotnet mewlex new MyMewApp
```

What happens:

- Creates folder `MyMewApp`
- Generates `MyMewApp.csproj`
- Generates `Program.cs`
- If run inside this repo layout, it also wires a local project reference to `LeXtudio.DevFlow.Agent.MewUI`

## 4. Build and run projects

From your project folder:

```powershell
dotnet mewlex build
dotnet mewlex run
```

Target a specific project:

```powershell
dotnet mewlex build .\src\MyMewApp\MyMewApp.csproj
dotnet mewlex run .\src\MyMewApp\MyMewApp.csproj
```

Common options:

- `-c|--configuration Debug|Release`
- `-r|--runtime <RID>`
- `--framework <TFM>`
- `--output <folder>` (for `build`/`publish`)

## 5. Publish and package

Publish:

```powershell
dotnet mewlex publish .\src\MyMewApp\MyMewApp.csproj -c Release
```

Package (publishes first, then zips output as `publish.zip` in current directory):

```powershell
dotnet mewlex package .\src\MyMewApp\MyMewApp.csproj -c Release
```

## 6. Environment and diagnostics

```powershell
dotnet mewlex doctor
dotnet mewlex env
dotnet mewlex diagnostics
```

## 7. Use MewUI CLI with DevFlow

Assumes your app is running with DevFlow enabled on `localhost:5500`.

Check agent status:

```powershell
dotnet mewlex devflow status
dotnet mewlex devflow status --host localhost --port 5500
```

Capture screenshot:

```powershell
dotnet mewlex devflow screenshot --output mewui-shot.png
```

Tap an element:

```powershell
dotnet mewlex devflow tap --id <element-id>
```

## 8. Output modes for automation

Global options:

- `--json`
- `-v|--verbose`
- `--dry-run`
- `--ci`

Examples:

```powershell
dotnet mewlex --json devflow status
dotnet mewlex --dry-run package .\src\MyMewApp\MyMewApp.csproj
```

## 9. Expected behavior and exit codes

- Success returns exit code `0`.
- Unknown command/invalid options return non-zero.
- `devflow` commands return clear connectivity/action errors if the agent is unavailable.
- `package` generates `publish.zip` only after a successful publish.
