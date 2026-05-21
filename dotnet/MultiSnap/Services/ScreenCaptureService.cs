using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;

namespace MultiSnap.Services;

public sealed class ScreenCaptureService
{
    public BitmapSource CaptureCursorDisplay()
    {
        var screen = Forms.Screen.FromPoint(Forms.Cursor.Position);
        return CaptureBounds(screen.Bounds);
    }

    public BitmapSource CaptureBounds(Rectangle bounds)
    {
        using var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);
        return ToBitmapSource(bitmap);
    }

    public BitmapSource Crop(BitmapSource source, Int32Rect crop)
    {
        var safeCrop = new Int32Rect(
            Math.Max(0, crop.X),
            Math.Max(0, crop.Y),
            Math.Min(crop.Width, source.PixelWidth - crop.X),
            Math.Min(crop.Height, source.PixelHeight - crop.Y));

        if (safeCrop.Width <= 0 || safeCrop.Height <= 0)
        {
            return source;
        }

        var cropped = new CroppedBitmap(source, safeCrop);
        cropped.Freeze();
        return cropped;
    }

    public void SavePng(BitmapSource image, string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        using var stream = File.Create(filePath);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        encoder.Save(stream);
    }

    private static BitmapSource ToBitmapSource(Bitmap bitmap)
    {
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        stream.Position = 0;

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
