namespace RGBLed.Api.Models;

/// <summary>
/// LED效果类型
/// </summary>
public static class LedEffect
{
    /// <summary>
    /// 关闭
    /// </summary>
    public const string Off = "Off";
    
    /// <summary>
    /// 静态颜色
    /// </summary>
    public const string Static = "Static";
    
    /// <summary>
    /// 闪烁
    /// </summary>
    public const string Blink = "Blink";
    
    /// <summary>
    /// 呼吸灯
    /// </summary>
    public const string Breathe = "Breathe";
    
    /// <summary>
    /// 彩虹循环
    /// </summary>
    public const string Rainbow = "Rainbow";

    /// <summary>
    /// 获取所有可用效果
    /// </summary>
    public static string[] All => new[] { Off, Static, Blink, Breathe, Rainbow };
}

/// <summary>
/// LED颜色
/// </summary>
public class LedColor
{
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public LedColor() { }

    public LedColor(byte red, byte green, byte blue)
    {
        R = red;
        G = green;
        B = blue;
    }

    // 预定义颜色
    public static LedColor Black => new(0, 0, 0);
    public static LedColor Red => new(255, 0, 0);
    public static LedColor Green => new(0, 255, 0);
    public static LedColor Blue => new(0, 0, 255);
    public static LedColor White => new(255, 255, 255);
    public static LedColor Yellow => new(255, 255, 0);
    public static LedColor Cyan => new(0, 255, 255);
    public static LedColor Magenta => new(255, 0, 255);
    public static LedColor Orange => new(255, 165, 0);
    public static LedColor Purple => new(128, 0, 128);
}

/// <summary>
/// LED控制请求
/// </summary>
public class LedControlRequest
{
    /// <summary>
    /// 效果类型
    /// </summary>
    public string Effect { get; set; } = LedEffect.Static;
    
    /// <summary>
    /// 颜色
    /// </summary>
    public LedColor Color { get; set; } = LedColor.White;
    
    /// <summary>
    /// 亮度 (0-100)
    /// </summary>
    public int Brightness { get; set; } = 100;
    
    /// <summary>
    /// 速度/间隔时间(毫秒)
    /// </summary>
    public int Speed { get; set; } = 1000;
}

/// <summary>
/// LED状态
/// </summary>
public class LedStatus
{
    public string CurrentEffect { get; set; } = LedEffect.Off;
    public LedColor CurrentColor { get; set; } = LedColor.Black;
    public int Brightness { get; set; }
    public int Speed { get; set; }
    public bool IsRunning { get; set; }
    public string[] AvailableColors { get; set; } = Array.Empty<string>();
    public string[] AvailableEffects { get; set; } = Array.Empty<string>();
}
