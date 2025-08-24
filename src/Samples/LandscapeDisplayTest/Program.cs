using System.Device.Gpio;
using System.Device.Spi;
using Verdure.Iot.Device;

var gpio = new GpioController();

// 1.47寸屏幕设置
var settings = new SpiConnectionSettings(0, 1)
{
    ClockFrequency = 24_000_000,
    Mode = SpiMode.Mode0,
};

try
{
    Console.WriteLine("正在测试1.47寸屏幕横屏显示...");
    
    // 测试竖屏模式
    Console.WriteLine("创建竖屏模式屏幕 (172x320)");
    using var portraitDisplay = new ST7789Display(
        settings, gpio, true, 
        dcPin: 25, resetPin: 27, 
        displayType: DisplayType.Display147Inch, 
        isLandscape: false  // 竖屏模式
    );
    
    Console.WriteLine($"竖屏尺寸: {portraitDisplay.Width}x{portraitDisplay.Height}");
    
    // 竖屏红色测试
    portraitDisplay.FillScreen(0xF800); // 红色
    Console.WriteLine("竖屏显示红色，按任意键继续...");
    Console.ReadKey();
    
    portraitDisplay.Dispose();
    
    Thread.Sleep(500);
    
    // 测试横屏模式 
    Console.WriteLine("创建横屏模式屏幕 (320x172)");
    using var landscapeDisplay = new ST7789Display(
        settings, gpio, true, 
        dcPin: 25, resetPin: 27, 
        displayType: DisplayType.Display147Inch, 
        isLandscape: true  // 横屏模式  
    );
    
    Console.WriteLine($"横屏尺寸: {landscapeDisplay.Width}x{landscapeDisplay.Height}");
    
    // 横屏测试 - 应该没有空白区域
    var colors = new ushort[] 
    { 
        0xF800, // 红色
        0x07E0, // 绿色  
        0x001F, // 蓝色
        0xFFE0, // 黄色
        0x0000  // 黑色
    };

    var colorNames = new string[] 
    { 
        "红色", "绿色", "蓝色", "黄色", "黑色" 
    };

    Console.WriteLine("横屏颜色测试开始...");
    
    for (int i = 0; i < colors.Length; i++)
    {
        Console.WriteLine($"显示: {colorNames[i]}");
        landscapeDisplay.FillScreen(colors[i]);
        Console.WriteLine("检查是否还有空白区域，按任意键继续下一个颜色...");
        Console.ReadKey();
    }
    
    Console.WriteLine("测试完成！横屏模式应该没有空白区域了。");
}
catch (Exception ex)
{
    Console.WriteLine($"错误: {ex.Message}");
    Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
}
finally
{
    gpio?.Dispose();
}
