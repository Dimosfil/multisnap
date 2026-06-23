using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MultiSnap.Core;
using MultiSnap.Services;
using Serilog;
using WpfClipboard = System.Windows.Clipboard;
using WpfColor = System.Windows.Media.Color;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace MultiSnap;

public partial class MainWindow : Window
{
    private const double DefaultBrushSize = 3;
    private readonly ScreenCaptureService _capture;
    private readonly Action _startAreaCapture;
    private readonly Action _captureFullScreen;
    private AppSettings _settings;
    private BitmapSource? _currentImage;

    public MainWindow(ScreenCaptureService capture, Action startAreaCapture, Action captureFullScreen, AppSettings settings)
    {
        _capture = capture;
        _startAreaCapture = startAreaCapture;
        _captureFullScreen = captureFullScreen;
        _settings = settings;
        InitializeComponent();
        InkLayer.DefaultDrawingAttributes.Color = Colors.Red;
        ApplyBrushSize(DefaultBrushSize);
        BrushSizeSlider.ValueChanged += BrushSizeSlider_ValueChanged;
        ApplySettings(settings);
    }

    public void LoadImage(BitmapSource image)
    {
        _currentImage = image;
        CaptureImage.Source = image;
        EmptyState.Visibility = Visibility.Collapsed;
        InkLayer.Strokes.Clear();
    }

    public void ApplySettings(AppSettings settings)
    {
        _settings = settings;
        AreaHotkeyText.Text = settings.AreaCaptureHotkey;
        VideoHotkeyText.Text = settings.VideoRecordingHotkey;
        ClipboardStatusText.Text = settings.CopyCapturedImageToClipboard
            ? "Captured images are copied to clipboard immediately."
            : "Captured images stay in the editor until copied or saved.";
    }

    public void SetHotkeyStatus(HotkeyRegistrationResult registered, string areaHotkey, string videoHotkey)
    {
        AreaHotkeyText.Text = areaHotkey;
        VideoHotkeyText.Text = videoHotkey;
        HotkeyStatusText.Text = registered is { AreaCaptureRegistered: true, VideoRecordingRegistered: true }
            ? "Active"
            : "Partially unavailable";
        HotkeyStatusText.Foreground = registered is { AreaCaptureRegistered: true, VideoRecordingRegistered: true }
            ? new SolidColorBrush(WpfColor.FromRgb(56, 217, 150))
            : new SolidColorBrush(WpfColor.FromRgb(255, 104, 104));
    }

    private void CaptureArea_Click(object sender, RoutedEventArgs e)
    {
        _startAreaCapture();
    }

    private void CaptureFull_Click(object sender, RoutedEventArgs e)
    {
        _captureFullScreen();
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        var composed = ComposeImage();
        if (composed is not null)
        {
            WpfClipboard.SetImage(composed);
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var composed = ComposeImage();
        if (composed is null)
        {
            return;
        }

        var dialog = new WpfSaveFileDialog
        {
            Filter = "PNG image (*.png)|*.png",
            FileName = $"multisnap-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png"
        };

        if (dialog.ShowDialog(this) == true)
        {
            _capture.SavePng(composed, dialog.FileName);
            Log.Information("Saved screenshot to {Path}", dialog.FileName);
        }
    }

    private void ClearInk_Click(object sender, RoutedEventArgs e)
    {
        InkLayer.Strokes.Clear();
    }

    private void BrushSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        ApplyBrushSize(e.NewValue);
    }

    private void ApplyBrushSize(double value)
    {
        var brushSize = Math.Round(value);
        InkLayer.DefaultDrawingAttributes.Width = brushSize;
        InkLayer.DefaultDrawingAttributes.Height = brushSize;
        BrushSizeValueText.Text = $"{brushSize:0} px";
    }

    private BitmapSource? ComposeImage()
    {
        if (_currentImage is null)
        {
            return null;
        }

        var originalWidth = _currentImage.PixelWidth;
        var originalHeight = _currentImage.PixelHeight;
        var visual = new DrawingVisual();

        using (var context = visual.RenderOpen())
        {
            context.DrawImage(_currentImage, new Rect(0, 0, originalWidth, originalHeight));

            var group = new DrawingGroup();
            using (var inkContext = group.Open())
            {
                InkLayer.Strokes.Draw(inkContext);
            }

            var imageBounds = GetDisplayedImageBounds(originalWidth, originalHeight);
            var scaleX = originalWidth / Math.Max(1, imageBounds.Width);
            var scaleY = originalHeight / Math.Max(1, imageBounds.Height);
            var inkToImage = new Matrix(
                scaleX,
                0,
                0,
                scaleY,
                -imageBounds.X * scaleX,
                -imageBounds.Y * scaleY);

            context.PushClip(new RectangleGeometry(new Rect(0, 0, originalWidth, originalHeight)));
            context.PushTransform(new MatrixTransform(inkToImage));
            context.DrawDrawing(group);
            context.Pop();
            context.Pop();
        }

        var bitmap = new RenderTargetBitmap(originalWidth, originalHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    private Rect GetDisplayedImageBounds(int imagePixelWidth, int imagePixelHeight)
    {
        var controlWidth = Math.Max(1, CaptureImage.ActualWidth);
        var controlHeight = Math.Max(1, CaptureImage.ActualHeight);
        var imageAspect = imagePixelWidth / Math.Max(1.0, imagePixelHeight);
        var controlAspect = controlWidth / controlHeight;

        if (controlAspect > imageAspect)
        {
            var displayedHeight = controlHeight;
            var displayedWidth = displayedHeight * imageAspect;
            return new Rect((controlWidth - displayedWidth) / 2, 0, displayedWidth, displayedHeight);
        }

        var width = controlWidth;
        var height = width / imageAspect;
        return new Rect(0, (controlHeight - height) / 2, width, height);
    }
}
