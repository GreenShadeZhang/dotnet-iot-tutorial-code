using Iot.Device.ST7789V3;
using LedMatrix.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Textures.PixelFormats;
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

// SPI0 CS0
SpiConnectionSettings senderSettings1 = new(0, 1)
{
    ClockFrequency = ST7789V3.SpiClockFrequency,
    Mode = ST7789V3.SpiMode
};

using SpiDevice senderDevice1 = SpiDevice.Create(senderSettings1);


var lcd1 = new ST7789V3(dataCommandPin, senderDevice1, resetPin, pwmChannel, shouldDispose: false);

lcd.Reset();
lcd.Init();
lcd.SetWindows(0, 0, 172, 320);

lcd.Clear();

//lcd1.Reset();
lcd1.Init();
lcd1.SetWindows(0, 0, 172, 320);

lcd1.Clear();

var imageFilePath = "./Pic/excited.png";
//var imageFilePath = "./Pic/LCD_1inch47.jpg";

using (Image<Bgra32> image = Image.Load<Bgra32>(imageFilePath))
{
    using (Image<Bgr24> convertedImage = image.CloneAs<Bgr24>())
    {
        var dataList = Helper.GetImageBytes(convertedImage);

        while (true)
        {
            lcd.SpiWrite(true, new ReadOnlySpan<byte>(dataList));
            lcd1.SpiWrite(true, new ReadOnlySpan<byte>(dataList));
        }
        //lcd.SpiWrite(true, new ReadOnlySpan<byte>(dataList));

        //Thread.Sleep(3000);

        //lcd.Clear();

        //Thread.Sleep(3000);

        //lcd.SpiWrite(true, new ReadOnlySpan<byte>(dataList));
    }
}

//var dataList = Helper.ConvertImageToRGB565ByteArray(imageFilePath);

//var dataList = new byte[172 * 320 * 2];

//var image = Image.Load<Rgb565>(imageFilePath);

//image.CopyPixelDataTo(dataList);

//var image2 = Image.LoadPixelData<Bgr565>(dataList, 172, 320);

//await image2.SaveAsJpegAsync("/Pic/LCD_1inch47-01.jpg");

//var dataList = image.ImageToRgb565(image.Width, image.Height);

//lcd.SpiWrite(true, new ReadOnlySpan<byte>(dataList));

//Thread.Sleep(3000);

//lcd.Clear();

//Thread.Sleep(3000);

//lcd.SpiWrite(true, new ReadOnlySpan<byte>(dataList));

//var resultString = $"var bitmap = new byte[] {{{String.Join(",", dataList.Select(b => $"0x{b.ToString("X2")}"))}}}";
//Console.WriteLine(resultString);
Console.ReadKey();
