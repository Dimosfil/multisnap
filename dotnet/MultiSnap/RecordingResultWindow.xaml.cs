using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using MultiSnap.Services;
using Serilog;
using WpfClipboard = System.Windows.Clipboard;
using WpfMessageBox = System.Windows.MessageBox;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace MultiSnap;

public partial class RecordingResultWindow : Window
{
    private string? _filePath;
    private readonly DispatcherTimer _playbackTimer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private bool _isSeeking;
    private bool _isPlaying;

    public RecordingResultWindow(RecordingResult? result)
    {
        InitializeComponent();
        _playbackTimer.Tick += (_, _) => UpdatePlaybackProgress();
        ApplyResult(result);
    }

    protected override void OnClosed(EventArgs e)
    {
        _playbackTimer.Stop();
        PreviewPlayer.Stop();
        PreviewPlayer.Source = null;
        base.OnClosed(e);
    }

    private void ApplyResult(RecordingResult? result)
    {
        _filePath = result?.FilePath;
        var hasFile = !string.IsNullOrWhiteSpace(_filePath) && File.Exists(_filePath);

        SaveAsButton.IsEnabled = hasFile;
        CopyPathButton.IsEnabled = hasFile;
        OpenFolderButton.IsEnabled = hasFile;
        DeleteButton.IsEnabled = hasFile;

        if (!hasFile)
        {
            ResultStatusText.Text = string.IsNullOrWhiteSpace(result?.ErrorMessage)
                ? "Recording stopped. No video file was produced."
                : $"Recording failed: {result.ErrorMessage}";
            FilePathText.Text = string.IsNullOrWhiteSpace(result?.ErrorMessage)
                ? "No playable recording file is available."
                : "Recorder did not produce a playable file.";
            EmptyPreview.Visibility = Visibility.Visible;
            EmptyPreviewDetails.Text = string.IsNullOrWhiteSpace(result?.ErrorMessage)
                ? "Try recording for at least a couple of seconds. If this repeats, check the local log."
                : result.ErrorMessage;
            PreviewPlayer.Visibility = Visibility.Collapsed;
            PlayerControls.Visibility = Visibility.Collapsed;
            _playbackTimer.Stop();
            return;
        }

        ResultStatusText.Text = "Recording saved.";
        FilePathText.Text = _filePath;
        EmptyPreview.Visibility = Visibility.Collapsed;
        PreviewPlayer.Visibility = Visibility.Visible;
        PlayerControls.Visibility = Visibility.Visible;
        PreviewPlayer.Source = new Uri(_filePath!);
        PreviewPlayer.Play();
        _isPlaying = true;
        PlayPauseButton.Content = "Pause";
        _playbackTimer.Start();
    }

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetExistingFile(out var sourcePath))
        {
            return;
        }

        var dialog = new WpfSaveFileDialog
        {
            Filter = "Video file (*.*)|*.*",
            FileName = Path.GetFileName(sourcePath)
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        File.Copy(sourcePath, dialog.FileName, overwrite: true);
        _filePath = dialog.FileName;
        ApplyResult(new RecordingResult(null, null, DateTime.Now, _filePath));
    }

    private void CopyPath_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetExistingFile(out var sourcePath))
        {
            WpfClipboard.SetText(sourcePath);
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetExistingFile(out var sourcePath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{sourcePath}\"",
            UseShellExecute = true
        });
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetExistingFile(out var sourcePath))
        {
            return;
        }

        PreviewPlayer.Stop();
        PreviewPlayer.Source = null;
        File.Delete(sourcePath);
        Log.Information("Deleted recording result {Path}", sourcePath);
        _filePath = null;
        ApplyResult(null);
    }

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetExistingFile(out _))
        {
            return;
        }

        if (_isPlaying)
        {
            PreviewPlayer.Pause();
            _isPlaying = false;
            PlayPauseButton.Content = "Play";
        }
        else
        {
            PreviewPlayer.Play();
            _isPlaying = true;
            PlayPauseButton.Content = "Pause";
            _playbackTimer.Start();
        }
    }

    private void StopPlayback_Click(object sender, RoutedEventArgs e)
    {
        PreviewPlayer.Stop();
        PreviewPlayer.Position = TimeSpan.Zero;
        _isPlaying = false;
        PlayPauseButton.Content = "Play";
        UpdatePlaybackProgress();
    }

    private void PreviewPlayer_MediaOpened(object sender, RoutedEventArgs e)
    {
        UpdatePlaybackProgress();
    }

    private void PreviewPlayer_MediaEnded(object sender, RoutedEventArgs e)
    {
        _isPlaying = false;
        PlayPauseButton.Content = "Play";
        _playbackTimer.Stop();
        UpdatePlaybackProgress();
    }

    private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isSeeking || !TimelineSlider.IsMouseCaptureWithin)
        {
            return;
        }

        PreviewPlayer.Position = TimeSpan.FromSeconds(TimelineSlider.Value);
        UpdatePlaybackProgress();
    }

    private void UpdatePlaybackProgress()
    {
        if (!PreviewPlayer.NaturalDuration.HasTimeSpan)
        {
            TimeText.Text = "00:00 / 00:00";
            TimelineSlider.Value = 0;
            return;
        }

        var duration = PreviewPlayer.NaturalDuration.TimeSpan;
        var position = PreviewPlayer.Position;
        _isSeeking = true;
        TimelineSlider.Maximum = Math.Max(1, duration.TotalSeconds);
        TimelineSlider.Value = Math.Min(TimelineSlider.Maximum, Math.Max(0, position.TotalSeconds));
        _isSeeking = false;
        TimeText.Text = $"{FormatTime(position)} / {FormatTime(duration)}";
    }

    private static string FormatTime(TimeSpan value)
    {
        return value.TotalHours >= 1
            ? value.ToString(@"hh\:mm\:ss")
            : value.ToString(@"mm\:ss");
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private bool TryGetExistingFile(out string sourcePath)
    {
        sourcePath = _filePath ?? string.Empty;
        if (!File.Exists(sourcePath))
        {
            WpfMessageBox.Show("The recording file is not available yet.", "MultiSnap");
            return false;
        }

        return true;
    }
}
