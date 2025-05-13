using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verdure.Iot.Device;

namespace DualDisplayLottieExample;

public class DualLottiePlayer
{
    private readonly ST7789Display _display1;
    private readonly ST7789Display _display2;
    private readonly int _display1Width;
    private readonly int _display1Height;
    private readonly int _display2Width;
    private readonly int _display2Height;
    private readonly LottieRenderer _lottieRenderer;

    // 用于控制停止播放
    private CancellationTokenSource _cancellationTokenSource;

    public DualLottiePlayer(ST7789Display display1, int display1Width, int display1Height,
                           ST7789Display display2, int display2Width, int display2Height,
                           string lottieFilePath)
    {
        _display1 = display1;
        _display2 = display2;
        _display1Width = display1Width;
        _display1Height = display1Height;
        _display2Width = display2Width;
        _display2Height = display2Height;

        // 初始化Lottie渲染器
        _lottieRenderer = new LottieRenderer(lottieFilePath);
    }

    // 同时播放动画到两个屏幕
    public async Task PlayAnimationAsync(int loops = -1, int fps = 30)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        var totalFrames = _lottieRenderer.FrameCount;
        int frameDurationMs = 1000 / fps;
        int currentLoop = 0;

        try
        {
            while ((loops == -1 || currentLoop < loops) && !token.IsCancellationRequested)
            {
                for (int frame = 0; frame < totalFrames && !token.IsCancellationRequested; frame++)
                {
                    // 记录开始时间用于帧率控制
                    var startTime = DateTime.Now;

                    // 预先渲染两个屏幕的帧到内存
                    byte[] frameData1 = _lottieRenderer.RenderFrame(frame, _display1Width, _display1Height);
                    byte[] frameData2 = _lottieRenderer.RenderFrame(frame, _display2Width, _display2Height);

                    // 顺序更新屏幕 (避免SPI冲突)
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        await Task.Run(() =>
                        {
                            // 完全更新第一个屏幕
                            _display1.SetAddressWindow(0, 0, _display1Width - 1, _display1Height - 1);
                            _display1.SendData(frameData1);
                            // 添加少量延时确保CS信号稳定
                            Thread.Sleep(2);

                            // 完全更新第二个屏幕
                            _display2.SetAddressWindow(0, 0, _display2Width - 1, _display2Height - 1);
                            _display2.SendData(frameData2);
                            Thread.Sleep(2);
                        });
                    }
                 
                    // 计算需要等待的时间以保持帧率
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    var delay = frameDurationMs - elapsed;

                    if (delay > 0)
                    {
                        await Task.Delay((int)delay, token);
                    }
                }

                if (loops != -1)
                    currentLoop++;
            }
        }
        catch (TaskCanceledException)
        {
            // 正常取消，不需要处理
        }
        catch (Exception ex)
        {
            Console.WriteLine($"动画播放错误: {ex.Message}");
        }
    }

    // 停止动画播放
    public void StopAnimation()
    {
        _cancellationTokenSource?.Cancel();
    }
}