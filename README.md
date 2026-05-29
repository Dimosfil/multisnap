# MultiSnap

MultiSnap is a Windows-first screenshot utility inspired by the fast capture and annotation workflow of tools like Monosnap.

## MVP goals

- Capture full screen or selected area.
- Open captures in a lightweight editor.
- Add simple annotations.
- Copy to clipboard or save as PNG.
- Keep a local screenshot history.

## Development

```powershell
dotnet run --project .\dotnet\MultiSnap\MultiSnap.csproj
```

Build the WPF app:

```powershell
dotnet build .\dotnet\MultiSnap\MultiSnap.csproj -p:Platform=x64
```

Default shortcuts while the app is running:

- `PrintScreen` - capture a selected area.
- `Ctrl+Shift+PrintScreen` - start video recording.
