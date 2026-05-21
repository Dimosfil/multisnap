namespace MultiSnap.Core;

public sealed class AppSettings
{
    public string AreaCaptureHotkey { get; set; } = AppSettingsContract.DefaultAreaCaptureHotkey;
    public bool StartMinimizedToTray { get; set; }
    public bool CopyCapturedImageToClipboard { get; set; } = true;
}
