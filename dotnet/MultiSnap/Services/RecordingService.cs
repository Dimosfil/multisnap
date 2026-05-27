using System.IO;
using Serilog;
using ScreenRecorderLib;
using Forms = System.Windows.Forms;

namespace MultiSnap.Services;

public sealed class RecordingService
{
    private Recorder? _recorder;
    private string? _activeFilePath;
    private TaskCompletionSource<RecordingResult>? _completion;

    public RecordingState State { get; private set; } = RecordingState.Idle;
    public RecordingMode? ActiveMode { get; private set; }
    public ScreenRecordingRegion? ActiveRegion { get; private set; }

    public event EventHandler<RecordingStatusChangedEventArgs>? StatusChanged;

    public Task StartRegionRecordingAsync(ScreenRecordingRegion region)
    {
        return StartAsync(RecordingMode.Region, region);
    }

    public Task StartFullScreenRecordingAsync()
    {
        return StartAsync(RecordingMode.FullScreen, null);
    }

    public async Task<RecordingResult?> StopAsync()
    {
        if (State != RecordingState.Recording)
        {
            return null;
        }

        SetState(RecordingState.Stopping, "Stopping recording.");
        _recorder?.Stop();

        try
        {
            var completed = _completion is null
                ? null
                : await _completion.Task.WaitAsync(TimeSpan.FromSeconds(12));

            SetState(RecordingState.Idle, completed?.FilePath is null
                ? "Recording stopped, but no output file was produced."
                : "Recording stopped.");
            return completed;
        }
        catch (Exception ex)
        {
            SetState(RecordingState.Failed, "Recording failed while finalizing.");
            Log.Error(ex, "Recording finalization failed.");
            return new RecordingResult(ActiveMode, ActiveRegion, DateTime.Now, null, ex.Message);
        }
        finally
        {
            ResetActiveRecording();
        }
    }

    public Task CancelAsync()
    {
        if (State == RecordingState.Idle)
        {
            return Task.CompletedTask;
        }

        SetState(RecordingState.Cancelled, "Recording cancelled.");
        _recorder?.Stop();
        if (!string.IsNullOrWhiteSpace(_activeFilePath) && File.Exists(_activeFilePath))
        {
            File.Delete(_activeFilePath);
        }

        ResetActiveRecording();
        SetState(RecordingState.Idle, "Ready.");
        return Task.CompletedTask;
    }

    private Task StartAsync(RecordingMode mode, ScreenRecordingRegion? region)
    {
        if (State != RecordingState.Idle)
        {
            SetState(RecordingState.Failed, "Recording is already active.");
            ActiveMode = null;
            ActiveRegion = null;
            SetState(RecordingState.Idle, "Ready.");
            return Task.CompletedTask;
        }

        _activeFilePath = CreateOutputFilePath();
        _completion = new TaskCompletionSource<RecordingResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        ActiveMode = mode;
        ActiveRegion = region;

        try
        {
            var options = CreateRecorderOptions(region);
            _recorder = Recorder.CreateRecorder(options);
            _recorder.OnRecordingComplete += Recorder_OnRecordingComplete;
            _recorder.OnRecordingFailed += Recorder_OnRecordingFailed;
            _recorder.OnStatusChanged += Recorder_OnStatusChanged;

            var scope = region is null
                ? mode.ToString()
                : $"{mode} {region.Left},{region.Top} {region.Width}x{region.Height}";
            SetState(RecordingState.Countdown, $"{scope} recording requested.");
            _recorder.Record(_activeFilePath);
            SetState(RecordingState.Recording, $"Recording to {_activeFilePath}.");
        }
        catch
        {
            ResetActiveRecording();
            SetState(RecordingState.Failed, "Recording backend failed to start.");
            SetState(RecordingState.Idle, "Ready.");
            throw;
        }

        return Task.CompletedTask;
    }

    private static RecorderOptions CreateRecorderOptions(ScreenRecordingRegion? region)
    {
        var options = new RecorderOptions
        {
            OutputOptions = new OutputOptions
            {
                RecorderMode = RecorderMode.Video,
                Stretch = StretchMode.Uniform
            },
            AudioOptions = new AudioOptions
            {
                IsAudioEnabled = false
            },
            VideoEncoderOptions = new VideoEncoderOptions
            {
                Framerate = 30,
                IsFixedFramerate = true,
                Bitrate = 8_000 * 1_000,
                Encoder = new H264VideoEncoder
                {
                    BitrateMode = H264BitrateControlMode.CBR,
                    EncoderProfile = H264Profile.Main
                },
                IsFragmentedMp4Enabled = true
            },
            LogOptions = new LogOptions
            {
                IsLogEnabled = true,
                LogSeverityLevel = ScreenRecorderLib.LogLevel.Warn
            }
        };

        if (region is null)
        {
            return options;
        }

        var width = Math.Max(16, region.Width - (region.Width % 2));
        var height = Math.Max(16, region.Height - (region.Height % 2));
        var displayBounds = ResolveDisplayBounds(region);
        var displays = Recorder.GetDisplays();
        var source = !string.IsNullOrWhiteSpace(region.DisplayDeviceName)
            ? displays.FirstOrDefault(display => string.Equals(display.DeviceName, region.DisplayDeviceName, StringComparison.OrdinalIgnoreCase))
            : null;

        source ??= displays.FirstOrDefault();
        if (source is not null)
        {
            source.SourceRect = new ScreenRect(
                region.Left - displayBounds.Left,
                region.Top - displayBounds.Top,
                width,
                height);
            source.OutputSize = new ScreenSize(width, height);
            source.Stretch = StretchMode.Uniform;
            options.SourceOptions = new SourceOptions
            {
                RecordingSources = new List<RecordingSourceBase> { source }
            };
        }
        else
        {
            options.OutputOptions.SourceRect = new ScreenRect(region.Left, region.Top, width, height);
        }

        options.OutputOptions.OutputFrameSize = new ScreenSize(width, height);
        return options;
    }

    private static ScreenRecordingDisplayBounds ResolveDisplayBounds(ScreenRecordingRegion region)
    {
        if (!string.IsNullOrWhiteSpace(region.DisplayDeviceName))
        {
            var screen = Forms.Screen.AllScreens.FirstOrDefault(candidate =>
                string.Equals(candidate.DeviceName, region.DisplayDeviceName, StringComparison.OrdinalIgnoreCase));
            if (screen is not null)
            {
                return new ScreenRecordingDisplayBounds(screen.Bounds.Left, screen.Bounds.Top);
            }
        }

        return new ScreenRecordingDisplayBounds(region.DisplayLeft, region.DisplayTop);
    }

    private void Recorder_OnRecordingComplete(object? sender, RecordingCompleteEventArgs e)
    {
        var path = !string.IsNullOrWhiteSpace(e.FilePath) ? e.FilePath : _activeFilePath;
        Log.Information("Recording complete: {Path}", path);
        _completion?.TrySetResult(new RecordingResult(ActiveMode, ActiveRegion, DateTime.Now, path));
    }

    private void Recorder_OnRecordingFailed(object? sender, RecordingFailedEventArgs e)
    {
        Log.Error("Recording failed: {Error}", e.Error);
        _completion?.TrySetException(new InvalidOperationException(e.Error));
    }

    private void Recorder_OnStatusChanged(object? sender, RecordingStatusEventArgs e)
    {
        Log.Information("Recorder backend status changed to {Status}", e.Status);
    }

    private static string CreateOutputFilePath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "MultiSnap", "Recordings");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"MultiSnap-{DateTime.Now:yyyyMMdd-HHmmss}.mp4");
    }

    private void ResetActiveRecording()
    {
        if (_recorder is not null)
        {
            _recorder.OnRecordingComplete -= Recorder_OnRecordingComplete;
            _recorder.OnRecordingFailed -= Recorder_OnRecordingFailed;
            _recorder.OnStatusChanged -= Recorder_OnStatusChanged;
            _recorder.Dispose();
        }

        _recorder = null;
        _completion = null;
        _activeFilePath = null;
        ActiveMode = null;
        ActiveRegion = null;
    }

    private void SetState(RecordingState state, string message)
    {
        State = state;
        Log.Information("Recording state changed to {State}: {Message}", state, message);
        StatusChanged?.Invoke(this, new RecordingStatusChangedEventArgs(state, message));
    }
}

public enum RecordingState
{
    Idle,
    Selecting,
    Countdown,
    Recording,
    Stopping,
    Cancelled,
    Failed
}

public enum RecordingMode
{
    Region,
    FullScreen
}

public sealed record ScreenRecordingRegion(
    int Left,
    int Top,
    int Width,
    int Height,
    string? DisplayDeviceName = null,
    int DisplayLeft = 0,
    int DisplayTop = 0);
public sealed record ScreenRecordingDisplayBounds(int Left, int Top);
public sealed record RecordingResult(
    RecordingMode? Mode,
    ScreenRecordingRegion? Region,
    DateTime CompletedAt,
    string? FilePath,
    string? ErrorMessage = null);
public sealed record RecordingStatusChangedEventArgs(RecordingState State, string Message);
