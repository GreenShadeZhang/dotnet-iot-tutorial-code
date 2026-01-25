using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;

namespace Verdure.Iot.Device;

public class GC9D01Display : IDisposable
{
    private readonly SpiDevice _spiDevice;
    private readonly GpioController _gpio;
    private readonly int _dcPin;
    private readonly int _resetPin;
    private readonly int _csPin;
    private const int MAX_TRANSFER_SIZE = 4096;
    private readonly int _width;
    private readonly int _height;

    public int Width => _width;
    public int Height => _height;

    public GC9D01Display(SpiConnectionSettings settings, GpioController gpio, int dcPin, int resetPin, int csPin = -1)
    {
        _spiDevice = SpiDevice.Create(settings);
        _gpio = gpio;
        _dcPin = dcPin;
        _resetPin = resetPin;
        _csPin = csPin;
        _width = 160;
        _height = 160;

        // 初始化GPIO引脚
        gpio.OpenPin(_dcPin, PinMode.Output);
        gpio.OpenPin(_resetPin, PinMode.Output);
        if (_csPin >= 0)
        {
            gpio.OpenPin(_csPin, PinMode.Output);
            gpio.Write(_csPin, PinValue.High);
        }
        Reset();
        Initialize();
    }
    /// <summary>
    /// 设置显示区域（窗口）
    /// </summary>
    public void SetAddressWindow(int x0, int y0, int x1, int y1)
    {
        SendCommand(0x2A);
        SendData((byte)(x0 >> 8));
        SendData((byte)(x0 & 0xFF));
        SendData((byte)(x1 >> 8));
        SendData((byte)(x1 & 0xFF));
        SendCommand(0x2B);
        SendData((byte)(y0 >> 8));
        SendData((byte)(y0 & 0xFF));
        SendData((byte)(y1 >> 8));
        SendData((byte)(y1 & 0xFF));
        SendCommand(0x2C);
    }

    /// <summary>
    /// 填充全屏纯色
    /// </summary>
    public void FillScreen(ushort color)
    {
        SetAddressWindow(0, 0, _width - 1, _height - 1);
        int totalPixels = _width * _height;
        int bufferSize = Math.Min(totalPixels * 2, 32768); // 最大32KB
        byte[] buffer = new byte[bufferSize];
        for (int i = 0; i < buffer.Length; i += 2)
        {
            buffer[i] = (byte)(color >> 8);
            buffer[i + 1] = (byte)(color & 0xFF);
        }
        int bytesWritten = 0;
        int totalBytes = totalPixels * 2;
        while (bytesWritten < totalBytes)
        {
            int bytesToWrite = Math.Min(buffer.Length, totalBytes - bytesWritten);
            SendData(buffer.AsSpan(0, bytesToWrite));
            bytesWritten += bytesToWrite;
        }
    }

    public void Reset()
    {
        _gpio.Write(_resetPin, PinValue.High);
        Thread.Sleep(50);
        _gpio.Write(_resetPin, PinValue.Low);
        Thread.Sleep(50);
        _gpio.Write(_resetPin, PinValue.High);
        Thread.Sleep(120);
    }

    public void Initialize()
    {
        SendCommand(0xFE);
        SendCommand(0xEF);

        for (byte reg = 0x80; reg <= 0x8F; reg++)
        {
            SendCommand(reg);
            SendData(0xFF);
        }
        SendCommand(0x90); SendData(0xFF); // 0x90-0x97未用
        SendCommand(0x91); SendData(0xFF);
        SendCommand(0x92); SendData(0xFF);
        SendCommand(0x93); SendData(0xFF);
        SendCommand(0x94); SendData(0xFF);
        SendCommand(0x95); SendData(0xFF);
        SendCommand(0x96); SendData(0xFF);
        SendCommand(0x97); SendData(0xFF);

        SendCommand(0x3A); SendData(0x05);
        SendCommand(0xEC); SendData(0x01);

        SendCommand(0x74);
        SendData(0x02); SendData(0x0E); SendData(0x00); SendData(0x00); SendData(0x00); SendData(0x00); SendData(0x00);

        SendCommand(0x98); SendData(0x3E);
        SendCommand(0x99); SendData(0x3E);

        SendCommand(0xB5); SendData(0x0D); SendData(0x0D);

        SendCommand(0x60); SendData(0x38); SendData(0x0F); SendData(0x79); SendData(0x67);
        SendCommand(0x61); SendData(0x38); SendData(0x11); SendData(0x79); SendData(0x67);

        SendCommand(0x64); SendData(0x38); SendData(0x17); SendData(0x71); SendData(0x5F); SendData(0x79); SendData(0x67);
        SendCommand(0x65); SendData(0x38); SendData(0x13); SendData(0x71); SendData(0x5B); SendData(0x79); SendData(0x67);

        SendCommand(0x6A); SendData(0x00); SendData(0x00);

        SendCommand(0x6C); SendData(0x22); SendData(0x02); SendData(0x22); SendData(0x02); SendData(0x22); SendData(0x22); SendData(0x50);

        SendCommand(0x6E);
        SendData(0x03); SendData(0x03); SendData(0x01); SendData(0x01); SendData(0x00); SendData(0x00);
        SendData(0x0f); SendData(0x0f); SendData(0x0d); SendData(0x0d); SendData(0x0b); SendData(0x0b);
        SendData(0x09); SendData(0x09); SendData(0x00); SendData(0x00); SendData(0x00); SendData(0x00);
        SendData(0x0a); SendData(0x0a); SendData(0x0c); SendData(0x0c); SendData(0x0e); SendData(0x0e);
        SendData(0x10); SendData(0x10); SendData(0x00); SendData(0x00); SendData(0x02); SendData(0x02);
        SendData(0x04); SendData(0x04);

        SendCommand(0xBF); SendData(0x01);
        SendCommand(0xF9); SendData(0x40);
        SendCommand(0x9B); SendData(0x3B);
        SendCommand(0x93); SendData(0x33); SendData(0x7F); SendData(0x00);
        SendCommand(0x7E); SendData(0x30);
        SendCommand(0x70); SendData(0x0D); SendData(0x02); SendData(0x08); SendData(0x0D); SendData(0x02); SendData(0x08);
        SendCommand(0x71); SendData(0x0D); SendData(0x02); SendData(0x08);
        SendCommand(0x91); SendData(0x0E); SendData(0x09);
        SendCommand(0xC3); SendData(0x18);
        SendCommand(0xC4); SendData(0x18);
        SendCommand(0xC9); SendData(0x3C);
        SendCommand(0xF0); SendData(0x13); SendData(0x15); SendData(0x04); SendData(0x05); SendData(0x01); SendData(0x38);
        SendCommand(0xF2); SendData(0x13); SendData(0x15); SendData(0x04); SendData(0x05); SendData(0x01); SendData(0x34);
        SendCommand(0xF1); SendData(0x4B); SendData(0xB8); SendData(0x7B); SendData(0x34); SendData(0x35); SendData(0xEF);
        SendCommand(0xF3); SendData(0x47); SendData(0xB4); SendData(0x72); SendData(0x34); SendData(0x35); SendData(0xDA);
        SendCommand(0x36); SendData(0x00);
        SendCommand(0x11); Thread.Sleep(200);
        SendCommand(0x29);
        SendCommand(0x2C);
    }

    public void EnterSleep()
    {
        SendCommand(0x28);
        Thread.Sleep(120);
        SendCommand(0x10);
        Thread.Sleep(50);
    }

    public void ExitSleep()
    {
        SendCommand(0x11);
        Thread.Sleep(120);
        SendCommand(0x29);
    }

    public void SendCommand(byte command)
    {
        if (_csPin >= 0)
        {
            _gpio.Write(_csPin, PinValue.Low);
        }
        _gpio.Write(_dcPin, PinValue.Low);
        _spiDevice.Write(new[] { command });
        if (_csPin >= 0)
        {
            _gpio.Write(_csPin, PinValue.High);
        }
        Thread.Sleep(1);
    }

    public void SendData(byte data)
    {
        if (_csPin >= 0)
        {
            _gpio.Write(_csPin, PinValue.Low);
        }
        _gpio.Write(_dcPin, PinValue.High);
        _spiDevice.Write(new[] { data });
        if (_csPin >= 0)
        {
            _gpio.Write(_csPin, PinValue.High);
        }
    }

    public void SendData(byte[] data)
    {
        SendData((ReadOnlySpan<byte>)data);
    }

    public void SendData(ReadOnlySpan<byte> data)
    {
        if (_csPin >= 0)
        {
            _gpio.Write(_csPin, PinValue.Low);
        }
        _gpio.Write(_dcPin, PinValue.High);
        for (int i = 0; i < data.Length; i += MAX_TRANSFER_SIZE)
        {
            int length = Math.Min(MAX_TRANSFER_SIZE, data.Length - i);
            _spiDevice.Write(data.Slice(i, length).ToArray());
        }
        if (_csPin >= 0)
        {
            _gpio.Write(_csPin, PinValue.High);
        }
    }

    public void Dispose()
    {
        _spiDevice?.Dispose();
    }
}
