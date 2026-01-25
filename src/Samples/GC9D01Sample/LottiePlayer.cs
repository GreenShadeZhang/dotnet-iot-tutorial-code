using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Verdure.Iot.Device;

public class LottiePlayer
{
    private readonly GC9D01Display _display;
    private readonly int _width;
    private readonly int _height;
    private readonly LottieRenderer _lottieRenderer;
    private CancellationTokenSource? _cancellationTokenSource;

    public LottiePlayer(GC9D01Display display, int width, int height, string lottieFilePath)
    {
        _display = display;
        _width = width;
        _height = height;
        _lottieRenderer = new LottieRenderer(lottieFilePath);
    }

    public async Task PlayAnimationAsync(int loops = -1, int fps = 30)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        var totalFrames = (int)_lottieRenderer.FrameCount;
        int frameDurationMs = 1000 / fps;
        int currentLoop = 0;
        try
        {
            while ((loops == -1 || currentLoop < loops) && !token.IsCancellationRequested)
            {
                for (int frame = 0; frame < totalFrames && !token.IsCancellationRequested; frame++)
                {
                    var startTime = DateTime.Now;
                    byte[] frameData = _lottieRenderer.RenderFrame(frame, _width, _height);
                    _display.FillScreen(0x0000); // 清屏
                    _display.SetAddressWindow(0, 0, _width - 1, _height - 1);
                    _display.SendData(frameData);
                    var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                    int delay = frameDurationMs - (int)elapsed;
                    if (delay > 0) Thread.Sleep(delay);
                }
                currentLoop++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lottie动画播放错误: {ex.Message}");
        }
    }

    public void StopAnimation()
    {
        _cancellationTokenSource?.Cancel();
    }
}
