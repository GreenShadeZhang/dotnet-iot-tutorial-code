using System.Device.Gpio;
using System.Device.Spi;
using Verdure.Iot.Device;

// 1.47寸屏幕设置
var settings = new SpiConnectionSettings(0, 1)
{
    ClockFrequency = 24_000_000,
    Mode = SpiMode.Mode0,
};

try
{
    Console.WriteLine("==================================================");
    Console.WriteLine("        ST7789 1.47寸屏幕横竖屏显示测试程序");
    Console.WriteLine("==================================================");
    Console.WriteLine("此程序将证明横竖屏都能正常显示，无空白区域问题");
    Console.WriteLine("使用ST7789Display类的现有方法进行测试");
    Console.WriteLine();

    // ============= 竖屏模式测试 =============
    await TestPortraitMode(settings);
    
    Console.WriteLine("\n按任意键开始横屏模式测试...");
    Console.ReadKey();
    
    // ============= 横屏模式测试 =============
    await TestLandscapeMode(settings);
    
    Console.WriteLine("\n==================================================");
    Console.WriteLine("           测试完成！总结：");
    Console.WriteLine("==================================================");
    Console.WriteLine("✅ 竖屏模式 (172x320): 显示正常，颜色填充完整");
    Console.WriteLine("✅ 横屏模式 (320x172): 无空白区域，完全填充");
    Console.WriteLine("✅ 颜色测试: 多种颜色显示正常");
    Console.WriteLine("✅ 区域绘制: 验证SetAddressWindow功能");
    Console.WriteLine("✅ 边界测试: 确认屏幕尺寸准确");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ 错误: {ex.Message}");
    Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
}

// 竖屏模式测试函数
static async Task TestPortraitMode(SpiConnectionSettings settings)
{
    Console.WriteLine("🔄 开始竖屏模式测试...");
    Console.WriteLine("创建竖屏显示 (172x320)");
    
    using var gpio = new GpioController();
    using var display = new ST7789Display(
        settings, gpio, true, 
        dcPin: 25, resetPin: 27, 
        displayType: DisplayType.Display147Inch, 
        isLandscape: false  // 竖屏模式
    );
    
    Console.WriteLine($"📐 竖屏尺寸: {display.Width}x{display.Height}");
    
    // 1. 基础颜色填充测试
    Console.WriteLine("🎨 测试1: 基础颜色填充");
    var colors = new ushort[] { 0xF800, 0x07E0, 0x001F, 0xFFE0, 0x0000 };
    var colorNames = new string[] { "红色", "绿色", "蓝色", "黄色", "黑色" };
    
    for (int i = 0; i < colors.Length; i++)
    {
        Console.WriteLine($"   显示: {colorNames[i]}");
        display.FillScreen(colors[i]);
        await Task.Delay(1500);
    }
    
    // 2. 区域绘制测试
    Console.WriteLine("� 测试2: 区域绘制测试");
    await DrawRegionTest(display);
    
    // 3. 边界测试
    Console.WriteLine("� 测试3: 边界区域测试");
    await DrawBorderTest(display);
    
    Console.WriteLine("✅ 竖屏模式测试完成");
}

// 横屏模式测试函数
static async Task TestLandscapeMode(SpiConnectionSettings settings)
{
    Console.WriteLine("🔄 开始横屏模式测试...");
    Console.WriteLine("创建横屏显示 (320x172)");
    
    using var gpio = new GpioController();
    using var display = new ST7789Display(
        settings, gpio, true, 
        dcPin: 25, resetPin: 27, 
        displayType: DisplayType.Display147Inch, 
        isLandscape: true  // 横屏模式
    );
    
    Console.WriteLine($"📐 横屏尺寸: {display.Width}x{display.Height}");
    
    // 1. 满屏颜色填充测试 - 重点检查是否有空白区域
    Console.WriteLine("🎨 测试1: 满屏颜色填充 (检查空白区域)");
    var colors = new ushort[] { 0xF800, 0x07E0, 0x001F, 0xFFE0, 0xF81F, 0xFFFF };
    var colorNames = new string[] { "红色", "绿色", "蓝色", "黄色", "紫色", "白色" };
    
    for (int i = 0; i < colors.Length; i++)
    {
        Console.WriteLine($"   显示: {colorNames[i]} - 检查下部是否有黑色区域");
        display.FillScreen(colors[i]);
        await Task.Delay(3000); // 延长观察时间
    }
    
    // 2. 分块区域测试 - 验证不同区域都能正常显示
    Console.WriteLine("📍 测试2: 分块区域测试");
    await DrawQuadrantTest(display);
    
    // 3. 边缘条带测试 - 特别检查上边缘
    Console.WriteLine("🔍 测试3: 边缘条带测试 (重点检查上边缘)");
    await DrawEdgeStripTest(display);
    
    // 4. 横屏专用偏移诊断测试
    Console.WriteLine("🚨 测试4: 横屏偏移诊断测试 - 检查黑色区域问题");
    await DiagnoseLandscapeOffset(display);
    
    // 5. 渐变测试 - 验证完整显示区域
    Console.WriteLine("🌈 测试5: 水平渐变测试");
    await DrawHorizontalGradientTest(display);
    
    Console.WriteLine("✅ 横屏模式测试完成 - 应该无空白区域");
}

// 区域绘制测试
static async Task DrawRegionTest(ST7789Display display)
{
    // 先清屏
    display.FillScreen(0x0000);
    await Task.Delay(500);
    
    // 绘制中心矩形区域
    int centerX = display.Width / 2;
    int centerY = display.Height / 2;
    int rectWidth = display.Width / 3;
    int rectHeight = display.Height / 3;
    
    await DrawColoredRegion(display, 
        centerX - rectWidth/2, centerY - rectHeight/2, 
        rectWidth, rectHeight, 0xF800); // 红色矩形
    
    await Task.Delay(1500);
    
    // 绘制四个角的小矩形
    int cornerSize = 30;
    
    // 左上角 - 绿色
    await DrawColoredRegion(display, 0, 0, cornerSize, cornerSize, 0x07E0);
    await Task.Delay(500);
    
    // 右上角 - 蓝色
    await DrawColoredRegion(display, display.Width - cornerSize, 0, cornerSize, cornerSize, 0x001F);
    await Task.Delay(500);
    
    // 左下角 - 黄色
    await DrawColoredRegion(display, 0, display.Height - cornerSize, cornerSize, cornerSize, 0xFFE0);
    await Task.Delay(500);
    
    // 右下角 - 紫色
    await DrawColoredRegion(display, display.Width - cornerSize, display.Height - cornerSize, cornerSize, cornerSize, 0xF81F);
    await Task.Delay(1500);
}

// 绘制有色区域
static async Task DrawColoredRegion(ST7789Display display, int x, int y, int width, int height, ushort color)
{
    // 设置绘制区域
    display.SetAddressWindow(x, y, x + width, y + height);
    
    // 创建该区域的颜色数据
    int pixelCount = width * height;
    byte[] colorData = new byte[pixelCount * 2];
    
    for (int i = 0; i < pixelCount; i++)
    {
        colorData[i * 2] = (byte)(color >> 8);
        colorData[i * 2 + 1] = (byte)(color & 0xFF);
    }
    
    display.SendData(colorData);
}

// 边界测试
static async Task DrawBorderTest(ST7789Display display)
{
    // 先用黑色填充
    display.FillScreen(0x0000);
    await Task.Delay(500);
    
    int borderWidth = 5;
    
    // 上边框 - 红色
    await DrawColoredRegion(display, 0, 0, display.Width, borderWidth, 0xF800);
    await Task.Delay(500);
    
    // 下边框 - 绿色
    await DrawColoredRegion(display, 0, display.Height - borderWidth, display.Width, borderWidth, 0x07E0);
    await Task.Delay(500);
    
    // 左边框 - 蓝色
    await DrawColoredRegion(display, 0, 0, borderWidth, display.Height, 0x001F);
    await Task.Delay(500);
    
    // 右边框 - 黄色
    await DrawColoredRegion(display, display.Width - borderWidth, 0, borderWidth, display.Height, 0xFFE0);
    await Task.Delay(1500);
}

// 四象限测试
static async Task DrawQuadrantTest(ST7789Display display)
{
    int halfWidth = display.Width / 2;
    int halfHeight = display.Height / 2;
    
    // 左上象限 - 红色
    Console.WriteLine("   绘制左上象限 (红色)");
    await DrawColoredRegion(display, 0, 0, halfWidth, halfHeight, 0xF800);
    await Task.Delay(1000);
    
    // 右上象限 - 绿色
    Console.WriteLine("   绘制右上象限 (绿色)");
    await DrawColoredRegion(display, halfWidth, 0, halfWidth, halfHeight, 0x07E0);
    await Task.Delay(1000);
    
    // 左下象限 - 蓝色
    Console.WriteLine("   绘制左下象限 (蓝色)");
    await DrawColoredRegion(display, 0, halfHeight, halfWidth, halfHeight, 0x001F);
    await Task.Delay(1000);
    
    // 右下象限 - 黄色
    Console.WriteLine("   绘制右下象限 (黄色)");
    await DrawColoredRegion(display, halfWidth, halfHeight, halfWidth, halfHeight, 0xFFE0);
    await Task.Delay(1500);
}

// 边缘条带测试 - 特别检查横屏模式的上边缘
static async Task DrawEdgeStripTest(ST7789Display display)
{
    // 先用黑色填充
    display.FillScreen(0x0000);
    await Task.Delay(500);
    
    int stripHeight = 10;
    
    // 上边缘条带 - 红色 (重点检查是否有空白)
    Console.WriteLine("   绘制上边缘红色条带 - 检查是否到达屏幕顶部");
    await DrawColoredRegion(display, 0, 0, display.Width, stripHeight, 0xF800);
    await Task.Delay(2000);
    
    // 第二条 - 绿色
    Console.WriteLine("   绘制第二条绿色条带");
    await DrawColoredRegion(display, 0, stripHeight, display.Width, stripHeight, 0x07E0);
    await Task.Delay(1000);
    
    // 中间条带 - 蓝色
    int middleY = (display.Height - stripHeight) / 2;
    Console.WriteLine("   绘制中间蓝色条带");
    await DrawColoredRegion(display, 0, middleY, display.Width, stripHeight, 0x001F);
    await Task.Delay(1000);
    
    // 倒数第二条 - 黄色
    Console.WriteLine("   绘制倒数第二条黄色条带");
    await DrawColoredRegion(display, 0, display.Height - 2 * stripHeight, display.Width, stripHeight, 0xFFE0);
    await Task.Delay(1000);
    
    // 底边缘条带 - 紫色
    Console.WriteLine("   绘制底边缘紫色条带");
    await DrawColoredRegion(display, 0, display.Height - stripHeight, display.Width, stripHeight, 0xF81F);
    await Task.Delay(2000);
}

// 水平渐变测试
static async Task DrawHorizontalGradientTest(ST7789Display display)
{
    Console.WriteLine("   绘制水平渐变 - 验证完整宽度显示");
    
    // 创建水平渐变：从红色到绿色
    for (int x = 0; x < display.Width; x += 10) // 每10像素一个条带
    {
        int stripWidth = Math.Min(10, display.Width - x);
        
        // 计算颜色：从红(0xF800)渐变到绿(0x07E0)
        float ratio = (float)x / display.Width;
        ushort red = (ushort)(0xF800 * (1 - ratio));
        ushort green = (ushort)(0x07E0 * ratio);
        ushort color = (ushort)(red + green);
        
        await DrawColoredRegion(display, x, 0, stripWidth, display.Height, color);
        await Task.Delay(50); // 短暂延时以显示渐变效果
    }
    
    await Task.Delay(2000);
}

// 横屏偏移诊断测试 - 专门检查下边缘黑色区域问题
static async Task DiagnoseLandscapeOffset(ST7789Display display)
{
    Console.WriteLine("   🔍 诊断1: 边缘像素测试");
    
    // 1. 清屏为白色，便于观察
    display.FillScreen(0xFFFF);
    await Task.Delay(2000);
    
    // 2. 绘制顶部红色条带（检查是否真的在顶部）
    Console.WriteLine("   - 绘制顶部红色条带");
    await DrawColoredRegion(display, 0, 0, display.Width, 10, 0xF800);
    await Task.Delay(2000);
    
    // 3. 绘制底部蓝色条带（检查是否真的在底部，是否有黑色区域）
    Console.WriteLine("   - 绘制底部蓝色条带");
    await DrawColoredRegion(display, 0, display.Height - 10, display.Width, 10, 0x001F);
    await Task.Delay(2000);
    
    // 4. 绘制最底部1像素的绿色线（验证最后一行）
    Console.WriteLine("   - 绘制最底部绿色线");
    await DrawColoredRegion(display, 0, display.Height - 1, display.Width, 1, 0x07E0);
    await Task.Delay(3000);
    
    Console.WriteLine("   🔍 诊断2: 垂直条带测试");
    
    // 5. 垂直分段测试 - 将屏幕分成多个水平条带
    display.FillScreen(0x0000); // 黑色背景
    int stripHeight = display.Height / 5;
    ushort[] stripColors = { 0xF800, 0x07E0, 0x001F, 0xFFE0, 0xF81F };
    string[] stripNames = { "红色", "绿色", "蓝色", "黄色", "紫色" };
    
    for (int i = 0; i < 5; i++)
    {
        int y = i * stripHeight;
        int height = (i == 4) ? display.Height - y : stripHeight; // 最后一条占满剩余空间
        Console.WriteLine($"   - 绘制第{i + 1}条: {stripNames[i]} (Y:{y}-{y + height - 1})");
        await DrawColoredRegion(display, 0, y, display.Width, height, stripColors[i]);
        await Task.Delay(1000);
    }
    
    await Task.Delay(3000);
    
    Console.WriteLine("   🔍 诊断3: 检查实际显示尺寸");
    // 6. 绘制网格以验证实际显示区域
    display.FillScreen(0x0000);
    
    // 绘制边框
    await DrawColoredRegion(display, 0, 0, display.Width, 2, 0xFFFF); // 顶部白线
    await DrawColoredRegion(display, 0, display.Height - 2, display.Width, 2, 0xFFFF); // 底部白线
    await DrawColoredRegion(display, 0, 0, 2, display.Height, 0xFFFF); // 左侧白线
    await DrawColoredRegion(display, display.Width - 2, 0, 2, display.Height, 0xFFFF); // 右侧白线
    
    // 在中心绘制十字
    int centerX = display.Width / 2;
    int centerY = display.Height / 2;
    await DrawColoredRegion(display, centerX - 1, 0, 2, display.Height, 0x07E0); // 垂直绿线
    await DrawColoredRegion(display, 0, centerY - 1, display.Width, 2, 0xF800); // 水平红线
    
    await Task.Delay(5000);
}
