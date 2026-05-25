# WinFormsDevFlowTestApp

This sample WinForms app demonstrates a classic WinForms application instrumented with `LeXtudio.DevFlow.Agent.WinForms`.

## Run

```powershell
cd src\DevFlow\WinFormsDevFlowTestApp
dotnet run
```

The app starts a DevFlow agent on port `9223` by default. Tests can override this with the `DEVFLOW_AGENT_PORT` environment variable.

## DevFlow endpoints

Useful smoke checks:

```powershell
Invoke-RestMethod http://localhost:9223/api/v1/agent/status
Invoke-RestMethod http://localhost:9223/api/v1/ui/tree
```

The sample includes controls named `ActionButton`, `InputBox`, `ResponseLabel`, and `MainScrollPanel` so integration tests can validate tap, fill, clear, focus, key, scroll, and screenshot behavior.
