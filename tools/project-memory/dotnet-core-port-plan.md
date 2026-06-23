# .NET Core Port Plan

Goal: rebuild the same minimal MultiSnap workflow on a modern Windows desktop stack after the Electron MVP is stable.

## First Port Scope

- .NET 8 or newer WPF desktop app.
- Tray-resident process with context menu actions.
- Global hotkey for region capture.
- Fullscreen transparent region-selection overlay.
- Full-window post-capture editor matching the current Electron MVP.
- Copy to clipboard and save to PNG.
- Local JSON settings.
- Serilog local logging.

## Progress

2026-05-21:

- Created `dotnet/MultiSnap` as a .NET 8 WPF app.
- Added tray actions for area capture, full-screen capture, show, and quit.
- Added Ctrl+PrintScreen global hotkey registration for area capture.
- Added frozen-screen region overlay and a first editor window with ink, copy, and PNG save.
- Added `%APPDATA%/MultiSnap/settings.json` and daily Serilog file logs under `%APPDATA%/MultiSnap/logs`.
- Verified with `dotnet build dotnet\MultiSnap\MultiSnap.csproj`.

2026-06-23:

- Added an editor toolbar brush-size slider for ink annotations. New strokes use the selected thickness.

Current first-slice limitations:

- Annotation is first-pass ink only, not full Electron tool parity yet.
- Hotkey UI reports Ctrl+PrintScreen status but does not yet expose editable shortcut choices.

## Later

- Installer/package pipeline.
- Active-window capture.
- Video recording with ScreenRecorderLib/FFmpeg.
- WebView2 auth flows.
- Cloud integrations.
- OCR/AI/OpenAI features.
- Sentry/error reporting.

## Not In The First Port

- Squirrel.Windows compatibility.
- Cloud upload providers.
- Account/login UI.
- Screen recording.
- AI/OCR.
- Full Monosnap feature parity.
