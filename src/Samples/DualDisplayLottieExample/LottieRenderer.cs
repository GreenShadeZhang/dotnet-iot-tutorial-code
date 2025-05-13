using SkiaSharp;
using SkiaSharp.Skottie;

namespace DualDisplayLottieExample;

public class LottieRenderer
{
    private readonly string _lottieFilePath;
    private SKData _lottieData;
    private Animation _animation;
    private double _totalFrames;
    private double _totalSeconds;
    private double _framerate;

    public LottieRenderer(string lottieFilePath)
    {
        _lottieFilePath = lottieFilePath;
        InitializeAnimation();
    }

    private void InitializeAnimation()
    {
        try
        {
            _lottieData = SKData.Create(_lottieFilePath);
            _animation = Animation.Create(_lottieData);
            _totalFrames = (int)_animation.OutPoint;
            _totalSeconds = _animation.Duration.TotalSeconds;
            _framerate = _animation.Fps;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lottie 动画加载错误: {ex.Message}");
            throw;
        }
    }

    // 渲染指定帧到位图
    public byte[] RenderFrame(int frameIndex, int width, int height)
    {
        if (_animation == null)
            throw new InvalidOperationException("动画未初始化");

        // 确保帧索引在有效范围内
        frameIndex = (int)(frameIndex % _totalFrames);

        // 计算时间点
        double timePoint = frameIndex / _totalFrames * _totalSeconds;

        // 创建适合目标屏幕的Surface
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        using var canvas = surface.Canvas;

        // 清除背景
        canvas.Clear(SKColors.Black);

        // 设置动画大小以适应屏幕
        var rect = new SKRect(0, 0, width, height);

        // 在指定时间点渲染帧
        _animation.SeekFrameTime(timePoint);
        _animation.Render(canvas, rect);

        // 获取图像
        using var image = surface.Snapshot();
        using var pixmap = image.PeekPixels();

        // 转换为RGB565格式（ST7789V3兼容）
        return ConvertToRgb565(pixmap, width, height);
    }

    // 将SkiaSharp像素转换为RGB565格式
    private byte[] ConvertToRgb565(SKPixmap pixmap, int width, int height)
    {
        byte[] buffer = new byte[width * height * 2]; // 16位/像素

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SKColor color = pixmap.GetPixelColor(x, y);

                // 转换为RGB565格式
                int r = color.Red >> 3;
                int g = color.Green >> 2;
                int b = color.Blue >> 3;
                ushort rgb565 = (ushort)(r << 11 | g << 5 | b);

                // 存储为大端序
                int pos = (y * width + x) * 2;
                buffer[pos] = (byte)(rgb565 >> 8);
                buffer[pos + 1] = (byte)(rgb565 & 0xFF);
            }
        }

        return buffer;
    }

    public double FrameCount => _totalFrames;
    public double Framerate => _framerate;
}
