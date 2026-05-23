# CLI Port Plan (MAUI DevFlow Driver -> wpf-labs CLI)

## Goal
- Bring CLI/driver behavior to MAUI parity for features already present in MAUI DevFlow.

## Current status
- Core UI routes are available in local agent implementations.
- Batch action route is available and covered by focused integration tests.
- WebView raw context/screenshot/CDP endpoints exist locally.
- Invoke route baseline exists in local agents.

## Feature comparison
| CLI capability | MAUI driver | wpf-labs status | Notes |
|---|---|---|---|
| Basic UI inspect/actions commands | Yes | Partial | Needs final command parity audit |
| Batch actions command | Yes | Partial | Route exists; confirm CLI wrapper parity |
| Invoke actions (list/run) | Yes | Missing in CLI | Agent route now exists; CLI wrapper next |
| Profiler commands | Yes | Missing | Full endpoint family not ported |
| WebView CDP command | Yes | Partial | Raw CDP route exists |
| WebView DOM/query/input/navigation commands | Yes | Missing | Depends on agent-side endpoint parity |
| Capability-aware command gating | Yes | Missing | Needed for UX and reliable CI |

## Missing CLI/driver parity items
- Add/finish MAUI-equivalent client wrappers for:
  - `/api/v1/invoke/actions`
  - `/api/v1/invoke/actions/{name}`
  - `/api/v1/profiler/*` endpoint family
  - UI actions not yet wrapped in local CLI: `navigate`, `gesture`, `resize`
  - Rich webview helpers: DOM/query/navigation/input wrappers where we adopt those server endpoints
- Capability-aware command gating:
  - CLI should check agent capabilities and fail with clear guidance when unsupported.
- Test coverage:
  - CLI-level tests for invoke/profiler/webview commands
  - Golden/snapshot checks for CLI JSON output format where applicable

## Proposed work order
1. `invoke` command surface and typed client support.
2. Profiler commands (start/stop session, stream/pull samples, emit spans).
3. Remaining UI action commands (`navigate`, `gesture`, `resize`) with MAUI payload parity.
4. WebView DOM/input/navigation commands once agent-side endpoints are aligned.

## Out of scope for now
- New protocol inventions not present in MAUI DevFlow.
- Runtime-specific CLI behavior that would break cross-framework consistency.

## Done definition
- Commands map 1:1 to MAUI-compatible endpoints/payloads.
- Capability checks are explicit and user-friendly.
- Unit/integration tests pass without requiring changes to `external/maui-labs`.
