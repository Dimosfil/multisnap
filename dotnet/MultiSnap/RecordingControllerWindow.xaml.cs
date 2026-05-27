using System.Windows;
using System.Windows.Threading;

namespace MultiSnap;

public partial class RecordingControllerWindow : Window
{
    private readonly Func<Task> _stopRecording;
    private readonly Func<Task> _cancelRecording;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private readonly DateTime _startedAt = DateTime.Now;

    public RecordingControllerWindow(Func<Task> stopRecording, Func<Task> cancelRecording)
    {
        _stopRecording = stopRecording;
        _cancelRecording = cancelRecording;
        InitializeComponent();
        Loaded += (_, _) => PositionNearTopRight();
        _timer.Tick += (_, _) => UpdateTimer();
        _timer.Start();
        UpdateTimer();
    }

    protected override void OnClosed(EventArgs e)
    {
        _timer.Stop();
        base.OnClosed(e);
    }

    private async void Stop_Click(object sender, RoutedEventArgs e)
    {
        await _stopRecording();
        Close();
    }

    private async void Cancel_Click(object sender, RoutedEventArgs e)
    {
        await _cancelRecording();
        Close();
    }

    private void UpdateTimer()
    {
        var elapsed = DateTime.Now - _startedAt;
        TimerText.Text = elapsed.TotalHours >= 1
            ? elapsed.ToString(@"hh\:mm\:ss")
            : elapsed.ToString(@"mm\:ss");
    }

    private void PositionNearTopRight()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - 24;
        Top = workArea.Top + 24;
    }
}
