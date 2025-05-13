// 定义SPI设备设置
using System.Device.Gpio;
using System.Device.Spi;
using Verdure.Iot.Device;

var gpio = new GpioController();

var settings1 = new SpiConnectionSettings(0, 0)
{
    ClockFrequency = 24_000_000, // 尝试降低SPI时钟频率以减少闪烁
    Mode = SpiMode.Mode0,
};

var settings2 = new SpiConnectionSettings(0, 1)
{
    ClockFrequency = 24_000_000,
    Mode = SpiMode.Mode0,
};

// 创建显示屏对象
using (var display1 = new ST7789Display(settings1, gpio, dcPin: 25, resetPin: 27, displayType: DisplayType.Display24Inch))
using (var display2 = new ST7789Display(settings2, gpio, dcPin: 25, resetPin: 27, displayType: DisplayType.Display147Inch))
{
    try
    {
        while (true)
        {
            // 首先完成第一个屏幕的绘制
            display1.FillScreen(0xF800);  // 红色
                                          // 添加延时确保CS信号稳定
            Thread.Sleep(10);

            // 然后绘制第二个屏幕
            display2.FillScreen(0x07E0);  // 绿色
            Thread.Sleep(10);

            Thread.Sleep(1000);

            display1.FillScreen(0x001F);  // 蓝色
            Thread.Sleep(10);

            display2.FillScreen(0xFFE0);  // 黄色
            Thread.Sleep(10);

            Thread.Sleep(1000);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"错误: {ex.Message}");
    }
}