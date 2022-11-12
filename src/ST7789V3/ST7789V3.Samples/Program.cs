using Iot.Device.ST7789V3;
using System.Device.Pwm;
using System.Device.Spi;

var resetPin = 27;
var dataCommandPin = 25;
var backlightPin = 18;
var blFreq = 1000;
// SPI0 CS0
SpiConnectionSettings senderSettings = new(0, 0)
{
    ClockFrequency = ST7789V3.SpiClockFrequency,
    Mode = ST7789V3.SpiMode
};

using SpiDevice senderDevice = SpiDevice.Create(senderSettings);

var pwmChannel = PwmChannel.Create(backlightPin, blFreq);

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
