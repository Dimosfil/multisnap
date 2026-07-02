using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
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
    private readonly Stack<InkUndoAction> _inkUndoStack = new();
    private bool _isApplyingInkUndo;

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
        InkLayer.StrokeCollected += InkLayer_StrokeCollected;
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, Undo_Executed, Undo_CanExecute));
        InputBindings.Add(new KeyBinding(ApplicationCommands.Undo, new KeyGesture(Key.Z, ModifierKeys.Control)));
        ApplySettings(settings);
    }

    public void LoadImage(BitmapSource image)
    {
        _currentImage = image;
        CaptureImage.Source = image;
        EmptyState.Visibility = Visibility.Collapsed;
        _inkUndoStack.Clear();
        InkLayer.Strokes.Clear();
        CommandManager.InvalidateRequerySuggested();
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
        if (InkLayer.Strokes.Count == 0)
        {
            return;
        }

        _inkUndoStack.Push(InkUndoAction.RestoreStrokes(InkLayer.Strokes.Clone()));
        InkLayer.Strokes.Clear();
        CommandManager.InvalidateRequerySuggested();
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

    private void InkLayer_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
    {
        if (_isApplyingInkUndo)
        {
            return;
        }

        _inkUndoStack.Push(InkUndoAction.RemoveStroke(e.Stroke));
        CommandManager.InvalidateRequerySuggested();
    }

    private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = _inkUndoStack.Count > 0;
        e.Handled = true;
    }

    private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        if (_inkUndoStack.Count == 0)
        {
            return;
        }

        var action = _inkUndoStack.Pop();
        _isApplyingInkUndo = true;
        try
        {
            switch (action.Kind)
            {
                case InkUndoActionKind.RemoveStroke:
                    if (action.Stroke is not null && InkLayer.Strokes.Contains(action.Stroke))
                    {
                        InkLayer.Strokes.Remove(action.Stroke);
                    }
                    break;
                case InkUndoActionKind.RestoreStrokes:
                    if (action.Strokes is not null)
                    {
                        InkLayer.Strokes = action.Strokes.Clone();
                    }
                    break;
            }
        }
        finally
        {
            _isApplyingInkUndo = false;
        }

        CommandManager.InvalidateRequerySuggested();
        e.Handled = true;
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

            var imageBounds = GetDisplayedImageBounds(_currentImage);
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

    private Rect GetDisplayedImageBounds(BitmapSource image)
    {
        var imageTopLeft = CaptureImage.TranslatePoint(new System.Windows.Point(0, 0), InkLayer);
        var controlWidth = Math.Max(1, CaptureImage.ActualWidth);
        var controlHeight = Math.Max(1, CaptureImage.ActualHeight);
        var imageAspect = image.Width / Math.Max(1.0, image.Height);
        var controlAspect = controlWidth / controlHeight;

        if (controlAspect > imageAspect)
        {
            var displayedHeight = controlHeight;
            var displayedWidth = displayedHeight * imageAspect;
            return new Rect(imageTopLeft.X + (controlWidth - displayedWidth) / 2, imageTopLeft.Y, displayedWidth, displayedHeight);
        }

        var width = controlWidth;
        var height = width / imageAspect;
        return new Rect(imageTopLeft.X, imageTopLeft.Y + (controlHeight - height) / 2, width, height);
    }

    private enum InkUndoActionKind
    {
        RemoveStroke,
        RestoreStrokes
    }

    private sealed record InkUndoAction(InkUndoActionKind Kind, Stroke? Stroke, StrokeCollection? Strokes)
    {
        public static InkUndoAction RemoveStroke(Stroke stroke) => new(InkUndoActionKind.RemoveStroke, stroke, null);

        public static InkUndoAction RestoreStrokes(StrokeCollection strokes) => new(InkUndoActionKind.RestoreStrokes, null, strokes);
    }
}
