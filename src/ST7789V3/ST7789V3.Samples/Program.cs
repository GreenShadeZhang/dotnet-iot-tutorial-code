using Iot.Device.ST7789V3;
using LedMatrix.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Device.Pwm.Drivers;
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

var pwmChannel = new SoftwarePwmChannel(pinNumber: backlightPin, frequency: blFreq);

var lcd = new ST7789V3(dataCommandPin, senderDevice, resetPin, pwmChannel, shouldDispose: false);

lcd.Reset();
lcd.Init();
lcd.SetWindows(0, 0, 172, 320);

var imageFilePath = "./Pic/LCD_1inch47.jpg";
//var imageFilePath = "./Pic/verdure90.png";

var image = Image.Load<Bgr565>(imageFilePath);

//var dataList = new byte[172 * 320 * 2];

//image.CopyPixelDataTo(dataList);

var dataList = image.ImageToRgb565(image.Width, image.Height);

lcd.SpiWrite(true, new ReadOnlySpan<byte>(dataList));


//var resultString = $"var bitmap = new byte[] {{{String.Join(",", dataList.Select(b => $"0x{b.ToString("X2")}"))}}}";
//Console.WriteLine(resultString);
Console.ReadKey();
