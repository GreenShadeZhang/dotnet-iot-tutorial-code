using System.Device.Gpio;
using System.Device.Spi;
using Verdure.Iot.Device;

var gpio = new GpioController();

// SPI0, CE0, 10MHz, Mode0
var spiSettings = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 24_000_000,
    Mode = SpiMode.Mode0
};

// 根据实际接线修改GPIO编号
int dcPin = 25;    // 例：GPIO25
int resetPin = 27; // 例：GPIO24
int csPin = 8;     // 例：CE0, 可选

using var display = new GC9D01Display(spiSettings, gpio, dcPin, resetPin, csPin);

try
{
    // 先切换三次颜色
    FillScreen(display, 0xF800); // 红色
    Thread.Sleep(1000);
    FillScreen(display, 0x07E0); // 绿色
    Thread.Sleep(1000);
    FillScreen(display, 0x001F); // 蓝色
    Thread.Sleep(1000);

    // 播放Lottie动画（循环）
    var lottiePath = "LottieLogo.json"; // 使用本地文件，便于直接运行
    var player = new LottiePlayer(display, display.Width, display.Height, lottiePath);
    Console.WriteLine("开始播放Lottie动画，按任意键停止...");
    var playTask = player.PlayAnimationAsync(loops: -1, fps: 24);
    Console.ReadKey();
    player.StopAnimation();
    await playTask;
}
catch (Exception ex)
{
    Console.WriteLine($"错误: {ex.Message}");
}

void FillScreen(GC9D01Display display, ushort color) => display.FillScreen(color);
