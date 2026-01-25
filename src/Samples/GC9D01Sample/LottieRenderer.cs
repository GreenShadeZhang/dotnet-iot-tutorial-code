using SkiaSharp;
using SkiaSharp.Resources;
using SkiaSharp.Skottie;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class LottieRenderer : IDisposable
{
    private readonly string _lottieFilePath;
    private SKData _lottieData;
    private Animation? _animation;
    private double _totalFrames;
    private double _totalSeconds;
    private double _framerate;
    private Stopwatch _playbackTimer = new Stopwatch();
    private double _lastRenderTime = 0;
    private bool _disposed = false;

    public LottieRenderer(string lottieFilePath)
    {
        _lottieFilePath = lottieFilePath;
        InitializeAnimation();
    }

    private void InitializeAnimation()
    {
        _lottieData = SKData.Create(_lottieFilePath);
        using var dataUriProvider = new DataUriResourceProvider(preDecode: true);
        _animation = Animation.CreateBuilder().SetResourceProvider(dataUriProvider).Build(_lottieData);
        if (_animation == null)
            throw new InvalidOperationException("无法加载Lottie动画");
        _totalFrames = (int)_animation.OutPoint;
        _totalSeconds = _animation.Duration.TotalSeconds;
        _framerate = _animation.Fps;
        _playbackTimer.Start();
    }

    public byte[] RenderFrame(int frameIndex, int width, int height)
    {
        if (_animation == null)
            throw new InvalidOperationException("动画未初始化");
        frameIndex = Math.Max(0, frameIndex % (int)_totalFrames);
        double timePoint = frameIndex / _framerate;
        return RenderAtTimePoint(timePoint, width, height);
    }

    private byte[] RenderAtTimePoint(double timePoint, int width, int height)
    {
        _lastRenderTime = _playbackTimer.Elapsed.TotalSeconds;
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        using var canvas = surface.Canvas;
        canvas.Clear(SKColors.Black);
        var rect = new SKRect(0, 0, width, height);
        _animation.SeekFrameTime(timePoint);
        _animation.Render(canvas, rect);
        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();
        return ConvertToRgb565(pixmap, width, height);
    }

    private byte[] ConvertToRgb565(SKPixmap pixmap, int width, int height)
    {
        byte[] buffer = new byte[width * height * 2];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SKColor color = pixmap.GetPixelColor(x, y);
                int r = color.Red >> 3;
                int g = color.Green >> 2;
                int b = color.Blue >> 3;
                ushort rgb565 = (ushort)(r << 11 | g << 5 | b);
                int pos = (y * width + x) * 2;
                buffer[pos] = (byte)(rgb565 >> 8);
                buffer[pos + 1] = (byte)(rgb565 & 0xFF);
            }
        }
        return buffer;
    }

    public double FrameCount => _totalFrames;
    public double Framerate => _framerate;
    public double TotalSeconds => _totalSeconds;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _animation?.Dispose();
                _lottieData?.Dispose();
                _playbackTimer?.Stop();
            }
            _disposed = true;
        }
    }

    ~LottieRenderer()
    {
        Dispose(false);
    }
}
