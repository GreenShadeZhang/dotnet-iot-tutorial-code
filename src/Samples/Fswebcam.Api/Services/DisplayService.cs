using System.Device.Gpio;
using System.Device.Spi;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using Verdure.Iot.Device;

namespace Fswebcam.Api.Services;

/// <summary>
/// 屏幕显示服务 - 专门用于 2.4 寸屏幕显示
/// </summary>
public class DisplayService : IDisposable
{
    private ST7789Display? _display24Inch;
    private GpioController? _gpio;
    private readonly ILogger<DisplayService> _logger;
    private bool _disposed = false;

    // 2.4寸屏幕尺寸配置
    private const int Display24Width = 320;
    private const int Display24Height = 240;

    public DisplayService(ILogger<DisplayService> logger)
    {
        _logger = logger;

        // 只在 Linux 平台初始化硬件
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            InitializeDisplay();
        }
        else
        {
            _logger.LogWarning("非Linux平台，显示器初始化跳过");
        }
    }

    /// <summary>
    /// 初始化2.4寸显示器
    /// </summary>
    private void InitializeDisplay()
    {
        try
        {
            _gpio = new GpioController();

            // SPI 配置 - 使用 SPI0.0
            var settings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 24_000_000,
                Mode = SpiMode.Mode0,
            };

            // 创建2.4寸显示器
            _display24Inch = new ST7789Display(settings, _gpio, true, dcPin: 25, resetPin: 27, displayType: DisplayType.Display24Inch);

            // 清屏为黑色
            _display24Inch.FillScreen(0x0000);

            _logger.LogInformation("2.4寸显示器初始化成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示器初始化失败");
        }
    }

    /// <summary>
    /// 在2.4寸屏幕上显示图片
    /// </summary>
    /// <param name="imagePath">图片文件路径</param>
    public async Task DisplayImageAsync(string imagePath)
    {
        if (_display24Inch == null)
        {
            _logger.LogWarning("显示器未初始化，跳过图片显示");
            return;
        }

        if (!File.Exists(imagePath))
        {
            _logger.LogError($"图片文件不存在: {imagePath}");
            return;
        }

        try
        {
            _logger.LogInformation($"开始显示图片: {imagePath}");

            // 使用 ImageSharp 加载和处理图片
            using var image = await Image.LoadAsync<Bgr24>(imagePath);
            
            // 调整图片尺寸以适应屏幕
            var resizedImage = ResizeImageForDisplay(image, Display24Width, Display24Height);
            
            // 转换为屏幕所需的格式并显示
            var imageData = ConvertToDisplayFormat(resizedImage);
            
            await Task.Run(() => 
            {
                _display24Inch.SetAddressWindow(0, 0, Display24Width, Display24Height);
                _display24Inch.SendData(imageData);
            });

            _logger.LogInformation("图片显示完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"显示图片失败: {imagePath}");
        }
    }

    /// <summary>
    /// 从字节数组显示图片
    /// </summary>
    /// <param name="imageBytes">图片字节数组</param>
    public async Task DisplayImageAsync(byte[] imageBytes)
    {
        if (_display24Inch == null)
        {
            _logger.LogWarning("显示器未初始化，跳过图片显示");
            return;
        }

        try
        {
            _logger.LogInformation("开始显示图片 (从字节数组)");

            // 使用 ImageSharp 从字节数组加载图片
            using var image = Image.Load<Bgr24>(imageBytes);
            
            // 调整图片尺寸以适应屏幕
            var resizedImage = ResizeImageForDisplay(image, Display24Width, Display24Height);
            
            // 转换为屏幕所需的格式并显示
            var imageData = ConvertToDisplayFormat(resizedImage);
            
            await Task.Run(() => 
            {
                _display24Inch.SetAddressWindow(0, 0, Display24Width, Display24Height);
                _display24Inch.SendData(imageData);
            });

            _logger.LogInformation("图片显示完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示图片失败 (从字节数组)");
        }
    }

    /// <summary>
    /// 调整图片尺寸以适应屏幕
    /// </summary>
    private Image<Bgr24> ResizeImageForDisplay(Image<Bgr24> originalImage, int targetWidth, int targetHeight)
    {
        var resizedImage = originalImage.Clone();
        
        // 计算缩放比例，保持宽高比
        float scaleX = (float)targetWidth / originalImage.Width;
        float scaleY = (float)targetHeight / originalImage.Height;
        float scale = Math.Min(scaleX, scaleY);

        int newWidth = (int)(originalImage.Width * scale);
        int newHeight = (int)(originalImage.Height * scale);

        // 缩放图片
        resizedImage.Mutate(x => x.Resize(newWidth, newHeight));

        // 如果缩放后的图片小于目标尺寸，则居中放置在黑色背景上
        if (newWidth != targetWidth || newHeight != targetHeight)
        {
            var centeredImage = new Image<Bgr24>(targetWidth, targetHeight);
            centeredImage.Mutate(x => x.Fill(Color.Black)); // 黑色背景

            int offsetX = (targetWidth - newWidth) / 2;
            int offsetY = (targetHeight - newHeight) / 2;

            centeredImage.Mutate(x => x.DrawImage(resizedImage, new Point(offsetX, offsetY), 1f));
            
            resizedImage.Dispose();
            return centeredImage;
        }

        return resizedImage;
    }

    /// <summary>
    /// 将图片转换为显示器所需的RGB565格式
    /// </summary>
    private byte[] ConvertToDisplayFormat(Image<Bgr24> image)
    {
        byte[] buffer = new byte[image.Width * image.Height * 2]; // 16位/像素
        
        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                var pixel = image[x, y];
                
                // 转换为RGB565格式
                int r = pixel.R >> 3;  // 5位红色
                int g = pixel.G >> 2;  // 6位绿色 
                int b = pixel.B >> 3;  // 5位蓝色
                
                ushort rgb565 = (ushort)(r << 11 | g << 5 | b);
                
                // 存储为大端序
                int pos = (y * image.Width + x) * 2;
                buffer[pos] = (byte)(rgb565 >> 8);
                buffer[pos + 1] = (byte)(rgb565 & 0xFF);
            }
        }
        
        return buffer;
    }

    /// <summary>
    /// 显示文本信息
    /// </summary>
    /// <param name="text">要显示的文本</param>
    /// <param name="fontSize">字体大小</param>
    public async Task DisplayTextAsync(string text, int fontSize = 24)
    {
        if (_display24Inch == null)
        {
            _logger.LogWarning("显示器未初始化，跳过文本显示");
            return;
        }

        try
        {
            _logger.LogInformation($"显示文本: {text}");

            // 创建文本图像
            using var textImage = CreateTextImage(text, Display24Width, Display24Height, fontSize);
            
            // 转换为显示格式
            var imageData = ConvertToDisplayFormat(textImage);
            
            await Task.Run(() => 
            {
                _display24Inch.SetAddressWindow(0, 0, Display24Width, Display24Height);
                _display24Inch.SendData(imageData);
            });

            _logger.LogInformation("文本显示完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"显示文本失败: {text}");
        }
    }

    /// <summary>
    /// 创建文本图像
    /// </summary>
    private Image<Bgr24> CreateTextImage(string text, int width, int height, int fontSize)
    {
        var image = new Image<Bgr24>(width, height);
        
        try
        {
            // 尝试使用系统字体，如果失败则使用简单的文本显示
            var font = SystemFonts.CreateFont("Arial", fontSize, FontStyle.Bold);
            
            image.Mutate(x => x
                .Fill(Color.Black) // 黑色背景
                .DrawText(text, font, Color.White, new PointF(width / 2, height / 2))
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "无法使用系统字体，使用简单文本显示");
            
            // 如果字体加载失败，创建一个简单的文本显示
            image.Mutate(x => x.Fill(Color.Black));
            
            // 这里可以实现简单的像素级文本绘制，或者只显示背景色
            // 暂时只显示黑色背景
        }

        return image;
    }

    /// <summary>
    /// 清除屏幕
    /// </summary>
    /// <param name="color">清除的颜色 (RGB565格式，默认黑色)</param>
    public void ClearScreen(ushort color = 0x0000)
    {
        try
        {
            if (_display24Inch != null)
            {
                _display24Inch.FillScreen(color);
                _logger.LogDebug($"屏幕已清除 (颜色: 0x{color:X4})");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清屏失败");
        }
    }

    /// <summary>
    /// 显示状态信息
    /// </summary>
    public async Task DisplayStatusAsync(string status, bool isSuccess = true)
    {
        var color = isSuccess ? Color.Green : Color.Red;
        var bgColor = Color.Black;

        try
        {
            using var statusImage = new Image<Bgr24>(Display24Width, Display24Height);
            
            statusImage.Mutate(x => x.Fill(bgColor));
            
            try
            {
                var font = SystemFonts.CreateFont("Arial", 20, FontStyle.Bold);
                statusImage.Mutate(x => x
                    .DrawText(status, font, color, new PointF(Display24Width / 2, Display24Height / 2))
                );
            }
            catch (Exception fontEx)
            {
                _logger.LogWarning(fontEx, "字体加载失败，显示纯色状态");
                // 如果字体失败，显示纯色表示状态
                statusImage.Mutate(x => x.Fill(isSuccess ? Color.DarkGreen : Color.DarkRed));
            }

            var imageData = ConvertToDisplayFormat(statusImage);
            
            await Task.Run(() => 
            {
                _display24Inch?.SetAddressWindow(0, 0, Display24Width, Display24Height);
                _display24Inch?.SendData(imageData);
            });

            _logger.LogInformation($"状态显示完成: {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"显示状态失败: {status}");
        }
    }

    /// <summary>
    /// 检查显示器是否已初始化
    /// </summary>
    public bool IsInitialized => _display24Inch != null;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _display24Inch?.Dispose();
            _gpio?.Dispose();

            _logger.LogInformation("显示服务已释放资源");
            _disposed = true;
        }
    }
}
