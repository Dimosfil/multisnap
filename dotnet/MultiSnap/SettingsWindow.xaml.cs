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
            StartMinimizedToTray = settings.StartMinimizedToTray,
            CopyCapturedImageToClipboard = settings.CopyCapturedImageToClipboard
        };

        InitializeComponent();
        AreaHotkeyCombo.ItemsSource = AppSettingsContract.AreaCaptureHotkeys;
        SelectHotkey(Settings.AreaCaptureHotkey);
        StartMinimizedCheck.IsChecked = Settings.StartMinimizedToTray;
        CopyClipboardCheck.IsChecked = Settings.CopyCapturedImageToClipboard;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Settings.AreaCaptureHotkey = GetSelectedHotkey();
        Settings.StartMinimizedToTray = StartMinimizedCheck.IsChecked == true;
        Settings.CopyCapturedImageToClipboard = CopyClipboardCheck.IsChecked == true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void SelectHotkey(string hotkey)
    {
        AreaHotkeyCombo.SelectedItem = AppSettingsContract.NormalizeAreaCaptureHotkey(hotkey);
    }

    private string GetSelectedHotkey()
    {
        if (AreaHotkeyCombo.SelectedItem is string hotkey)
        {
            return AppSettingsContract.NormalizeAreaCaptureHotkey(hotkey);
        }

        return AppSettingsContract.DefaultAreaCaptureHotkey;
    }
}
