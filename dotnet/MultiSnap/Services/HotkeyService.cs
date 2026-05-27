using System.Runtime.InteropServices;
using System.Windows.Interop;
using MultiSnap.Core;

namespace MultiSnap.Services;

public sealed class HotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int AreaHotkeyId = 0x4d53;
    private const int VideoHotkeyId = 0x4d54;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint VkSnapshot = 0x2C;

    private HwndSource? _source;
    private bool _areaRegistered;
    private bool _videoRegistered;

    public event EventHandler? AreaCaptureRequested;
    public event EventHandler? VideoRecordingRequested;

    public HotkeyRegistrationResult Register(WindowInteropHelper helper, AppSettings settings)
    {
        var handle = helper.Handle;
        if (_source is null)
        {
            _source = HwndSource.FromHwnd(handle);
            _source?.AddHook(WndProc);
        }

        UnregisterCurrent();

        var areaHotkey = ParseHotkey(AppSettingsContract.NormalizeAreaCaptureHotkey(settings.AreaCaptureHotkey));
        var videoHotkey = ParseHotkey(AppSettingsContract.NormalizeVideoRecordingHotkey(settings.VideoRecordingHotkey));
        _areaRegistered = RegisterHotKey(handle, AreaHotkeyId, areaHotkey.Modifiers, areaHotkey.Key);
        _videoRegistered = RegisterHotKey(handle, VideoHotkeyId, videoHotkey.Modifiers, videoHotkey.Key);
        return new HotkeyRegistrationResult(_areaRegistered, _videoRegistered);
    }

    public void Dispose()
    {
        if (_source is null)
        {
            return;
        }

        UnregisterCurrent();
        _source.RemoveHook(WndProc);
        _source = null;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WmHotkey)
        {
            return IntPtr.Zero;
        }

        switch (wParam.ToInt32())
        {
            case AreaHotkeyId:
                AreaCaptureRequested?.Invoke(this, EventArgs.Empty);
                handled = true;
                break;
            case VideoHotkeyId:
                VideoRecordingRequested?.Invoke(this, EventArgs.Empty);
                handled = true;
                break;
        }

        return IntPtr.Zero;
    }

    private void UnregisterCurrent()
    {
        if (_source is null)
        {
            return;
        }

        if (_areaRegistered)
        {
            UnregisterHotKey(_source.Handle, AreaHotkeyId);
        }

        if (_videoRegistered)
        {
            UnregisterHotKey(_source.Handle, VideoHotkeyId);
        }

        _areaRegistered = false;
        _videoRegistered = false;
    }

    private static (uint Modifiers, uint Key) ParseHotkey(string hotkey)
    {
        return hotkey switch
        {
            "PrintScreen" => (0, VkSnapshot),
            "Alt+PrintScreen" => (ModAlt, VkSnapshot),
            "Shift+PrintScreen" => (ModShift, VkSnapshot),
            "Ctrl+Shift+PrintScreen" => (ModControl | ModShift, VkSnapshot),
            "Ctrl+Alt+PrintScreen" => (ModControl | ModAlt, VkSnapshot),
            _ => (ModControl, VkSnapshot)
        };
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

public sealed record HotkeyRegistrationResult(bool AreaCaptureRegistered, bool VideoRecordingRegistered);
