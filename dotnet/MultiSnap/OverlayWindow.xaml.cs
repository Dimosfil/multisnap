using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MultiSnap.Services;
using Forms = System.Windows.Forms;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfPoint = System.Windows.Point;

namespace MultiSnap;

public partial class OverlayWindow : Window
{
    private readonly BitmapSource _screenshot;
    private readonly ScreenCaptureService _capture;
    private WpfPoint? _startPoint;

    public OverlayWindow(BitmapSource screenshot, ScreenCaptureService capture)
    {
        _screenshot = screenshot;
        _capture = capture;
        InitializeComponent();

        var screen = Forms.Screen.FromPoint(Forms.Cursor.Position);
        Left = screen.Bounds.Left;
        Top = screen.Bounds.Top;
        Width = screen.Bounds.Width;
        Height = screen.Bounds.Height;
        ScreenshotImage.Source = screenshot;
        Focusable = true;
        Loaded += (_, _) => Focus();
    }

    public BitmapSource? CapturedImage { get; private set; }

    private void Overlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(SelectionCanvas);
        SelectionRect.Visibility = Visibility.Visible;
        UpdateSelection(_startPoint.Value, _startPoint.Value);
        Mouse.Capture(SelectionCanvas);
    }

    private void Overlay_MouseMove(object sender, WpfMouseEventArgs e)
    {
        if (_startPoint is WpfPoint start && e.LeftButton == MouseButtonState.Pressed)
        {
            UpdateSelection(start, e.GetPosition(SelectionCanvas));
        }
    }

    private void Overlay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_startPoint is not WpfPoint start)
        {
            return;
        }

        var end = e.GetPosition(SelectionCanvas);
        Mouse.Capture(null);
        _startPoint = null;

        var selection = Normalize(start, end);
        if (selection.Width < 4 || selection.Height < 4)
        {
            DialogResult = false;
            Close();
            return;
        }

        var scaleX = _screenshot.PixelWidth / Math.Max(1, ActualWidth);
        var scaleY = _screenshot.PixelHeight / Math.Max(1, ActualHeight);
        CapturedImage = _capture.Crop(
            _screenshot,
            new Int32Rect(
                (int)Math.Round(selection.X * scaleX),
                (int)Math.Round(selection.Y * scaleY),
                (int)Math.Round(selection.Width * scaleX),
                (int)Math.Round(selection.Height * scaleY)));
        DialogResult = true;
        Close();
    }

    private void Overlay_KeyDown(object sender, WpfKeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }

    private void UpdateSelection(WpfPoint start, WpfPoint end)
    {
        var selection = Normalize(start, end);
        Canvas.SetLeft(SelectionRect, selection.Left);
        Canvas.SetTop(SelectionRect, selection.Top);
        SelectionRect.Width = selection.Width;
        SelectionRect.Height = selection.Height;
    }

    private static Rect Normalize(WpfPoint start, WpfPoint end)
    {
        return new Rect(
            Math.Min(start.X, end.X),
            Math.Min(start.Y, end.Y),
            Math.Abs(start.X - end.X),
            Math.Abs(start.Y - end.Y));
    }
}
