using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace MultiSnap.Services;

public sealed class HotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private const int HotkeyId = 0x4d53;
    private const uint ModControl = 0x0002;
    private const uint VkSnapshot = 0x2C;

    private HwndSource? _source;
    private bool _registered;

    public event EventHandler? AreaCaptureRequested;

    public bool Register(WindowInteropHelper helper)
    {
        var handle = helper.Handle;
        _source = HwndSource.FromHwnd(handle);
        _source?.AddHook(WndProc);
        _registered = RegisterHotKey(handle, HotkeyId, ModControl, VkSnapshot);
        return _registered;
    }

    public void Dispose()
    {
        if (_source is null)
        {
            return;
        }

        if (_registered)
        {
            UnregisterHotKey(_source.Handle, HotkeyId);
        }

        _source.RemoveHook(WndProc);
        _source = null;
        _registered = false;
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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
