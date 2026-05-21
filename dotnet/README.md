# MultiSnap .NET App

This folder contains one .NET application split into two layers:

- `MultiSnap.Core` targets `netstandard2.0` and owns portable contracts such as settings models, defaults, and validation.
- `MultiSnap` targets `net8.0-windows` and owns the Windows runtime surface: WPF windows, tray integration, global hotkeys, screen capture, clipboard, and logging.

The executable host cannot target .NET Standard because WPF, WinForms tray APIs, global hotkeys, and desktop screenshot capture are Windows-specific runtime features. Keep shared business rules in `MultiSnap.Core`; keep Windows UI and OS integration in `MultiSnap`.
