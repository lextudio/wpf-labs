# DevFlow Port Plan (MAUI -> WPF/Uno/MewUI)

## Scope rule
- Port only features that already exist in MAUI DevFlow contract.
- Do not invent new protocol fields/endpoints unless MAUI adds them first.

## Already ported
- Core agent status and UI inspection:
  - `GET /api/v1/agent/status`
  - `GET /api/v1/ui/tree`
  - `GET /api/v1/ui/element`
  - `GET /api/v1/ui/elements`
  - `GET /api/v1/ui/screenshot` (full + element; selector where supported)
- UI actions:
  - `POST /api/v1/ui/tap`
  - `POST /api/v1/ui/actions/scroll`
  - `POST /api/v1/ui/actions/fill`
  - `POST /api/v1/ui/actions/clear`
  - `POST /api/v1/ui/actions/focus`
  - `POST /api/v1/ui/actions/key`
  - `POST /api/v1/ui/actions/back`
  - `POST /api/v1/ui/actions/batch`
- WebView surface currently in local code:
  - `GET /api/v1/webview/contexts`
  - `GET /api/v1/webview/screenshot`
  - `POST /api/v1/webview/cdp`
- Integration tests in place for WPF/Uno/MewUI for status, screenshot, tap/scroll, and batch baseline.
- Invoke parity baseline:
  - `GET /api/v1/invoke/actions`
  - `POST /api/v1/invoke/actions/{name}`

## Feature comparison
| Feature | MAUI | wpf-labs status | Notes |
|---|---|---|---|
| Agent status/tree/element/screenshot | Yes | Done | Parity baseline complete |
| UI actions: tap/fill/clear/focus/key/back/scroll | Yes | Done | Parity baseline complete |
| UI actions: batch | Yes | Done | Added + integration tests |
| Invoke: list/invoke actions | Yes | Done (baseline) | Static public action discovery + invoke |
| WebView contexts/screenshot/cdp | Yes | Done (baseline) | DOM/input helper endpoints still missing |
| WebView DOM/query/navigate/input APIs | Yes | Missing | Next WebView parity slice |
| Profiler APIs | Yes | Missing | Capabilities/sessions/samples/spans |
| UI actions: navigate/gesture/resize | Yes | Missing | Keep MAUI payload parity |

## Still missing vs MAUI
- MAUI `invoke` action system:
  - `GET /api/v1/invoke/actions`
  - `POST /api/v1/invoke/actions/{name}`
  - Runtime action discovery/execution plumbing
- MAUI profiler API family:
  - `/api/v1/profiler/capabilities`
  - `/api/v1/profiler/sessions` (create/delete)
  - `/api/v1/profiler/sessions/{id}/samples`
  - `/api/v1/profiler/spans`
- MAUI richer WebView endpoints (beyond raw CDP passthrough), for example:
  - `/api/v1/webview/dom`
  - `/api/v1/webview/dom/query`
  - `/api/v1/webview/navigate`
  - `/api/v1/webview/input/*`
- MAUI action endpoints not yet present locally:
  - `POST /api/v1/ui/actions/navigate`
  - `POST /api/v1/ui/actions/gesture`
  - `POST /api/v1/ui/actions/resize` (if we keep strict MAUI parity, this must match MAUI semantics)
- Capability parity gaps:
  - MAUI-style capability documents for jobs/profiler/invoke feature sets.

## Runtime caveats
- Uno WinUI target (`net10.0-windows10.0.19041.0`) still has intermittent/blocked test-host build issues (XAML compiler), so WebView-related integration validation should remain desktop-only for now.
- WPF supports WebView runtime paths; keep tests scoped to stable scenarios to avoid long hangs.

## Recommended next port order
1. Profiler API parity (capabilities/sessions/samples/spans) with capability flags aligned to MAUI.
2. UI action parity endpoints (`navigate`, `gesture`, `resize`) only with MAUI-compatible payloads.
3. Rich WebView DOM/navigation/input helpers (layered over CDP where applicable), with opt-in tests per runtime.

## Acceptance criteria for each new port
- Contract matches MAUI route + payload names.
- No changes required in `external/maui-labs` sources.
- Integration test added for WPF and for Uno/MewUI where runtime support is real.
- Capability flags updated only for existing MAUI features.
