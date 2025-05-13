using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Verdure.Iot.Device;

public class ST7789Display : IDisposable
{
    private readonly SpiDevice _spiDevice;
    private readonly GpioController _gpio;
    private readonly int _dcPin;
    private readonly int _resetPin;
    private readonly int _csPin;
    private readonly int _width;
    private readonly int _height;
    private readonly int _xOffset;
    private readonly int _yOffset;
    private readonly bool _isRgbPanel;

    // 构造函数支持不同尺寸屏幕的配置参数
    public ST7789Display(SpiConnectionSettings settings, int dcPin, int resetPin, int csPin,
                        DisplayType displayType = DisplayType.Display24Inch)
    {
        _spiDevice = SpiDevice.Create(settings);
        _gpio = new GpioController();
        _dcPin = dcPin;
        _resetPin = resetPin;
        _csPin = csPin;

        // 初始化GPIO引脚
        _gpio.OpenPin(_dcPin, PinMode.Output);
        _gpio.OpenPin(_resetPin, PinMode.Output);
        _gpio.OpenPin(_csPin, PinMode.Output);

        // 根据屏幕类型设置参数
        switch (displayType)
        {
            case DisplayType.Display24Inch:
                _width = 320;
                _height = 240;
                _xOffset = 0;
                _yOffset = 0;
                _isRgbPanel = true;
                break;

            case DisplayType.Display147Inch:
                _width = 172;
                _height = 320;
                _xOffset = 34;  // 可能需要根据实际情况调整
                _yOffset = 0;
                _isRgbPanel = true;
                break;

            default:
                throw new ArgumentException("不支持的显示屏类型");
        }

        Initialize();
    }

    // 初始化显示屏
    private void Initialize()
    {
        // 硬件复位
        HardReset();

        // 发送初始化命令序列
        SendCommand(0x01);    // Software Reset
        Thread.Sleep(150);

        SendCommand(0x11);    // Sleep Out
        Thread.Sleep(120);

        SendCommand(0x3A);    // COLMOD: Pixel Format Set
        SendData(0x55);       // 16-bit/pixel

        // 根据面板类型发送不同的内存访问控制命令
        SendCommand(0x36);    // MADCTL: Memory Data Access Control
        SendData(_isRgbPanel ? (byte)0x70 : (byte)0x00);

        // 设置显示区域
        SetAddressWindow(0, 0, _width - 1, _height - 1);

        SendCommand(0x29);    // Display On
        Thread.Sleep(20);
    }

    // 硬件复位
    public void HardReset()
    {
        _gpio.Write(_resetPin, PinValue.High);
        Thread.Sleep(10);
        _gpio.Write(_resetPin, PinValue.Low);
        Thread.Sleep(10);
        _gpio.Write(_resetPin, PinValue.High);
        Thread.Sleep(120);
    }

    // 发送命令
    public void SendCommand(byte command)
    {
        _gpio.Write(_csPin, PinValue.Low);
        _gpio.Write(_dcPin, PinValue.Low);  // DC低电平表示命令
        _spiDevice.Write(new[] { command });
        _gpio.Write(_csPin, PinValue.High);
        // 添加短暂延时以确保命令被处理
        Thread.Sleep(1);
    }

    // 发送数据
    public void SendData(byte data)
    {
        _gpio.Write(_csPin, PinValue.Low);
        _gpio.Write(_dcPin, PinValue.High); // DC高电平表示数据
        _spiDevice.Write(new[] { data });
        _gpio.Write(_csPin, PinValue.High);
    }

    // 发送数据数组
    public void SendData(byte[] data)
    {
        _gpio.Write(_csPin, PinValue.Low);
        _gpio.Write(_dcPin, PinValue.High);
        _spiDevice.Write(data);
        _gpio.Write(_csPin, PinValue.High);
    }

    // 设置显示区域
    public void SetAddressWindow(int x0, int y0, int x1, int y1)
    {
        x0 += _xOffset;
        y0 += _yOffset;
        x1 += _xOffset;
        y1 += _yOffset;

        // 列地址设置
        SendCommand(0x2A);
        SendData((byte)(x0 >> 8));
        SendData((byte)x0);
        SendData((byte)(x1 >> 8));
        SendData((byte)x1);

        // 行地址设置
        SendCommand(0x2B);
        SendData((byte)(y0 >> 8));
        SendData((byte)y0);
        SendData((byte)(y1 >> 8));
        SendData((byte)y1);

        // 写入RAM
        SendCommand(0x2C);
    }

    // 显示图像
    public void DrawImage(byte[] imageData)
    {
        SetAddressWindow(0, 0, _width - 1, _height - 1);
        SendData(imageData);
    }

    // 填充纯色
    public void FillScreen(ushort color)
    {
        SetAddressWindow(0, 0, _width - 1, _height - 1);

        int bufferSize = _width * _height * 2;
        byte[] buffer = new byte[bufferSize];

        for (int i = 0; i < bufferSize; i += 2)
        {
            buffer[i] = (byte)(color >> 8);
            buffer[i + 1] = (byte)(color & 0xFF);
        }

        SendData(buffer);
    }

    public void Dispose()
    {
        _spiDevice?.Dispose();
        _gpio?.Dispose();
    }
}

public enum DisplayType
{
    Display24Inch,
    Display147Inch
}
