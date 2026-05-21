using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
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
    private NotifyIcon? _tray;
    private MainWindow? _mainWindow;
    private bool _isQuitting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        ConfigureLogging();

        _settings.Load();
        _mainWindow = new MainWindow(_capture, StartAreaCapture, CaptureFullScreen);
        MainWindow = _mainWindow;
        _mainWindow.SourceInitialized += (_, _) =>
        {
            var registered = _hotkeys.Register(new System.Windows.Interop.WindowInteropHelper(_mainWindow));
            _mainWindow.SetHotkeyStatus(registered);
            if (!registered)
            {
                Log.Warning("Ctrl+PrintScreen hotkey registration failed.");
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
        CreateTray();
        _mainWindow.Show();
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
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip()
        };

        _tray.ContextMenuStrip.Items.Add("Capture area", null, (_, _) => StartAreaCapture());
        _tray.ContextMenuStrip.Items.Add("Capture full screen", null, (_, _) => CaptureFullScreen());
        _tray.ContextMenuStrip.Items.Add("Show MultiSnap", null, (_, _) => ShowMainWindow());
        _tray.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _tray.ContextMenuStrip.Items.Add("Quit", null, (_, _) => Quit());
        _tray.Click += (_, _) => StartAreaCapture();
        _tray.DoubleClick += (_, _) => ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        _mainWindow?.Show();
        _mainWindow?.Activate();
    }

    private void StartAreaCapture()
    {
        try
        {
            var screenshot = _capture.CaptureCursorDisplay();
            var overlay = new OverlayWindow(screenshot, _capture) { Owner = _mainWindow };
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
            OpenEditor(_capture.CaptureCursorDisplay());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Full-screen capture failed.");
            MessageBox.Show("Full-screen capture failed. See the local log for details.", "MultiSnap");
        }
    }

    private void OpenEditor(BitmapSource image)
    {
        Clipboard.SetImage(image);
        ShowMainWindow();
        _mainWindow?.LoadImage(image);
    }

    private void Quit()
    {
        _isQuitting = true;
        Shutdown();
    }
}
