namespace MultiSnap.Core;

public sealed class AppSettings
{
    public string AreaCaptureHotkey { get; set; } = AppSettingsContract.DefaultAreaCaptureHotkey;
    public string VideoRecordingHotkey { get; set; } = AppSettingsContract.DefaultVideoRecordingHotkey;
    public bool StartMinimizedToTray { get; set; }
    public bool CopyCapturedImageToClipboard { get; set; } = true;
}
