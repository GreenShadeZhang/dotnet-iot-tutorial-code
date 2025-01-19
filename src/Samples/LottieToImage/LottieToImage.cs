using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SkiaSharp;
using SkiaSharp.Skottie;

namespace Verdure.LottieToImage;

public class LottieToImage
{
    public static Image<Rgba32> RenderLottieFrame(Animation animation, double progress, int width, int height)
    {

        // 创建SKSurface用于渲染
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        // 清除背景
        canvas.Clear(SKColors.Transparent);

        animation.SeekFrameTime(progress);
        animation.Render(canvas, new SKRect(0, 0, width, height));

        // 将SKBitmap转换为byte数组
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var bytes = data.ToArray();

        // 转换为ImageSharp格式
        using var memStream = new MemoryStream(bytes);
        return Image.Load<Rgba32>(memStream);
    }

    public static async Task SaveLottieFramesAsync(string lottieJsonPath, string outputDir, int width, int height)
    {
        Directory.CreateDirectory(outputDir);
        // 读取Lottie JSON文件
        var animation = Animation.Create(lottieJsonPath);
        if (animation != null)
        {
            //帧数
            var frameCount = animation.OutPoint;
            for (int i = 0; i < frameCount; i++)
            {
                var progress = animation.Duration.TotalSeconds / (frameCount - i);
                var frame = RenderLottieFrame(animation, progress, width, height);
                await frame.SaveAsPngAsync(Path.Combine(outputDir, $"frame_{i:D4}.png"));
            }
        }
    }
}
