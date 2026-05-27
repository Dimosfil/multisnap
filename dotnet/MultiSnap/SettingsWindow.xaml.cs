using System.Windows;
using MultiSnap.Core;

namespace MultiSnap;

public partial class SettingsWindow : Window
{
    public AppSettings Settings { get; }

    public SettingsWindow(AppSettings settings)
    {
        Settings = new AppSettings
        {
            AreaCaptureHotkey = settings.AreaCaptureHotkey,
            VideoRecordingHotkey = settings.VideoRecordingHotkey,
            StartMinimizedToTray = settings.StartMinimizedToTray,
            CopyCapturedImageToClipboard = settings.CopyCapturedImageToClipboard
        };

        InitializeComponent();
        AreaHotkeyCombo.ItemsSource = AppSettingsContract.AreaCaptureHotkeys;
        VideoHotkeyCombo.ItemsSource = AppSettingsContract.VideoRecordingHotkeys;
        SelectAreaHotkey(Settings.AreaCaptureHotkey);
        SelectVideoHotkey(Settings.VideoRecordingHotkey);
        StartMinimizedCheck.IsChecked = Settings.StartMinimizedToTray;
        CopyClipboardCheck.IsChecked = Settings.CopyCapturedImageToClipboard;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Settings.AreaCaptureHotkey = GetSelectedAreaHotkey();
        Settings.VideoRecordingHotkey = GetSelectedVideoHotkey();
        Settings.StartMinimizedToTray = StartMinimizedCheck.IsChecked == true;
        Settings.CopyCapturedImageToClipboard = CopyClipboardCheck.IsChecked == true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void SelectAreaHotkey(string hotkey)
    {
        AreaHotkeyCombo.SelectedItem = AppSettingsContract.NormalizeAreaCaptureHotkey(hotkey);
    }

    private void SelectVideoHotkey(string hotkey)
    {
        VideoHotkeyCombo.SelectedItem = AppSettingsContract.NormalizeVideoRecordingHotkey(hotkey);
    }

    private string GetSelectedAreaHotkey()
    {
        if (AreaHotkeyCombo.SelectedItem is string hotkey)
        {
            return AppSettingsContract.NormalizeAreaCaptureHotkey(hotkey);
        }

        return AppSettingsContract.DefaultAreaCaptureHotkey;
    }

    private string GetSelectedVideoHotkey()
    {
        if (VideoHotkeyCombo.SelectedItem is string hotkey)
        {
            return AppSettingsContract.NormalizeVideoRecordingHotkey(hotkey);
        }

        return AppSettingsContract.DefaultVideoRecordingHotkey;
    }
}
