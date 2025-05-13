// 定义SPI设置
using DualDisplayLottieExample;
using System.Device.Spi;
using System.Runtime.InteropServices;
using Verdure.Iot.Device;

var settings1 = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 20_000_000,  // 稍低的SPI频率以减少闪烁
    Mode = SpiMode.Mode0,
    DataBitLength = 8
};

var settings2 = new SpiConnectionSettings(0, 1)
{
    ClockFrequency = 20_000_000,
    Mode = SpiMode.Mode0,
    DataBitLength = 8
};

try
{
    DualLottiePlayer? player = null;
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        // 创建两个显示对象
        using var display1 = new ST7789Display(settings1, dcPin: 25, resetPin: 27, csPin: 8, DisplayType.Display24Inch);
        using var display2 = new ST7789Display(settings2, dcPin: 25, resetPin: 27, csPin: 7, DisplayType.Display147Inch);

        // 清屏以准备播放动画
        display1.FillScreen(0x0000);  // 黑色
        display2.FillScreen(0x0000);  // 黑色

        // 创建动画播放器
        player = new DualLottiePlayer(
            display1, 320, 240,  // 2.4寸屏幕
            display2, 172, 320,  // 1.47寸屏幕
            "ask.json"
        );
    }
    else
    {
        // 创建动画播放器
        player = new DualLottiePlayer(
            null, 320, 240,  // 2.4寸屏幕
            null, 172, 320,  // 1.47寸屏幕
            "file-plus.json"
        );
    }


    Console.WriteLine("开始播放动画，按任意键停止...");

    // 开始播放动画（循环播放）
    var playTask = player.PlayAnimationAsync(loops: -1, fps: 30);

    // 等待用户输入停止
    Console.ReadKey();
    player.StopAnimation();

    await playTask;  // 等待动画任务结束
}
catch (Exception ex)
{
    Console.WriteLine($"发生错误: {ex.Message}");
}