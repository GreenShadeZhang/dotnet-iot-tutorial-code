using System.Device.Gpio;
using System.Device.Pwm.Drivers;
using System.Device.Spi;

namespace Iot.Lcd;

public class LcdConfig : IDisposable
{
    protected GpioController _gpio;
    protected SpiDevice _spi;
    protected SoftwarePwmChannel _pwmBacklight;
    protected int RST_PIN;
    protected int DC_PIN;
    protected int BL_PIN;
    protected int BL_freq;

    public LcdConfig(SpiDevice spi, int spi_freq = 40000000, int rst = 27, int dc = 25, int bl = 18, int bl_freq = 1000)
    {
        _gpio = new GpioController();
        this._spi = spi;
        this.RST_PIN = rst;
        this.DC_PIN = dc;
        this.BL_PIN = bl;
        this.BL_freq = bl_freq;

        _gpio.OpenPin(RST_PIN, PinMode.Output);
        _gpio.OpenPin(DC_PIN, PinMode.Output);
        _gpio.OpenPin(BL_PIN, PinMode.Output);
        DigitalWrite(BL_PIN, false);

        if (spi != null)
        {
            spi.ConnectionSettings.ClockFrequency = spi_freq;
            spi.ConnectionSettings.Mode = SpiMode.Mode0;
        }

        _pwmBacklight = new SoftwarePwmChannel(pinNumber: bl, frequency: bl_freq);
        _pwmBacklight.Start();
    }

    public void DigitalWrite(int pin, bool value)
    {
        _gpio.Write(pin, value ? PinValue.High : PinValue.Low);
    }

    public bool DigitalRead(int pin)
    {
        return _gpio.Read(pin) == PinValue.High;
    }

    public void DelayMs(int delaytime)
    {
        Thread.Sleep(delaytime);
    }

    public void SpiWriteByte(byte[] data)
    {
        _spi.Write(data);
    }

    public void BlDutyCycle(double duty)
    {
        _pwmBacklight.DutyCycle = duty / 100;
        // Implement PWM control for backlight if needed
    }

    public void BlFrequency(int freq)
    {
        _pwmBacklight.Frequency = freq;
        // Implement frequency control for backlight if needed
    }

    public void ModuleInit()
    {
        if (_spi != null)
        {
            _spi.ConnectionSettings.ClockFrequency = _spi.ConnectionSettings.ClockFrequency;
            _spi.ConnectionSettings.Mode = SpiMode.Mode0;
        }
    }

    public void ModuleExit()
    {
        Console.WriteLine("spi end");
        if (_spi != null)
        {
            _spi.Dispose();
        }

        Console.WriteLine("gpio cleanup...");
        DigitalWrite(RST_PIN, true);
        DigitalWrite(DC_PIN, false);
        _gpio.ClosePin(BL_PIN);
        Thread.Sleep(1);
        _gpio?.Dispose();
    }

    public void Dispose()
    {
        Console.WriteLine("spi end");
        if (_spi != null)
        {
            _spi.Dispose();
        }

        Console.WriteLine("gpio cleanup...");
        DigitalWrite(RST_PIN, true);
        DigitalWrite(DC_PIN, false);
        _gpio.ClosePin(BL_PIN);
        Thread.Sleep(1);
        _gpio?.Dispose();
    }
}