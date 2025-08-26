using System.Device.Gpio;
using System.Device.Pwm;
using RGBLed.Api.Models;

namespace RGBLed.Api.Services;

/// <summary>
/// RGB LED控制服务
/// </summary>
public class RgbLedService : IDisposable
{
    private readonly GpioController _gpio;
    private readonly PwmChannel _redChannel;
    private readonly PwmChannel _greenChannel;
    private readonly PwmChannel _blueChannel;
    
    // 按钮GPIO引脚
    private readonly int _button1Pin;
    private readonly int _button2Pin;
    private readonly int _button3Pin;
    
    private CancellationTokenSource? _effectCancellationSource;
    private Task? _effectTask;
    
    private string _currentEffect = LedEffect.Off;
    private LedColor _currentColor = LedColor.Black;
    private int _currentBrightness = 100;
    private int _currentSpeed = 1000;
    
    // 按钮防抖动
    private DateTime _lastButton1Press = DateTime.MinValue;
    private DateTime _lastButton2Press = DateTime.MinValue;
    private DateTime _lastButton3Press = DateTime.MinValue;
    private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(200);
    
    // 事件
    public event Action<int>? ButtonPressed;
    public event Action<LedStatus>? StatusChanged;

    public RgbLedService(
        int redPin = 18, int greenPin = 19, int bluePin = 20,
        int button1Pin = 2, int button2Pin = 3, int button3Pin = 17,
        int pwmChip = 0)
    {
        _gpio = new GpioController();
        
        // 初始化按钮引脚
        _button1Pin = button1Pin;
        _button2Pin = button2Pin;
        _button3Pin = button3Pin;
        
        InitializeButtons();
        
        // 初始化PWM通道用于LED控制
        try
        {
            _redChannel = PwmChannel.Create(pwmChip, redPin, 1000); // 1kHz频率
            _greenChannel = PwmChannel.Create(pwmChip, greenPin, 1000);
            _blueChannel = PwmChannel.Create(pwmChip, bluePin, 1000);
            
            _redChannel.Start();
            _greenChannel.Start();
            _blueChannel.Start();
        }
        catch (Exception)
        {
            // 如果PWM不可用，回退到GPIO
            _gpio.OpenPin(redPin, PinMode.Output);
            _gpio.OpenPin(greenPin, PinMode.Output);
            _gpio.OpenPin(bluePin, PinMode.Output);
            
            // 关闭LED
            _gpio.Write(redPin, PinValue.Low);
            _gpio.Write(greenPin, PinValue.Low);
            _gpio.Write(bluePin, PinValue.Low);
        }
    }

    private void InitializeButtons()
    {
        try
        {
            _gpio.OpenPin(_button1Pin, PinMode.InputPullUp);
            _gpio.OpenPin(_button2Pin, PinMode.InputPullUp);
            _gpio.OpenPin(_button3Pin, PinMode.InputPullUp);
            
            _gpio.RegisterCallbackForPinValueChangedEvent(_button1Pin, PinEventTypes.Falling, OnButton1Pressed);
            _gpio.RegisterCallbackForPinValueChangedEvent(_button2Pin, PinEventTypes.Falling, OnButton2Pressed);
            _gpio.RegisterCallbackForPinValueChangedEvent(_button3Pin, PinEventTypes.Falling, OnButton3Pressed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"按钮初始化失败: {ex.Message}");
        }
    }

    private void OnButton1Pressed(object sender, PinValueChangedEventArgs e)
    {
        var now = DateTime.UtcNow;
        if (now - _lastButton1Press < _debounceInterval)
            return;
            
        _lastButton1Press = now;
        ButtonPressed?.Invoke(1);
        
        // 按钮1: 切换颜色
        CycleColor();
        Console.WriteLine($"按钮1按下 - 当前颜色: R:{_currentColor.R} G:{_currentColor.G} B:{_currentColor.B}");
    }

    private void OnButton2Pressed(object sender, PinValueChangedEventArgs e)
    {
        var now = DateTime.UtcNow;
        if (now - _lastButton2Press < _debounceInterval)
            return;
            
        _lastButton2Press = now;
        ButtonPressed?.Invoke(2);
        
        // 按钮2: 切换效果
        CycleEffect();
        Console.WriteLine($"按钮2按下 - 当前效果: {_currentEffect}");
    }

    private void OnButton3Pressed(object sender, PinValueChangedEventArgs e)
    {
        var now = DateTime.UtcNow;
        if (now - _lastButton3Press < _debounceInterval)
            return;
            
        _lastButton3Press = now;
        ButtonPressed?.Invoke(3);
        
        // 按钮3: 调整亮度
        CycleBrightness();
        Console.WriteLine($"按钮3按下 - 当前亮度: {_currentBrightness}%");
    }

    /// <summary>
    /// 设置LED效果
    /// </summary>
    public async Task SetEffect(LedControlRequest request)
    {
        // 停止当前效果
        StopCurrentEffect();
        
        _currentEffect = request.Effect;
        _currentColor = request.Color;
        _currentBrightness = Math.Clamp(request.Brightness, 0, 100);
        _currentSpeed = Math.Max(request.Speed, 50);
        
        // 启动新效果
        await StartEffect();
        
        NotifyStatusChanged();
        Console.WriteLine($"设置效果: {_currentEffect}, 颜色: R:{_currentColor.R} G:{_currentColor.G} B:{_currentColor.B}, 亮度: {_currentBrightness}%");
    }

    private void StopCurrentEffect()
    {
        _effectCancellationSource?.Cancel();
        _effectTask?.Wait(1000);
        _effectCancellationSource?.Dispose();
    }

    private async Task StartEffect()
    {
        _effectCancellationSource = new CancellationTokenSource();
        
        _effectTask = _currentEffect switch
        {
            LedEffect.Off => Task.Run(() => SetColor(LedColor.Black)),
            LedEffect.Static => Task.Run(() => SetColor(_currentColor)),
            LedEffect.Blink => Task.Run(() => BlinkEffect(_effectCancellationSource.Token)),
            LedEffect.Breathe => Task.Run(() => BreatheEffect(_effectCancellationSource.Token)),
            LedEffect.Rainbow => Task.Run(() => RainbowEffect(_effectCancellationSource.Token)),
            _ => Task.CompletedTask
        };
        
        await Task.Yield();
    }

    private void SetColor(LedColor color)
    {
        var brightness = _currentBrightness / 100.0;
        var red = (byte)(color.R * brightness);
        var green = (byte)(color.G * brightness);
        var blue = (byte)(color.B * brightness);

        if (_redChannel != null && _greenChannel != null && _blueChannel != null)
        {
            // 使用PWM
            _redChannel.DutyCycle = red / 255.0;
            _greenChannel.DutyCycle = green / 255.0;
            _blueChannel.DutyCycle = blue / 255.0;
        }
        else
        {
            // 使用GPIO (简单开关)
            try
            {
                _gpio.Write(18, red > 127 ? PinValue.High : PinValue.Low);
                _gpio.Write(19, green > 127 ? PinValue.High : PinValue.Low);
                _gpio.Write(20, blue > 127 ? PinValue.High : PinValue.Low);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GPIO写入失败: {ex.Message}");
            }
        }
    }

    private async Task BlinkEffect(CancellationToken cancellationToken)
    {
        Console.WriteLine("开始闪烁效果");
        while (!cancellationToken.IsCancellationRequested)
        {
            // 亮
            SetColor(_currentColor);
            await Task.Delay(_currentSpeed / 2, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested) break;
            
            // 灭
            SetColor(LedColor.Black);
            await Task.Delay(_currentSpeed / 2, cancellationToken);
        }
        Console.WriteLine("闪烁效果结束");
    }

    private async Task BreatheEffect(CancellationToken cancellationToken)
    {
        Console.WriteLine("开始呼吸灯效果");
        while (!cancellationToken.IsCancellationRequested)
        {
            // 渐亮阶段 - 从0%到100%
            for (int i = 0; i <= 100 && !cancellationToken.IsCancellationRequested; i += 1)
            {
                var breatheBrightness = (i / 100.0) * (_currentBrightness / 100.0);
                var color = new LedColor(
                    (byte)(_currentColor.R * breatheBrightness),
                    (byte)(_currentColor.G * breatheBrightness),
                    (byte)(_currentColor.B * breatheBrightness)
                );
                SetColor(color);
                await Task.Delay(_currentSpeed / 200, cancellationToken); // 更平滑的变化
            }
            
            // 短暂停留在最亮状态
            await Task.Delay(_currentSpeed / 10, cancellationToken);
            
            // 渐暗阶段 - 从100%到0%
            for (int i = 100; i >= 0 && !cancellationToken.IsCancellationRequested; i -= 1)
            {
                var breatheBrightness = (i / 100.0) * (_currentBrightness / 100.0);
                var color = new LedColor(
                    (byte)(_currentColor.R * breatheBrightness),
                    (byte)(_currentColor.G * breatheBrightness),
                    (byte)(_currentColor.B * breatheBrightness)
                );
                SetColor(color);
                await Task.Delay(_currentSpeed / 200, cancellationToken); // 更平滑的变化
            }
            
            // 短暂停留在最暗状态
            await Task.Delay(_currentSpeed / 10, cancellationToken);
        }
        Console.WriteLine("呼吸灯效果结束");
    }

    private async Task RainbowEffect(CancellationToken cancellationToken)
    {
        Console.WriteLine("开始彩虹效果");
        int hue = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var color = HsvToRgb(hue, 1.0, 1.0);
            SetColor(color);
            
            hue = (hue + 2) % 360; // 稍快一点的变化
            await Task.Delay(_currentSpeed / 180, cancellationToken); // 根据速度调整
        }
        Console.WriteLine("彩虹效果结束");
    }

    private LedColor HsvToRgb(double h, double s, double v)
    {
        double c = v * s;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double r, g, b;
        if (h < 60) { r = c; g = x; b = 0; }
        else if (h < 120) { r = x; g = c; b = 0; }
        else if (h < 180) { r = 0; g = c; b = x; }
        else if (h < 240) { r = 0; g = x; b = c; }
        else if (h < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        var brightness = _currentBrightness / 100.0;
        return new LedColor(
            (byte)((r + m) * 255 * brightness),
            (byte)((g + m) * 255 * brightness),
            (byte)((b + m) * 255 * brightness)
        );
    }

    private void CycleColor()
    {
        var colors = new[]
        {
            LedColor.Red, LedColor.Green, LedColor.Blue,
            LedColor.Yellow, LedColor.Cyan, LedColor.Magenta,
            LedColor.White, LedColor.Orange, LedColor.Purple
        };

        var currentIndex = Array.FindIndex(colors, c => 
            c.R == _currentColor.R && c.G == _currentColor.G && c.B == _currentColor.B);
        
        var nextIndex = (currentIndex + 1) % colors.Length;
        _currentColor = colors[nextIndex];
        
        // 如果当前是静态效果，立即更新颜色
        if (_currentEffect == LedEffect.Static)
        {
            SetColor(_currentColor);
        }
        
        NotifyStatusChanged();
    }

    private void CycleEffect()
    {
        var effects = LedEffect.All;
        var currentIndex = Array.IndexOf(effects, _currentEffect);
        var nextIndex = (currentIndex + 1) % effects.Length;
        
        _currentEffect = effects[nextIndex];
        
        // 异步重新启动效果
        Task.Run(async () =>
        {
            StopCurrentEffect();
            await StartEffect();
            NotifyStatusChanged();
        });
    }

    private void CycleBrightness()
    {
        var levels = new[] { 25, 50, 75, 100 };
        var currentIndex = Array.IndexOf(levels, _currentBrightness);
        var nextIndex = (currentIndex + 1) % levels.Length;
        
        _currentBrightness = levels[nextIndex];
        
        // 立即应用新的亮度设置
        if (_currentEffect == LedEffect.Static)
        {
            SetColor(_currentColor);
        }
        // 对于动态效果，亮度会在下次更新时生效
        
        NotifyStatusChanged();
    }

    public LedStatus GetStatus()
    {
        return new LedStatus
        {
            CurrentEffect = _currentEffect,
            CurrentColor = _currentColor,
            Brightness = _currentBrightness,
            Speed = _currentSpeed,
            IsRunning = _effectTask?.Status == TaskStatus.Running,
            AvailableColors = new[] { "Red", "Green", "Blue", "Yellow", "Cyan", "Magenta", "White", "Orange", "Purple" },
            AvailableEffects = LedEffect.All
        };
    }

    private void NotifyStatusChanged()
    {
        StatusChanged?.Invoke(GetStatus());
    }

    public void Dispose()
    {
        StopCurrentEffect();
        
        _redChannel?.Dispose();
        _greenChannel?.Dispose();
        _blueChannel?.Dispose();
        _gpio?.Dispose();
    }
}
