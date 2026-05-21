# Codex Work Summary - 2026-05-21

## Request

Add screenshot hotkey settings and make the initial workflow:

- `Print Screen` opens frozen area selection for the display under the cursor.
- `Ctrl+Print Screen` captures the full display under the cursor.
- Area capture should show a crosshair selection overlay, copy the selected image to the clipboard, and open the captured image in the app with Save available.

## Current Changes

Modified files:

- `src/main/main.ts`
- `src/main/preload.ts`
- `src/renderer/App.tsx`
- `src/renderer/electron.d.ts`
- `src/renderer/styles.css`
- `tools/project-memory/pending-tasks.md`

Implemented:

- Persisted app screenshot hotkey settings in Electron `userData/settings.json`.
- Added shortcut choices in the renderer settings panel.
- Default shortcuts:
  - area capture: `PrintScreen`
  - full screen capture: `Control+PrintScreen`
- Added runtime registration status for each global shortcut (`Active` / `Unavailable`).
- Changed display capture to use the screen nearest the cursor instead of always the primary display.
- Changed area capture ordering so the screen frame is captured before the overlay opens.
- Area selection now uses the frozen frame, crosshair cursor, drag rectangle, and copies the resulting crop to the clipboard before opening it in the editor.
- Full-screen capture also copies the captured image to the clipboard before opening it in the editor.
- Restarted the dev app after main-process changes.

## Verification

Commands run successfully:

- `npm run check`
- `npm run build`

Notes:

- `npm run build` completed successfully but npm printed local environment warnings about access to a user npm CLI path and old user config keys (`python`, `msvs_version`).
- Global hotkeys require Electron main-process restart after code changes. The dev app was restarted after the latest changes.

## Manual Checks Still Useful

- Press `Print Screen` and verify the overlay shows the frozen display under the cursor, not a freshly darkened frame.
- Drag with LMB and release; verify the crop opens in MultiSnap and is already in the clipboard.
- Press `Ctrl+Print Screen`; verify the full display under the cursor opens in MultiSnap and is copied to clipboard.
- If a shortcut shows `Unavailable`, Windows or another app has reserved that global shortcut; choose fallback `Ctrl+Shift+5` / `Ctrl+Shift+6` in Settings.

## Worktree Status At Summary Time

Dirty files:

- `src/main/main.ts`
- `src/main/preload.ts`
- `src/renderer/App.tsx`
- `src/renderer/electron.d.ts`
- `src/renderer/styles.css`
- `tools/project-memory/pending-tasks.md`

No commit was created.
