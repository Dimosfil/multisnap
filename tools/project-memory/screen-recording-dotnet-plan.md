# Screen Recording .NET Plan

Date: 2026-05-27

Goal: implement screen recording in the Windows .NET/WPF MultiSnap app. Electron
recording APIs are out of scope.

## Architecture Direction

- Primary capture path: Windows.Graphics.Capture through .NET/WinRT interop.
- Encoding path: Media Foundation compatible MP4/H.264 if practical; use FFmpeg
  only if the native encoder path blocks MVP delivery.
- MVP modes: region recording and full-screen recording.
- Deferred: microphone/system audio, upload, GIF export, advanced annotation over
  video, and Electron parity.

## MVP State Machine

- `Idle`: no active recording workflow.
- `Selecting`: region selection overlay is active.
- `Countdown`: recording is requested but capture has not started.
- `Recording`: frames are being captured and encoded.
- `Stopping`: stop requested and encoder is finalizing the file.
- `Cancelled`: user cancelled before a durable result should be kept.
- `Failed`: recoverable error state that returns to `Idle` after reporting.

## Default Shortcuts

- Area screenshot: `Ctrl+PrintScreen`.
- Video recording mode: `Ctrl+Shift+PrintScreen`.

## Risks To Handle Early

- Mixed-DPI region coordinates across monitors.
- The floating recording controller appearing in the captured area.
- Encoder finalization on app quit or immediate stop after start.
- Hotkey collisions when users choose the same shortcut for screenshots and
  video.
