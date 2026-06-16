# Pending Tasks

Use this file for active project-wide plans and multi-step work.

Keep entries concise and task-relevant. Do not store full diffs, large logs,
generated outputs, secrets, credentials, or private production data.

## Status Markers

- `[ ]` not started
- `[~]` in progress
- `[x]` done
- `[!]` blocked or needs attention

## Tasks

### Build 0.1.13 Installer

Goal: produce the Windows installer for the current MultiSnap 0.1.13 build.

Execution order:

- [~] Confirm project-local version and installer script.
- [x] Confirm project-local version and installer script.
- [x] Run project checks and production build.
- [x] Run the installer build script.
- [x] Verify the generated setup artifact.

### Screen Recording .NET Sprint 1

Goal: start the screen recording feature as .NET-only work and seed settings with Ctrl+Shift+PrintScreen for video recording mode.

Planned changes:

- [x] Capture the .NET-only recording architecture decision.
- [x] Add recording settings defaults, including Ctrl+Shift+PrintScreen.
- [x] Add a recording state-machine/service skeleton.
- [x] Wire video hotkey registration to a safe placeholder flow.
- [x] Add WPF overlay mode for video-area selection.
- [x] Replace the modal recording placeholder with a floating Stop/Cancel controller.
- [x] Open a recording result window after Stop with preview and file actions.
- [x] Verify the .NET build.

### WPF Overlay Right-Click Cancel

Goal: make right-click close/cancel the .NET capture overlay immediately, including before any selection starts.

Planned changes:

- [x] Inspect WPF overlay mouse handlers.
- [x] Add right-click cancellation at the overlay root.
- [x] Verify .NET build.
- [x] Restart the WPF app.

### Capture Overlay Selection Reset

Goal: make mouse cancellation reset the active capture area selection reliably.

Planned changes:

- [x] Locate current overlay pointer/mouse handling.
- [x] Handle right-click cancellation for the active selection state.
- [x] Reset a short left-click selection instead of leaving an empty rectangle.
- [x] Verify TypeScript checks.

### Editor Toolbar Button Contrast Regression

Goal: make the editor toolbar actions readable against the dark header again.

Planned changes:

- [x] Locate the toolbar markup and CSS for Copy / Save PNG / Clear Ink.
- [x] Increase button contrast and preserve compact toolbar layout.
- [x] Verify WPF build.

### Edited Image Copy Alignment

Goal: make copied/saved annotated screenshots match the editor preview without clipping or shifted strokes.

Planned changes:

- [x] Inspect editor canvas coordinate mapping and export rendering.
- [x] Reuse one render path for preview, copy, and save.
- [x] Verify project checks.

### Title Bar Action Button Visibility

Goal: make the title bar actions readable on the dark editor header.

Planned changes:

- [x] Inspect the title bar button markup and CSS.
- [x] Adjust button styling so labels and icons have sufficient contrast.
- [x] Verify renderer checks.

### Auto-Bump Installer Version On Source Changes

Goal: make installer builds create a new patch version only when relevant project inputs changed since the last successful installer build.

Planned changes:

- [x] Define a source fingerprint that excludes generated build output and local logs.
- [x] Sync bumped versions across `package.json`, `package-lock.json`, `.csproj`, and Inno metadata.
- [x] Store the last successful C# build fingerprint.
- [x] Verify the C# build uses the computed version and creates a baseline fingerprint.

### Cancel Area Selection On Right Click

Goal: reset the in-progress capture selection when right-click is pressed while left mouse is held.

Planned changes:

- [x] Inspect capture overlay mouse handling.
- [x] Reset active selection on right-click during drag.
- [x] Verify TypeScript checks.

### Current Installer Build

Goal: produce the Windows installer for the current MultiSnap 0.1.0 build.

Execution order:

- [x] Confirm project-local version and installer script.
- [x] Run the installer build script.
- [x] Verify the generated setup artifact.

### Editor Clipboard Button Visibility

Goal: make the editor copy-to-clipboard action visible and reachable in the current app layout.

Planned changes:

- [x] Inspect the editor toolbar layout and reproduce the likely clipping cause.
- [x] Adjust renderer layout/CSS so the editor toolbar stays inside the workspace.
- [x] Verify TypeScript checks.

### Build 0.1.0 Installer

Goal: produce the Windows installer for the current MultiSnap 0.1.0 build.

Execution order:

- [x] Confirm project-local version and installer script.
- [x] Run the installer build script.
- [x] Verify the generated setup artifact.

### Copy Edited Screenshot To Clipboard

Goal: add an obvious editor action that copies the currently annotated screenshot back to the clipboard.

Planned changes:

- [x] Reuse the existing Electron clipboard IPC path.
- [x] Export the editor image with all current annotations before copying.
- [x] Add a clear toolbar button/status for the copy action.
- [x] Verify TypeScript checks.

### Monosnap-Style Electron MVP

Goal: make the current Electron prototype match the first requested Monosnap-style workflow: tray resident process, region selection, and full-window editor after capture.

Planned changes:

- [x] Keep the process accessible from the tray with capture/show/quit actions.
- [x] Show a full-window editor immediately after selecting a screen area.
- [x] Keep area selection as the first primary workflow.
- [x] Verify Electron/renderer type checks.

Deferred plan:

- [x] Capture the .NET Core port plan in project memory.
- [ ] Port the same MVP workflow to a modern .NET desktop stack.
- [ ] Add packaging/installer after the MVP workflow is stable.
- [ ] Revisit cloud upload, WebView auth, screen recording, OCR/AI, and integrations after the core capture/editor path works.

### Overlay Alpha Visibility Fix

Goal: keep area capture usable by showing the frozen screenshot through a light alpha overlay instead of a fully dark screen.

Planned changes:

- [x] Make overlay layers explicit and non-intercepting.
- [x] Keep a subtle alpha shade visible while selecting.
- [x] Verify renderer and Electron type checks.

Verification:

- [x] npm run check.

### Screenshot Hotkey Settings

Goal: add first-pass screenshot hotkey settings and make defaults match the requested workflow.

Planned changes:

- [x] Persist screenshot hotkey settings in Electron userData.
- [x] Register PrintScreen for frozen area selection and Ctrl+PrintScreen for full display capture.
- [x] Add a compact settings UI for choosing the two screenshot hotkeys.
- [x] Capture the display under the cursor for full-screen and overlay workflows.
- [x] Show whether Electron actually registered each global hotkey.
- [x] Capture the frozen screen frame before opening the selection overlay.
- [x] Copy the resulting screenshot to the clipboard immediately after capture.

Execution order:

- [x] Inspect current capture and shortcut paths.
- [x] Patch main/preload/renderer types and UI.
- [x] Run type checks.

Risks or dependencies:

- [!] Electron globalShortcut may fail if another app owns PrintScreen.

Verification:

- [x] npm run check.
- [x] npm run build.

### Ctrl+PrintScreen Area Capture

Goal: make Ctrl+PrintScreen launch the primary screenshot area-selection workflow.

Planned changes:

- [x] Move the area-capture default to Ctrl+PrintScreen.
- [x] Move full-display capture off Ctrl+PrintScreen to avoid shortcut conflicts.
- [x] Preserve existing user settings except for the prior legacy default pair.
- [x] Run type checks.

Verification:

- [x] npm run check.
- [x] Dev app launched with npm run dev.

### TODO Task Name

Goal: TODO

Planned changes:

- [ ] TODO

Execution order:

- [ ] TODO

Risks or dependencies:

### .NET Video Recording Fix

Goal: make the WPF screen recording flow produce a playable video file and expose preview controls in the result window.

Planned changes:

- [x] Replace the recording placeholder with a real Windows recording backend.
- [x] Add explicit player controls to the recording result window.
- [x] Verify the .NET build and update notes with any runtime caveats.

Verification:

- [x] dotnet build dotnet\MultiSnap\MultiSnap.csproj.

Runtime note: recording now uses ScreenRecorderLib/Media Foundation and the WPF host builds x64 so the native recorder assembly is selected.

- [ ] TODO

Verification:

- [ ] TODO

### .NET Branding And Inno Installer

Goal: add a polished MultiSnap visual identity to the WPF app and create an Inno Setup installer script.

Planned changes:

- [x] Add reusable WPF theme resources and a real MultiSnap mark in the main/settings UI.
- [x] Add app/icon assets and wire them into the WPF project where practical.
- [x] Add an Inno Setup script that packages the .NET publish output.
- [x] Verify the .NET build and installer script syntax/build if Inno Setup is available.

### Installed Capture Owner Fix

Goal: keep area capture working after install when MultiSnap starts minimized to tray.

Planned changes:

- [x] Avoid assigning an unseen main window as the overlay owner.
- [x] Verify .NET build and installer rebuild.

### Installer Build 2026-05-21

Goal: build the current MultiSnap Windows installer from local project metadata.

Planned changes:

- [x] Confirm app and installer versions are aligned.
- [x] Run the local installer build script.
- [x] Report produced artifact path or blocker.

Verification:

- [x] `tools\build-installer.ps1`

### .NET WPF Tray Settings

Goal: add first-pass configurable settings and make the tray right-click menu explicit in the .NET WPF app.

Planned changes:

- [x] Add persisted settings fields and a WPF settings window.
- [x] Re-register the screenshot hotkey when the saved setting changes.
- [x] Make tray right-click open the menu and keep left-click capture behavior clear.
- [x] Verify the .NET build without disturbing unrelated changes.

Verification:

- [x] dotnet build with an alternate output path if the running app locks the default exe.

### .NET Standard App Consolidation

Goal: consolidate the .NET app around a `netstandard` shared core while keeping Windows-only WPF/tray/capture code in the `net8.0-windows` host.

Planned changes:

- [x] Move settings defaults and validation into `MultiSnap.Core`.
- [x] Update the WPF host to consume the shared settings contract instead of local literals.
- [x] Add a clear project-level note explaining why the executable host cannot target `netstandard`.
- [x] Add a single .NET solution containing both the Windows host and shared core.
- [x] Verify the combined .NET build.

Verification:

- [x] dotnet build `dotnet\MultiSnap.sln` with alternate output path.

### .NET WPF Core Port First Slice

Goal: start the .NET 8 WPF port with the same minimal capture/editor workflow as the Electron MVP.

Planned changes:

- [x] Scaffold a Windows-only .NET 8 WPF app in a separate project folder.
- [x] Add tray menu actions for area capture, full-screen capture, show, and quit.
- [x] Register Ctrl+PrintScreen for area capture.
- [x] Add transparent full-screen region selection overlay.
- [x] Add a full-window editor with copy and save PNG actions.
- [x] Persist local JSON settings.
- [x] Add local Serilog file logging.

Execution order:

- [x] Confirm .NET 8 Windows Desktop SDK is available.
- [x] Create project structure and core services.
- [x] Implement WPF windows and app lifecycle.
- [x] Build the .NET app.

Risks or dependencies:

- [!] Global PrintScreen-family hotkeys may be owned by another app.
- [x] Serilog NuGet restore may require network access.

Verification:

- [x] dotnet build dotnet\MultiSnap\MultiSnap.csproj.

### .NET Standard Shared Layer

Goal: move portable .NET code into a .NET Standard library while keeping the Windows WPF host on a Windows target framework.

Planned changes:

- [x] Add a `MultiSnap.Core` .NET Standard project under `dotnet/`.
- [x] Move portable settings models into the shared project.
- [x] Reference the shared project from the WPF app.
- [x] Verify the .NET build.

Risks or dependencies:

- [!] WPF, WinForms tray, global hotkeys, and screen capture cannot target .NET Standard directly.

Verification:

- [x] dotnet build dotnet\MultiSnap\MultiSnap.csproj -p:OutputPath=bin\CodexVerify\.

### Remove Electron Prototype

Goal: remove the legacy Electron/Vite prototype now that the .NET/WPF app is the active runtime.

Planned changes:

- [x] Remove Electron source, config, package metadata, generated deps/build output, and old dev logs.
- [x] Update root docs/ignore rules so the project points at the .NET app.
- [x] Verify the remaining .NET app builds/checks.

Verification:

- [x] `dotnet build .\dotnet\MultiSnap\MultiSnap.csproj -p:Platform=x64`

### GI Instruction Kit Update 2026.06.16.5

Goal: apply accepted general-instructions migrations from `2026.06.11.6` through `2026.06.16.5` without changing unrelated product code.

Planned changes:

- [x] Add/update project-memory RAG structure, semantic chunk export, and optional Chroma adapter files.
- [x] Update local agent command rules for task-manager sync, `gi install`, shared-instruction init, and verification plan contracts.
- [x] Record applied migration metadata after verification.
- [x] Run scoped verification checks and handle commit/push according to local GI update policy.

Risks or dependencies:

- [!] Generated SQLite, JSONL, and vector indexes must remain ignored.
- [!] Preserve MultiSnap-specific local instructions while applying shared instruction updates.

Verification:

- [x] `git diff --check`
- [x] JSON validation for `tools/project-memory/rag-system.json` and `tools/project-memory/instruction-kit.json`
- [x] `python -m py_compile` for updated project-memory scripts
- [x] Project-memory index rebuild/stats/export checks
