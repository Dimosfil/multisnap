using System.Runtime.InteropServices;
using System.Windows.Interop;
using MultiSnap.Core;

namespace MultiSnap.Services;

public sealed class HotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int HotkeyId = 0x4d53;
    private const uint ModAlt = 0x0001;
    private const uint ModControl = 0x0002;
    private const uint ModShift = 0x0004;
    private const uint VkSnapshot = 0x2C;

    private HwndSource? _source;
    private bool _registered;

    public event EventHandler? AreaCaptureRequested;

    public bool Register(WindowInteropHelper helper, AppSettings settings)
    {
        var handle = helper.Handle;
        if (_source is null)
        {
            _source = HwndSource.FromHwnd(handle);
            _source?.AddHook(WndProc);
        }

        UnregisterCurrent();

        var hotkey = ParseHotkey(settings.AreaCaptureHotkey);
        _registered = RegisterHotKey(handle, HotkeyId, hotkey.Modifiers, hotkey.Key);
        return _registered;
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
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            AreaCaptureRequested?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private void UnregisterCurrent()
    {
        if (_source is not null && _registered)
        {
            UnregisterHotKey(_source.Handle, HotkeyId);
        }

        _registered = false;
    }

    private static (uint Modifiers, uint Key) ParseHotkey(string hotkey)
    {
        return AppSettingsContract.NormalizeAreaCaptureHotkey(hotkey) switch
        {
            "PrintScreen" => (0, VkSnapshot),
            "Alt+PrintScreen" => (ModAlt, VkSnapshot),
            "Shift+PrintScreen" => (ModShift, VkSnapshot),
            _ => (ModControl, VkSnapshot)
        };
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
