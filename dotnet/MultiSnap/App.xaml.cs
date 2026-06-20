using System.IO;
using System.Windows;
using System.Windows.Forms;
using DrawingIcon = System.Drawing.Icon;
using System.Windows.Media.Imaging;
using MultiSnap.Core;
using MultiSnap.Services;
using Serilog;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace MultiSnap;

public partial class App : Application
{
    private readonly SettingsService _settings = new();
    private readonly ScreenCaptureService _capture = new();
    private readonly HotkeyService _hotkeys = new();
    private readonly RecordingService _recording = new();
    private AppSettings _currentSettings = new();
    private NotifyIcon? _tray;
    private MainWindow? _mainWindow;
    private RecordingControllerWindow? _recordingController;
    private RecordingResultWindow? _recordingResultWindow;
    private bool _isQuitting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        ConfigureLogging();

        _currentSettings = _settings.Load();
        _mainWindow = new MainWindow(_capture, StartAreaCapture, CaptureFullScreen, _currentSettings);
        MainWindow = _mainWindow;
        _mainWindow.SourceInitialized += (_, _) =>
        {
            var registered = RegisterHotkey();
            if (!registered.AreaCaptureRegistered)
            {
                Log.Warning("{Hotkey} hotkey registration failed.", _currentSettings.AreaCaptureHotkey);
            }

            if (!registered.VideoRecordingRegistered)
            {
                Log.Warning("{Hotkey} hotkey registration failed.", _currentSettings.VideoRecordingHotkey);
            }
        };
        _mainWindow.Closing += (_, args) =>
        {
            if (_isQuitting)
            {
                return;
            }

            args.Cancel = true;
            _mainWindow.Hide();
        };

        _hotkeys.AreaCaptureRequested += (_, _) => Dispatcher.Invoke(StartAreaCapture);
        _hotkeys.VideoRecordingRequested += (_, _) => Dispatcher.Invoke(StartVideoRecording);
        _recording.StatusChanged += (_, args) => Log.Information("Recording status: {Status}", args.Message);
        CreateTray();
        if (!_currentSettings.StartMinimizedToTray)
        {
            _mainWindow.Show();
        }

        Log.Information("MultiSnap .NET WPF started.");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _isQuitting = true;
        _hotkeys.Dispose();
        _tray?.Dispose();
        Log.Information("MultiSnap .NET WPF stopped.");
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static void ConfigureLogging()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var logDir = Path.Combine(appData, "MultiSnap", "logs");
        Directory.CreateDirectory(logDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(logDir, "multisnap-.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private void CreateTray()
    {
        _tray = new NotifyIcon
        {
            Text = "MultiSnap",
            Icon = GetTrayIcon(),
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip()
        };

        _tray.ContextMenuStrip.Items.Add("Capture area", null, (_, _) => StartAreaCapture());
        _tray.ContextMenuStrip.Items.Add("Capture full screen", null, (_, _) => CaptureFullScreen());
        _tray.ContextMenuStrip.Items.Add("Record video area", null, (_, _) => StartVideoRecording());
        _tray.ContextMenuStrip.Items.Add("Settings...", null, (_, _) => ShowSettingsWindow());
        _tray.ContextMenuStrip.Items.Add("Show MultiSnap", null, (_, _) => ShowMainWindow());
        _tray.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _tray.ContextMenuStrip.Items.Add("Quit", null, (_, _) => Quit());
        _tray.MouseClick += (_, args) =>
        {
            if (args.Button == MouseButtons.Left)
            {
                StartAreaCapture();
            }
        };
        _tray.DoubleClick += (_, _) => ShowMainWindow();
    }

    private static DrawingIcon GetTrayIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "MultiSnap.ico");
        return File.Exists(iconPath) ? new DrawingIcon(iconPath) : System.Drawing.SystemIcons.Application;
    }

    private void ShowMainWindow()
    {
        _mainWindow?.Show();
        _mainWindow?.Activate();
    }

    private void ShowSettingsWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        ShowMainWindow();
        var window = new SettingsWindow(_currentSettings) { Owner = _mainWindow };
        if (window.ShowDialog() != true)
        {
            return;
        }

        _currentSettings = _settings.Save(window.Settings);
        _mainWindow.ApplySettings(_currentSettings);
        RegisterHotkey();
    }

    private async void StartVideoRecording()
    {
        try
        {
            var screenshot = _capture.CaptureCursorDisplay();
            var overlay = new OverlayWindow(screenshot, _capture, OverlayMode.ScreenRecording);
            if (_mainWindow?.IsVisible == true)
            {
                overlay.Owner = _mainWindow;
            }

            if (overlay.ShowDialog() != true || overlay.SelectedRecordingRegion is not { } region)
            {
                return;
            }

            await _recording.StartRegionRecordingAsync(region);
            ShowRecordingController();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Video recording start failed.");
            MessageBox.Show("Video recording failed. See the local log for details.", "MultiSnap");
        }
    }

    private void ShowRecordingController()
    {
        _recordingController?.Close();
        _recordingController = new RecordingControllerWindow(
            StopRecordingAndShowResultAsync,
            async () => await _recording.CancelAsync());
        _recordingController.Closed += (_, _) => _recordingController = null;
        _recordingController.Show();
    }

    private async Task StopRecordingAndShowResultAsync()
    {
        var result = await _recording.StopAsync();
        ShowRecordingResult(result);
    }

    private void ShowRecordingResult(RecordingResult? result)
    {
        _recordingResultWindow?.Close();
        _recordingResultWindow = new RecordingResultWindow(result);
        if (_mainWindow is not null)
        {
            _recordingResultWindow.Owner = _mainWindow;
        }

        _recordingResultWindow.Closed += (_, _) => _recordingResultWindow = null;
        _recordingResultWindow.Show();
        _recordingResultWindow.Activate();
    }

    private void StartAreaCapture()
    {
        try
        {
            var screenshot = _capture.CaptureVirtualDesktop();
            var overlay = new OverlayWindow(screenshot, _capture, overlayBounds: _capture.GetVirtualDesktopBounds());
            if (_mainWindow?.IsVisible == true)
            {
                overlay.Owner = _mainWindow;
            }

            if (overlay.ShowDialog() == true && overlay.CapturedImage is not null)
            {
                OpenEditor(overlay.CapturedImage);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Area capture failed.");
            MessageBox.Show("Area capture failed. See the local log for details.", "MultiSnap");
        }
    }

    private void CaptureFullScreen()
    {
        try
        {
            OpenEditor(_capture.CaptureVirtualDesktop());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Full-screen capture failed.");
            MessageBox.Show("Full-screen capture failed. See the local log for details.", "MultiSnap");
        }
    }

    private void OpenEditor(BitmapSource image)
    {
        if (_currentSettings.CopyCapturedImageToClipboard)
        {
            Clipboard.SetImage(image);
        }

        ShowMainWindow();
        _mainWindow?.LoadImage(image);
    }

    private HotkeyRegistrationResult RegisterHotkey()
    {
        if (_mainWindow is null)
        {
            return new HotkeyRegistrationResult(false, false);
        }

        var helper = new System.Windows.Interop.WindowInteropHelper(_mainWindow);
        var registered = _hotkeys.Register(helper, _currentSettings);
        _mainWindow.SetHotkeyStatus(
            registered,
            _currentSettings.AreaCaptureHotkey,
            _currentSettings.VideoRecordingHotkey);
        return registered;
    }

    private void Quit()
    {
        _isQuitting = true;
        Shutdown();
    }
}
