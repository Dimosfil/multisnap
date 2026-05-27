using System;

namespace MultiSnap.Core;

public static class AppSettingsContract
{
    public const string DefaultAreaCaptureHotkey = "Ctrl+PrintScreen";
    public const string DefaultVideoRecordingHotkey = "Ctrl+Shift+PrintScreen";

    public static readonly string[] AreaCaptureHotkeys =
    {
        DefaultAreaCaptureHotkey,
        "PrintScreen",
        "Alt+PrintScreen",
        "Shift+PrintScreen"
    };

    public static readonly string[] VideoRecordingHotkeys =
    {
        DefaultVideoRecordingHotkey,
        "Ctrl+Alt+PrintScreen",
        "Shift+PrintScreen",
        "Alt+PrintScreen"
    };

    public static AppSettings Normalize(AppSettings settings)
    {
        settings.AreaCaptureHotkey = NormalizeAreaCaptureHotkey(settings.AreaCaptureHotkey);
        settings.VideoRecordingHotkey = NormalizeVideoRecordingHotkey(settings.VideoRecordingHotkey);
        return settings;
    }

    public static string NormalizeAreaCaptureHotkey(string? hotkey)
    {
        foreach (var option in AreaCaptureHotkeys)
        {
            if (string.Equals(option, hotkey, StringComparison.Ordinal))
            {
                return option;
            }
        }

        return DefaultAreaCaptureHotkey;
    }

    public static string NormalizeVideoRecordingHotkey(string? hotkey)
    {
        foreach (var option in VideoRecordingHotkeys)
        {
            if (string.Equals(option, hotkey, StringComparison.Ordinal))
            {
                return option;
            }
        }

        return DefaultVideoRecordingHotkey;
    }
}
