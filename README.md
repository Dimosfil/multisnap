# MultiSnap

## Обзор

MultiSnap - Windows-first утилита для быстрых скриншотов, разметки и записи
выбранной области экрана. Приложение ориентировано на Monosnap-style workflow:
быстро вызвать захват горячей клавишей или из tray, выделить область, отметить
важное в легком редакторе и сразу скопировать или сохранить результат.

## Возможности

- Захват выбранной области через overlay.
- Захват всего виртуального рабочего стола.
- Легкая разметка снимка через WPF `InkCanvas`.
- Копирование снимков в буфер обмена и сохранение PNG.
- Запись выбранной области экрана через `ScreenRecorderLib`.
- Tray menu для захвата области, полного экрана, записи видео, настроек и выхода.
- Настройки горячих клавиш, запуска в tray и копирования снимков в clipboard.

## Рабочие сценарии

- `Ctrl+PrintScreen`: выбрать область и открыть снимок в редакторе.
- `Ctrl+Shift+PrintScreen`: выбрать область и начать запись видео.
- Кнопка `Full Screen`: захватить весь виртуальный рабочий стол.
- Tray left-click: запустить захват области.
- В редакторе: нарисовать пометки, затем `Copy`, `Save PNG` или `Clear Ink`.

## Разработка

Основной runtime - .NET 8 Windows desktop app:

```powershell
dotnet run --project .\dotnet\MultiSnap\MultiSnap.csproj
```

Сборка WPF-приложения:

```powershell
dotnet build .\dotnet\MultiSnap\MultiSnap.csproj -p:Platform=x64
```

Публикация:

```powershell
dotnet publish .\dotnet\MultiSnap\MultiSnap.csproj -c Release -r win-x64 --self-contained false
```

Инсталлятор собирается через Inno Setup:

```powershell
.\tools\build-installer.ps1
```

Каноническая инвентаризация стека находится в
`tools/project-memory/specs/technology-stack.md`.

## English

MultiSnap is a Windows-first screenshot, annotation, and selected-area screen
recording utility inspired by fast Monosnap-style workflows. It lets the user
capture an area or the full virtual desktop, mark up the image in a lightweight
WPF editor, copy or save the PNG, and record a selected screen region from the
tray or global hotkeys.

The main runtime is a .NET 8 WPF desktop app with WinForms tray integration,
global hotkeys, `ScreenRecorderLib` video recording, Serilog file logging, and
Inno Setup packaging. The stack inventory is kept at
`tools/project-memory/specs/technology-stack.md`.
