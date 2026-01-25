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
    while (true)
    {
        FillScreen(display, 0xF800); // 红色
        Thread.Sleep(1000);
        FillScreen(display, 0x07E0); // 绿色
        Thread.Sleep(1000);
        FillScreen(display, 0x001F); // 蓝色
        Thread.Sleep(1000);
        FillScreen(display, 0xFFFF); // 白色
        Thread.Sleep(1000);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"错误: {ex.Message}");
}

// 业务代码无需关心窗口和缓冲区，直接调用驱动方法
void FillScreen(GC9D01Display display, ushort color) => display.FillScreen(color);
