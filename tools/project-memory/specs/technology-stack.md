# Technology Stack

Last reviewed: 2026-06-24

## Summary

Русский: MultiSnap - Windows-first desktop-утилита на .NET 8. Основное
приложение - WPF executable с WinForms tray integration, глобальными Windows
hotkeys, сервисами screen capture, записью выбранной области экрана и упаковкой
через Inno Setup.

English: MultiSnap is a Windows-first desktop utility built on .NET 8. The
primary application is a WPF executable with WinForms tray integration, global
Windows hotkeys, screen capture services, selected-area video recording, and
Inno Setup packaging.

## Components

| Layer | Technology | Evidence | Notes |
| --- | --- | --- | --- |
| Desktop app | .NET 8, WPF, Windows target framework | `dotnet/MultiSnap/MultiSnap.csproj` | `net8.0-windows`, `OutputType=WinExe`, `UseWPF=true`. |
| Windows privilege model | Application manifest with `requireAdministrator` | `dotnet/MultiSnap/app.manifest`, `dotnet/MultiSnap/MultiSnap.csproj`, `installer/MultiSnap.iss` | MultiSnap runs elevated so global hotkeys and the selection overlay keep working over elevated/system windows; the installer postinstall launch uses ShellExecute so UAC is requested instead of failing with CreateProcess code 740. |
| Tray integration | Windows Forms `NotifyIcon` | `dotnet/MultiSnap/MultiSnap.csproj`, `dotnet/MultiSnap/App.xaml.cs` | `UseWindowsForms=true`; tray menu controls capture, recording, settings, show, and quit. |
| Hotkeys | Win32 global hotkey registration | `dotnet/MultiSnap/Services/HotkeyService.cs`, `dotnet/MultiSnap.Core/AppSettingsContract.cs` | Defaults are `Ctrl+PrintScreen` for area capture and `Ctrl+Shift+PrintScreen` for video recording. |
| Screen capture | WPF/Windows bitmap capture service | `dotnet/MultiSnap/Services/ScreenCaptureService.cs`, `dotnet/MultiSnap/OverlayWindow.xaml.cs` | Supports virtual desktop capture, cursor-display capture, region crop, and PNG save. |
| Annotation UI | WPF `InkCanvas` | `dotnet/MultiSnap/MainWindow.xaml`, `dotnet/MultiSnap/MainWindow.xaml.cs` | Supports ink markup, brush size, copy, save PNG, and clear ink. |
| Screen recording | `ScreenRecorderLib` 6.6.0 | `dotnet/MultiSnap/MultiSnap.csproj`, `dotnet/MultiSnap/Services/RecordingService.cs` | Records selected regions and shows controller/result windows. |
| Logging | Serilog 4.3.1 and `Serilog.Sinks.File` 7.0.0 | `dotnet/MultiSnap/MultiSnap.csproj`, `dotnet/MultiSnap/App.xaml.cs` | Writes rolling local logs named `multisnap-.log`. |
| Shared portable model layer | .NET Standard 2.0 class library | `dotnet/MultiSnap.Core/MultiSnap.Core.csproj` | Holds settings models and normalization contract. |
| Solution structure | Visual Studio solution | `dotnet/MultiSnap.sln` | Contains `MultiSnap` and `MultiSnap.Core`. |
| Installer | Inno Setup | `installer/MultiSnap.iss`, `tools/build-installer.ps1` | Produces `artifacts/installer/MultiSnap-Setup-<version>.exe`; current app/installer metadata is `0.1.23`. |
| GI/project memory | General Instructions kit | `AGENTS.md`, `COMMANDS.md`, `tools/project-memory/instruction-kit.json` | Local agent instruction and retrieval layer. |

## Commands

| Purpose | Command | Evidence |
| --- | --- | --- |
| Restore dependencies | `dotnet restore .\dotnet\MultiSnap.sln` | `AGENTS.md`, `dotnet/MultiSnap.sln` |
| Run app | `dotnet run --project .\dotnet\MultiSnap\MultiSnap.csproj` | `README.md`, `AGENTS.md`, `dotnet/MultiSnap/MultiSnap.csproj` |
| Build/check app | `dotnet build .\dotnet\MultiSnap\MultiSnap.csproj -p:Platform=x64` | `README.md`, `AGENTS.md` |
| Publish app | `dotnet publish .\dotnet\MultiSnap\MultiSnap.csproj -c Release -r win-x64 --self-contained false` | `AGENTS.md`, `dotnet/MultiSnap/MultiSnap.csproj` |
| Build installer | `.\tools\build-installer.ps1` | `tools/build-installer.ps1`, `installer/MultiSnap.iss` |
| Inspect logs | `Get-Content .\*.log -Tail 120` | `AGENTS.md` |

## Runtime State

| State | Location | Evidence | Notes |
| --- | --- | --- | --- |
| User settings | `%APPDATA%\MultiSnap\settings.json` | `dotnet/MultiSnap/Services/SettingsService.cs` | Stores hotkey, tray-start, and clipboard preferences. |
| Temporary recordings | `%TEMP%\MultiSnap\Recordings` | `dotnet/MultiSnap/Services/RecordingService.cs` | Recording output path is temporary until saved through the result window. |
| Local logs | Local rolling `multisnap-.log` files | `dotnet/MultiSnap/App.xaml.cs`, `AGENTS.md` | Inspect with the documented log tail command. |

## External Services

| Service | Role | Evidence | Boundary |
| --- | --- | --- | --- |
| None required for normal app runtime | Desktop capture, annotation, and recording are local | `dotnet/MultiSnap/MultiSnap.csproj`, `dotnet/MultiSnap/Services/*` | Do not add network/service dependencies without project-local config or explicit product need. |

## Open Gaps

- No dedicated automated test project is present under `dotnet/`; current documented verification is a focused .NET build.
- First-run/default reset paths are not documented beyond settings and temporary recording locations.
