using System;

namespace MultiSnap.Core;

public static class AppSettingsContract
{
    public const string DefaultAreaCaptureHotkey = "Ctrl+PrintScreen";

    public static readonly string[] AreaCaptureHotkeys =
    {
        DefaultAreaCaptureHotkey,
        "PrintScreen",
        "Alt+PrintScreen",
        "Shift+PrintScreen"
    };

    public static AppSettings Normalize(AppSettings settings)
    {
        settings.AreaCaptureHotkey = NormalizeAreaCaptureHotkey(settings.AreaCaptureHotkey);
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
}
