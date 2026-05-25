# WinForms CLI Guide (`dotnet winflex`)

This guide explains how to use the WinForms CLI for project workflows and DevFlow agent operations.

## 1. Install

```powershell
dotnet tool install -g LeXtudio.WinForms.Cli
```

If already installed:

```powershell
dotnet tool update -g LeXtudio.WinForms.Cli
```

## 2. Check available commands

```powershell
dotnet winflex --help
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

## 3. Scaffold a new WinForms app

```powershell
dotnet winflex new MyWinFormsApp
```

What happens:

- Creates folder `MyWinFormsApp`
- Generates `MyWinFormsApp.csproj`
- Generates `Program.cs`

## 4. Build and run projects

From a solution/project folder:

```powershell
dotnet winflex build
dotnet winflex run
```

Target a specific project:

```powershell
dotnet winflex build .\src\MyWinFormsApp\MyWinFormsApp.csproj
dotnet winflex run .\src\MyWinFormsApp\MyWinFormsApp.csproj
```

Common build options:

- `-c|--configuration Debug|Release`
- `-r|--runtime <RID>`
- `--framework <TFM>`
- `--output <folder>` (for `build`/`publish`)

## 5. Publish and package

Publish:

```powershell
dotnet winflex publish .\src\MyWinFormsApp\MyWinFormsApp.csproj -c Release
```

Package (publishes first, then zips output as `publish.zip` in current directory):

```powershell
dotnet winflex package .\src\MyWinFormsApp\MyWinFormsApp.csproj -c Release
```

## 6. Environment and diagnostics

Validate CLI environment:

```powershell
dotnet winflex doctor
```

Show runtime + SDK environment:

```powershell
dotnet winflex env
dotnet winflex diagnostics
```

## 7. Use WinForms CLI with DevFlow

Assumes your app is running with DevFlow agent enabled (default `localhost:9223`).

Check agent status:

```powershell
dotnet winflex devflow status
dotnet winflex devflow status --host localhost --port 9223
```

Capture screenshot:

```powershell
dotnet winflex devflow screenshot --output winforms-shot.png
```

Tap a UI element by DevFlow element id:

```powershell
dotnet winflex devflow tap --id <element-id>
```

## 8. Output modes for automation

Global options (can appear before command):

- `--json` for machine-readable output
- `-v|--verbose` for verbose logs
- `--dry-run` to preview actions
- `--ci` for CI-friendly mode

Example:

```powershell
dotnet winflex --json devflow status
dotnet winflex --dry-run package .\src\MyWinFormsApp\MyWinFormsApp.csproj
```

## 9. Expected behavior and exit codes

- Success returns exit code `0`.
- Unknown command/invalid options return non-zero.
- `devflow status|screenshot|tap` return actionable error messages when agent is unreachable.
- `package` fails if publish fails; zip is only generated after successful publish.
